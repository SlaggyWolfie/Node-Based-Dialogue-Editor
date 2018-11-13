using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using RPG.Nodes;
using Object = UnityEngine.Object;

namespace RPG.Nodes
{
    public sealed class NodeContextMenus
    {
        private NodeContextMenus() { }

        #region Context Menu Manipulation
        public void ShowRerouteContextMenu(Connection connection)
        {
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Remove"), false, () => connection.RemoveSelf());
            contextMenu.DropDown(new Rect(NodeEditorWindow.CurrentNodeEditorWindow.CachedEvent.mousePosition, Vector2.zero));
            NodeUtilities.AutoSaveAssets();
        }

        public static void ShowPortContextMenu(Port hoveredPort)
        {
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Clear Connections"), false, hoveredPort.ClearConnections);
            contextMenu.DropDown(new Rect(NodeEditorWindow.CurrentNodeEditorWindow.CachedEvent.mousePosition, Vector2.zero));
            NodeUtilities.AutoSaveAssets();
        }

        public static void ShowNodeContextMenu()
        {
            NodeEditorWindow window = NodeEditorWindow.CurrentNodeEditorWindow;

            GenericMenu contextMenu = new GenericMenu();
            bool oneNodeSelected = Selection.objects.Length == 1 && Selection.activeObject is Node;

            if (oneNodeSelected)
            {
                Node node = (Node)Selection.activeObject;
                contextMenu.AddItem(new GUIContent("Move To Top"), false, () => window.SendToFront(node));
                //contextMenu.AddItem(new GUIContent("Rename"), false, window.RenameSelectedNode);
            }

            contextMenu.AddItem(new GUIContent("Duplicate"), false, window.DuplicateSelectedNodes);
            contextMenu.AddItem(new GUIContent("Remove"), false, window.RemoveSelectedNodes);

            if (oneNodeSelected)
            {
                Node node = (Node)Selection.activeObject;
                AddCustomContextMenuItems(contextMenu, node);
            }

            contextMenu.DropDown(new Rect(window.CachedEvent.mousePosition, Vector2.zero));
        }

        public static void ShowGraphContextMenu()
        {
            NodeEditorWindow window = NodeEditorWindow.CurrentNodeEditorWindow;

            GenericMenu contextMenu = new GenericMenu();
            Vector2 pos = window.WindowToGridPosition(window.CachedEvent.mousePosition);

            //for (int i = 0; i < nodeTypes.Length; i++)
            //{
            //    Type type = nodeTypes[i];

            //    //Get node context menu path
            //    string path = window.GraphEditor.GetNodeMenuName(type);
            //    if (string.IsNullOrEmpty(path)) continue;

            //    contextMenu.AddItem(new GUIContent(path), false, () =>{ window.CreateNode(type, pos);});
            //}

            contextMenu.AddSeparator("");
            //contextMenu.AddItem(new GUIContent("Preferences"), false, () => OpenPreferences());
            AddCustomContextMenuItems(contextMenu, window.Graph);
            contextMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
        }

        private static void AddCustomContextMenuItems(GenericMenu contextMenu, object obj)
        {
            KeyValuePair<ContextMenu, System.Reflection.MethodInfo>[] items = NodeReflection.GetAttributeMethods<ContextMenu>(obj);
            
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
