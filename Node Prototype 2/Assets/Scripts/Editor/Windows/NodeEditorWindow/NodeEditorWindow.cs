#define RPG_DEBUG_NODE

using System;
using RPG.Nodes;
using RPG.Nodes.Base;
using RPG.Utility;
using RPG.Utility.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace RPG.Editor.Nodes
{
    [Serializable]
    public sealed partial class NodeEditorWindow : Window
    {
        private NodeEditorWindow()
        {
            IsPanning = false;
            NullifyStuff();
        }

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

            NodeEditorWindow window = (NodeEditorWindow)GetWindow(typeof(NodeEditorWindow), false, "RPG Node Editor", true);
            window.wantsMouseMove = true;
            window.Graph = nodeGraph;
            return true;
        }

        //public static void RepaintAll()
        //{
        //    NodeEditorWindow[] windows = Resources.FindObjectsOfTypeAll<NodeEditorWindow>();
        //    foreach (var window in windows) window.Repaint();
        //}

        protected override void OnFocus()
        {
            base.OnFocus();
            CurrentNodeEditorWindow = this;

            if (_graph == null) return;
            _graphEditor = NodeGraphEditor.GetEditor(_graph);

            if (_graph.LocalVariableInventory != null)
                Blackboard.Instance.CurrentLocalVariableInventory = _graph.LocalVariableInventory;

            if (_graphEditor != null) EditorUtilities.AutoSaveAssets();
        }

        private void OnDisable()
        {
            //On Before Serialize
        }

        private void OnEnable()
        {
            IsPanning = false;
            NullifyStuff();
        }

        public override void OnGUI()
        {
            if (_graph == null) return;

            if (Event.current.keyCode == KeyCode.T && Event.current.control) Select(this, false);

            PreCache();
            //ResetHover();
            //throw new NotImplementedException();
            Blackboard.Instance.CurrentLocalVariableInventory = _graph.LocalVariableInventory;
            GraphEditor = NodeGraphEditor.GetEditor(_graph);
            GraphEditor.Rect = position;

            //HandleEvents();

            DrawGrid();

            Utilities.BeginZoom(position, Zoom, TopPadding);
            DrawNodes();
            DrawConnections();
            DrawHeldConnection();
            Utilities.EndZoom(position, Zoom, TopPadding);

            DrawSelectionBox();
            //DrawTooltip();
            DrawChildWindows();

            CheckHoveringAndSelection();
            GraphEditor.OnGUI();
            HandleGUIEvents();

            //if (_shouldUseEvent)
            //{
            //    _cachedEvent.Use();
            //    _shouldUseEvent = false;
            //}

            //if (_shouldRepaint)
            //{
            //    Repaint();
            //    _shouldRepaint = false;
            //}

            ResetCache();
        }

        private void PreCache()
        {
            _cachedEvent = Event.current;
            _cachedMatrix = GUI.matrix;

            _mousePosition = _cachedEvent.mousePosition;

            _leftMouseButtonUsed = _cachedEvent.button == 0;
            _rightMouseButtonUsed = _cachedEvent.button == 1;
            _middleMouseButtonUsed = _cachedEvent.button == 2;

            _isLayoutEvent = _cachedEvent.type == EventType.Layout;
            _isRepaintEvent = _cachedEvent.type == EventType.Repaint;

            //Debug.Log("Left Click: " + _leftMouseButtonUsed);
            //Debug.Log("Right Click: " + _rightMouseButtonUsed);
            //Debug.Log("Mouse Position: " + _mousePosition);
            //Debug.Log("Current Event: " + _cachedEvent.type.ToString());
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

            _isLayoutEvent = false;
            _isRepaintEvent = false;
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
                EditorUtilities.AutoSaveAssets();
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
            EditorUtilities.AutoSaveAssets();
        }
    }
}