using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RPG.Nodes;
using RPG.Nodes.Base;
using RPG.Utility.Editor;
using UnityEditor;
using UnityEngine;

namespace RPG.Editor.Nodes
{
    public partial class NodeEditorWindow
    {
        private const string CLEAR_CONNECTIONS_TEXT = "Clear Connections";
        private const string SEND_TO_FRONT_TEXT = "Send to Front";
        private const string SEND_TO_BACK_TEXT = "Send to Back";
        private const string SEND_FORWARD_TEXT = "Send Forward";
        private const string SEND_BACKWARD_TEXT = "Send Backward";
        private const string REMOVE_TEXT = "Remove _del";
        private const string DUPLICATE_TEXT = "Duplicate %d";
        private const string RENAME_TEXT = "Rename _F2";

        private class NodePortSelector
        {
            public Node node = null;
            public int actualIndex = -1;
            public OutputPort output = null;
        }

        #region Context Menus
        private void ShowPortContextMenu(Port port)
        {
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent(CLEAR_CONNECTIONS_TEXT), false, port.ClearConnections);

            InputPort input = port as InputPort;
            if (input != null)
            {
                List<NodePortSelector> namedNodes = new List<NodePortSelector>();
                List<NodePortSelector> unnamedNodes = new List<NodePortSelector>();

                for (int i = 0; i < input.ConnectionsCount; i++)
                {
                    Connection connection = input.GetConnection(i);
                    if (connection == null) continue;
                    OutputPort output = connection.Start;
                    if (output == null) continue;
                    Node node = output.Node;
                    if (node == null) continue;

                    NodePortSelector nps = new NodePortSelector() { actualIndex = i, node = node, output = output };
                    if (node.name == ObjectNames.NicifyVariableName(node.GetType().Name))
                        unnamedNodes.Add(nps);
                    else namedNodes.Add(nps);
                }

                if (namedNodes.Count != 0 || unnamedNodes.Count != 0)
                {
                    contextMenu.AddSeparator(string.Empty);

                    //TODO: Test this
                    foreach (NodePortSelector namedNode in namedNodes)
                    {
                        NodePortSelector captured = namedNode;
                        contextMenu.AddItem(new GUIContent(string.Format("Select \"{0}\" port.", namedNode.node.name)),
                            false,
                            () =>
                            {
                                HoveredPort = captured.output;
                                DraggedPort = captured.output;
                            });
                    }

                    foreach (NodePortSelector unnamedNode in unnamedNodes)
                    {
                        NodePortSelector captured = unnamedNode;
                        contextMenu.AddItem(
                            new GUIContent(string.Format("Select \"{0} {1}\" port.",
                                unnamedNode.node.name, unnamedNode.actualIndex)),
                            false,
                            () =>
                            {
                                HoveredPort = captured.output;
                                DraggedPort = captured.output;
                            });
                    }
                }
            }

            AddCustomContextMenuItems(contextMenu, port);
            contextMenu.DropDown(new Rect(_mousePosition, Vector2.zero));
            EditorUtilities.AutoSaveAssets();
        }

        private void ShowConnectionContextMenu()
        {
            GenericMenu contextMenu = new GenericMenu();
            bool oneConnectionSelected = Selection.objects.Length == 1 && Selection.activeObject is Connection;
            Connection connection = oneConnectionSelected ? (Connection)Selection.activeObject : null;

            if (oneConnectionSelected)
            {
                contextMenu.AddItem(new GUIContent(SEND_TO_FRONT_TEXT), false, () => SendConnectionToFront(connection));
                contextMenu.AddItem(new GUIContent(SEND_TO_BACK_TEXT), false, () => SendConnectionToBack(connection));
                contextMenu.AddItem(new GUIContent(SEND_FORWARD_TEXT), false, () => SendConnectionForward(connection));
                contextMenu.AddItem(new GUIContent(SEND_BACKWARD_TEXT), false, () => SendConnectionBackward(connection));
                contextMenu.AddSeparator(string.Empty);

                var modTypes = ReflectionUtilities.GetDerivedTypes<ConnectionModifier>();
                if (modTypes.Length != 0)
                {
                    foreach (Type modType in modTypes)
                    {
                        var type = modType;
                        contextMenu.AddItem(
                            new GUIContent("Add Connection Modifier/" + ObjectNames.NicifyVariableName(modType.Name)),
                            false, () => AddConnectionModifierToConnection(connection, type));
                    }
                }
            }

            contextMenu.AddItem(new GUIContent(REMOVE_TEXT), false, RemoveSelectedConnections);
            if (oneConnectionSelected)AddCustomContextMenuItems(contextMenu, connection);
            contextMenu.DropDown(new Rect(_mousePosition, Vector2.zero));
        }

