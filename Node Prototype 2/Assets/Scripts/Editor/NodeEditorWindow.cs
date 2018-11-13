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

        private NodeGraph _graph = null;
        public NodeGraph Graph
        {
            get { return _graph; }
            private set { _graph = value; }
        }

        private NodeGraphEditor _graphEditor = null;
        public NodeGraphEditor GraphEditor
        {
            get { return _graphEditor; }
            private set { _graphEditor = value; }
        }
        
        private Vector2 _panOffset;
        public Vector2 PanOffset
        {
            get { return _panOffset; }
            private set
            {
                _panOffset = value;
                Repaint();
            }
        }

        private float _zoom = 1;
        public float Zoom
        {
            get { return _zoom; }
            private set
            {
                _zoom = Mathf.Clamp(value, NodePreferences.MIN_ZOOM, NodePreferences.MAX_ZOOM);
                Repaint();
            }
        }

        private bool _isPanning = false;
        public bool IsPanning
        {
            get { return _isPanning; }
            private set { _isPanning = value; }
        }

        private Event _cachedEvent = default(Event);
        public Event CachedEvent { get { return _cachedEvent; } }

        private Matrix4x4 _cachedMatrix = Matrix4x4.identity;
        
        private enum Activity { Idle, HoldingNode, DraggingNode, HoldingGrid, DraggingGrid }
        private Activity _currentActivity = Activity.Idle;

        private bool _leftClick = false;
        private bool _rightClick = false;
        private bool _middleClick = false;

        private Port _draggedPort = null;
        private Port _hoveredPort = null;
        private Node _draggedNode = null;
        private Node _hoveredNode = null;

        private Port _draggedOutputTarget = null;

        private bool _shouldRepaint = false;
        private Rect _selectionRect = default(Rect);
        private Vector2 _dragStart = Vector2.zero;

        private bool IsDraggingPort { get { return _draggedPort != null; } }
        private bool IsHoveringPort { get { return _hoveredPort != null; } }
        private bool IsDraggingNode { get { return _draggedNode != null; } }
        private bool IsHoveringNode { get { return _hoveredNode != null; } }

        private Func<bool> _isDockedMethod;
        private Func<bool> IsDockedMethod
        {
            get
            {
                if (_isDockedMethod != null) return _isDockedMethod;

                const BindingFlags binding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
                MethodInfo isDockedMethod = typeof(NodeEditorWindow).GetProperty("docked", binding).GetGetMethod(true);
                _isDockedMethod = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), this, isDockedMethod);
                return _isDockedMethod;
            }
        }
        private bool IsDocked { get { return IsDockedMethod(); } }

        private Node[] SelectedNodes { get { return Selection.GetFiltered<Node>(SelectionMode.Unfiltered); } }

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

            HandleEvents(_cachedEvent);

            DrawGrid();
            DrawNodes();
            DrawConnections();
            DrawHeldConnection();
            //DrawSelectionBox();
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

            _leftClick = _cachedEvent.button == 0;
            _rightClick = _cachedEvent.button == 1;
            _middleClick = _cachedEvent.button == 2;

            //_currentActivity = Activity.Idle;
        }

        private void ResetCache()
        {
            GUI.matrix = _cachedMatrix;
            _cachedEvent = null;
            _cachedMatrix = Matrix4x4.identity;

            //Doesn't matter but eh
            _leftClick = false;
            _rightClick = false;
            _middleClick = false;
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