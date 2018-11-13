using System;
using System.Collections;
using System.Collections.Generic;
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
            set { _currentNodeEditorWindow = value; }
        }

        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line)
        {
            NodeGraph nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as NodeGraph;
            if (nodeGraph == null) return false;

            NodeEditorWindow window =
                (NodeEditorWindow)GetWindow(typeof(NodeEditorWindow), false, "Node Editor", true);
            window.wantsMouseMove = true;
            window.Graph = nodeGraph;
            return true;
        }

        public static void RepaintAll()
        {
            NodeEditorWindow[] windows = Resources.FindObjectsOfTypeAll<NodeEditorWindow>();
            foreach (var window in windows) window.Repaint();
        }



        private NodeGraph _graph = null;

        public NodeGraph Graph
        {
            get { return _graph; }
            set { _graph = value; }
        }

        private NodeGraphEditor _graphEditor = null;

        public NodeGraphEditor GraphEditor
        {
            get { return _graphEditor; }
            set { _graphEditor = value; }
        }

        //settings
        private Vector2 _panOffset;

        public Vector2 PanOffset
        {
            get { return _panOffset; }
            set
            {
                _panOffset = value;
                Repaint();
            }
        }

        private float _zoom = 1;

        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = Mathf.Clamp(value, NodePreferences.MIN_ZOOM, NodePreferences.MAX_ZOOM);
                Repaint();
            }
        }

        //Cache

        private Event _cachedEvent = default(Event);
        public Event CachedEvent { get { return _cachedEvent; } }

        private Matrix4x4 _cachedMatrix = Matrix4x4.identity;

        //Convenience
        private Port _draggedPort = null;
        private Port _hoveredPort = null;
        private Node _draggedNode = null;
        private Node _hoveredNode = null;

        private bool IsDraggingPort { get { return _draggedPort != null; } }
        private bool IsHoveringPort { get { return _hoveredPort != null; } }
        private bool IsDraggingNode { get { return _draggedNode != null; } }
        private bool IsHoveringNode { get { return _hoveredNode != null; } }

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
            //Cache
            _cachedEvent = Event.current;
            _cachedMatrix = GUI.matrix;

            if (_graph == null) return;
            //throw new NotImplementedException();
            _graphEditor = NodeGraphEditor.GetEditor(_graph);
            _graphEditor.Rectangle = position;

            HandleEvents(_cachedEvent);

            DrawGrid();
            DrawNodes();
            DrawConnections();
            DrawHeldConnection();
            //DrawSelectionBox();
            //DrawTooltip();
            _graphEditor.OnGUI();

            //Reset
            GUI.matrix = _cachedMatrix;
            _cachedEvent = null;
            _cachedMatrix = Matrix4x4.identity;
        }

        public static NodeEditorWindow ForceWindow()
        {
            NodeEditorWindow w = CreateInstance<NodeEditorWindow>();
            w.titleContent = new GUIContent("Node Editor");
            w.wantsMouseMove = true;
            w.Show();
            return w;
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