        private void ShowConnectionModifierContextMenu()
        {
            GenericMenu contextMenu = new GenericMenu();
            bool oneConnectionSelected = Selection.objects.Length == 1 && Selection.activeObject is ConnectionModifier;
            ConnectionModifier modifier = oneConnectionSelected ? (ConnectionModifier)Selection.activeObject : null;

            contextMenu.AddItem(new GUIContent(DUPLICATE_TEXT), false, DuplicateSelectedConnectionModifiers);
            contextMenu.AddItem(new GUIContent(REMOVE_TEXT), false, RemoveSelectedConnectionModifiers);
            if (oneConnectionSelected) AddCustomContextMenuItems(contextMenu, modifier);
            contextMenu.DropDown(new Rect(_mousePosition, Vector2.zero));
        }

        private void ShowNodeContextMenu()
        {
            GenericMenu contextMenu = new GenericMenu();
            bool oneNodeSelected = Selection.objects.Length == 1 && Selection.activeObject is Node;
            Node node = oneNodeSelected ? (Node)Selection.activeObject : null;

            if (oneNodeSelected)
            {
                contextMenu.AddItem(new GUIContent(SEND_TO_FRONT_TEXT), false, () => SendNodeToFront(node));
                contextMenu.AddItem(new GUIContent(SEND_TO_BACK_TEXT), false, () => SendNodeToBack(node));
                contextMenu.AddItem(new GUIContent(SEND_FORWARD_TEXT), false, () => SendNodeForward(node));
                contextMenu.AddItem(new GUIContent(SEND_BACKWARD_TEXT), false, () => SendNodeBackward(node));
                contextMenu.AddSeparator(string.Empty);
                contextMenu.AddItem(new GUIContent(RENAME_TEXT), false, RenameSelectedNode);
            }

            contextMenu.AddItem(new GUIContent(DUPLICATE_TEXT), false, DuplicateSelectedNodes);
            contextMenu.AddItem(new GUIContent(REMOVE_TEXT), false, RemoveSelectedNodes);

            if (oneNodeSelected)AddCustomContextMenuItems(contextMenu, node);

            contextMenu.DropDown(new Rect(_mousePosition, Vector2.zero));
            EditorUtilities.AutoSaveAssets();
        }

        private void ShowGraphContextMenu()
        {
            GenericMenu contextMenu = new GenericMenu();
            Vector2 mousePosition = _mousePosition;

            foreach (Type type in NodeTypes)
            {
                string path = GraphEditor.GetNodeMenuName(type);
                if (string.IsNullOrEmpty(path)) continue;
                Type capturedType = type;
                contextMenu.AddItem(new GUIContent(path), false, () =>
                {
                    CreateNode(capturedType, WindowToGridPosition(mousePosition));
                });
            }

            //contextMenu.AddSeparator(string.Empty);
            //contextMenu.AddItem(new GUIContent("Preferences"), false, OpenPreferences);
            AddCustomContextMenuItems(contextMenu, Graph);
            contextMenu.DropDown(new Rect(mousePosition, Vector2.zero));
        }

        private void ShowDifferentObjectsContextMenu()
        {
            GenericMenu contextMenu = new GenericMenu();
            Vector2 mousePosition = _mousePosition;
            
            contextMenu.AddItem(new GUIContent(DUPLICATE_TEXT), false, DuplicateDifferentSelectedObjects);
            contextMenu.AddItem(new GUIContent(REMOVE_TEXT), false, RemoveDifferentSelectedObjects);

            contextMenu.DropDown(new Rect(mousePosition, Vector2.zero));
        }

        private static void AddCustomContextMenuItems(GenericMenu contextMenu, object obj)
        {
            KeyValuePair<ContextMenu, MethodInfo>[] items = ReflectionUtilities.GetAttributeMethods<ContextMenu>(obj);

            //#if UNITY_5_5_OR_NEWER
            var list = items.ToList();
            list.Sort((x, y) => x.Key.priority.CompareTo(y.Key.priority));
            items = list.ToArray();
            //#endif

            if (items.Length == 0) return;
            contextMenu.AddSeparator(string.Empty);
            foreach (var kvp in items)
            {
                var kvpCopy = kvp;
                contextMenu.AddItem(new GUIContent(kvp.Key.menuItem), false, () => kvpCopy.Value.Invoke(obj, null));
            }
        }
        #endregion
    }
}
