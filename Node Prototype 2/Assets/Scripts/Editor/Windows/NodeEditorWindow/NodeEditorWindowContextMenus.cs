using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RPG.Nodes;
using UnityEditor;
using UnityEngine;

namespace RPG.Editor.Nodes
{
    public partial class NodeEditorWindow
    {
        private class NodePortSelector
        {
            public Node node = null;
            public int actualIndex = -1;
            public OutputPort output = null;
        }

        #region Context Menu Manipulation
        private void ShowPortContextMenu(Port port)
        {
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Clear Connections"), false, port.ClearConnections);

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
                    contextMenu.AddSeparator("");

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
            NodeUtility.AutoSaveAssets();
        }

        private void ShowConnectionContextMenu()
        {
            GenericMenu contextMenu = new GenericMenu();
            bool oneConnectionSelected = Selection.objects.Length == 1 && Selection.activeObject is Connection;
            Connection connection = oneConnectionSelected ? (Connection)Selection.activeObject : null;

            if (oneConnectionSelected)
            {
                contextMenu.AddItem(new GUIContent("Move To Top"), false, () => SendConnectionToFront(connection));
            }

            contextMenu.AddItem(new GUIContent("Remove"), false, RemoveSelectedConnections);

            if (oneConnectionSelected)
            {
                AddCustomContextMenuItems(contextMenu, connection);
            }

            contextMenu.DropDown(new Rect(_mousePosition, Vector2.zero));
        }

        private void ShowNodeContextMenu()
        {
            GenericMenu contextMenu = new GenericMenu();
            bool oneNodeSelected = Selection.objects.Length == 1 && Selection.activeObject is Node;
            Node node = oneNodeSelected ? (Node)Selection.activeObject : null;

            if (oneNodeSelected)
            {
                contextMenu.AddItem(new GUIContent("Move To Top"), false, () => SendNodeToFront(node));
                contextMenu.AddItem(new GUIContent("Rename"), false, RenameSelectedNode);
            }

            contextMenu.AddItem(new GUIContent("Duplicate"), false, DuplicateSelectedNodes);
            contextMenu.AddItem(new GUIContent("Remove"), false, RemoveSelectedNodes);

            if (oneNodeSelected)
            {
                AddCustomContextMenuItems(contextMenu, node);
            }

            contextMenu.DropDown(new Rect(_mousePosition, Vector2.zero));
            NodeUtility.AutoSaveAssets();
        }

        private void ShowGraphContextMenu()
        {
            GenericMenu contextMenu = new GenericMenu();
            Vector2 mousePosition = _mousePosition;
            //Vector2 mousePosition = WindowToGridPosition(_mousePosition);

            foreach (Type type in NodeTypes)
            {
                string path = GraphEditor.GetNodeMenuName(type);
                if (string.IsNullOrEmpty(path)) continue;
                Type capturedType = type;
                contextMenu.AddItem(new GUIContent(path), false, () =>
                {
                    CreateNode(capturedType, mousePosition);
                });
            }

            //contextMenu.AddSeparator("");
            //contextMenu.AddItem(new GUIContent("Preferences"), false, OpenPreferences);
            AddCustomContextMenuItems(contextMenu, Graph);
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

            contextMenu.AddSeparator("");
            foreach (var kvp in items)
            {
                var kvp1 = kvp;
                contextMenu.AddItem(new GUIContent(kvp.Key.menuItem), false, () => kvp1.Value.Invoke(obj, null));
            }
        }
        #endregion
    }
}
