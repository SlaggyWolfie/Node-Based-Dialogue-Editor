using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using RPG.Nodes;

namespace RPG.Nodes
{
    public sealed partial class NodeEditorWindow : EditorWindow
    {
        private static NodeEditorWindow _currentNodeEditorWindow = null;
        public static NodeEditorWindow CurrentNodeEditorWindow
        {
            get { return _currentNodeEditorWindow; }
            private set { _currentNodeEditorWindow = value; }
        }

        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line)
        {
            NodeGraph nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as NodeGraph;
            if (nodeGraph == null) return false;

            NodeEditorWindow window = (NodeEditorWindow)GetWindow(typeof(NodeEditorWindow), false, "Node Editor", true);
            window.wantsMouseMove = true;
            window.Graph = nodeGraph;
            return true;
        }

        public static void RepaintAll()
        {
            NodeEditorWindow[] windows = Resources.FindObjectsOfTypeAll<NodeEditorWindow>();
            foreach (var window in windows) window.Repaint();
        }

        private void OnFocus()
        {
            CurrentNodeEditorWindow = this;
            _graphEditor = NodeGraphEditor.GetEditor(_graph);
            if (_graphEditor != null) NodeUtilities.AutoSaveAssets();
        }

        private void OnDisable()
        {
            //On Before Serialize
        }

        private void OnEnable()
        {
            //On Before Deserialize
        }

        private void OnGUI()
        {
            PreCache();

            if (_graph == null) return;
            //throw new NotImplementedException();
            _graphEditor = NodeGraphEditor.GetEditor(_graph);
            _graphEditor.Rectangle = position;

            HandleEvents();

            DrawGrid();
            DrawNodes();
            DrawConnections();
            DrawHeldConnection();
            DrawSelectionBox();
            //DrawTooltip();

            _graphEditor.OnGUI();

            if (_shouldRepaint)
            {
                Repaint();
                _shouldRepaint = false;
            }

            ResetCache();
        }

        private void PreCache()
        {
            _cachedEvent = Event.current;
            _cachedMatrix = GUI.matrix;

            _leftMouseButtonUsed = _cachedEvent.button == 0;
            _rightMouseButtonUsed = _cachedEvent.button == 1;
            _middleMouseButtonUsed = _cachedEvent.button == 2;

            //_currentActivity = Activity.Idle;
        }

        private void ResetCache()
        {
            GUI.matrix = _cachedMatrix;
            _cachedEvent = null;
            _cachedMatrix = Matrix4x4.identity;

            //Doesn't matter but eh
            _leftMouseButtonUsed = false;
            _rightMouseButtonUsed = false;
            _middleMouseButtonUsed = false;
        }

        public static NodeEditorWindow ForceWindow()
        {
            NodeEditorWindow window = CreateInstance<NodeEditorWindow>();
            window.titleContent = new GUIContent("Node Editor");
            window.wantsMouseMove = true;
            window.Show();
            return window;
        }

        public void Save()
        {
            if (AssetDatabase.Contains(_graph))
            {
                EditorUtility.SetDirty(_graph);
                NodeUtilities.AutoSaveAssets();
            }
            else SaveAs();
        }

        public void SaveAs()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Node Graph", "NewNodeGraph", "asset", "");
            if (string.IsNullOrEmpty(path)) return;

            NodeGraph existingGraph = AssetDatabase.LoadAssetAtPath<NodeGraph>(path);
            if (existingGraph != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(_graph, path);
            EditorUtility.SetDirty(_graph);
            NodeUtilities.AutoSaveAssets();
        }
    }
}