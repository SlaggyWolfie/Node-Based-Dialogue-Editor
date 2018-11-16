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
    public sealed partial class NodeEditorWindow
    {
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

        private Event _cachedEvent = null;

        private Matrix4x4 _cachedMatrix = Matrix4x4.identity;

        private enum Activity { Idle, Holding, Dragging, HoldingGrid, DraggingGrid }
        private Activity _currentActivity = Activity.Idle;

        private bool _leftMouseButtonUsed = false;
        private bool _rightMouseButtonUsed = false;
        private bool _middleMouseButtonUsed = false;

        private Port _draggedPort = null;
        private Port _hoveredPort = null;
        private OutputPort _draggedOutput = null;
        private OutputPort _hoveredOutput = null;
        private InputPort _draggedInput = null;
        private InputPort _hoveredInput = null;

        private Port _draggedPortTarget = null;
        private InputPort _draggedOutputTarget = null;
        private OutputPort _draggedInputTarget = null;

        private Port DraggedPort
        {
            get { return _draggedPort; }
            set
            {
                _draggedInput = null;
                _draggedOutput = null;

                _draggedPort = value;

                if (value is InputPort) _draggedInput = (InputPort)value;
                else if (value is OutputPort) _draggedOutput = (OutputPort)value;
            }
        }
        private InputPort DraggedInput
        {
            get { return _draggedInput; }
            set { DraggedPort = value; }
        }
        private OutputPort DraggedOutput
        {
            get { return _draggedOutput; }
            set { DraggedPort = value; }
        }

        private Port HoveredPort
        {
            get { return _draggedPort; }
            set
            {
                _hoveredPort = value;
                _hoveredInput = value as InputPort;
                _hoveredOutput = value as OutputPort;
            }
        }
        private InputPort HoveredInput
        {
            get { return _hoveredInput; }
            set { HoveredPort = value; }
        }
        private OutputPort HoveredOutput
        {
            get { return _hoveredOutput; }
            set { HoveredPort = value; }
        }

        private Port DraggedPortTarget
        {
            get { return _draggedPortTarget; }
            set
            {
                _draggedInputTarget = null;
                _draggedOutputTarget = null;

                _draggedPortTarget = value;

                if (value is InputPort) _draggedOutputTarget = (InputPort)value;
                else if (value is OutputPort) _draggedInputTarget = (OutputPort)value;
            }
        }
        private OutputPort DraggedInputTarget
        {
            get { return _draggedInputTarget; }
            set { DraggedPortTarget = value; }
        }
        private InputPort DraggedOutputTarget
        {
            get { return _draggedOutputTarget; }
            set { DraggedPortTarget = value; }
        }

        private bool IsDraggingPort { get { return DraggedPort != null; } }
        private bool IsDraggingInput { get { return DraggedInput != null; } }
        private bool IsDraggingOutput { get { return DraggedOutput != null; } }
        private bool IsHoveringPort { get { return HoveredPort != null; } }
        private bool IsHoveringInput { get { return HoveredInput != null; } }
        private bool IsHoveringOutput { get { return HoveredOutput != null; } }

        private Node _draggedNode = null;
        private Node _hoveredNode = null;

        private bool IsDraggingNode { get { return _draggedNode != null; } }
        private bool IsHoveringNode { get { return _hoveredNode != null; } }

        private bool _shouldRepaint = false;
        private Rect _selectionRect = default(Rect);

        private Dictionary<Node, Vector2> _dragOffset = new Dictionary<Node, Vector2>();
        private Vector2 _dragStart = Vector2.zero;

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

        private static Node[] GetSelectedNodes() { return Selection.GetFiltered<Node>(SelectionMode.Unfiltered); }
        private UnityEngine.Object[] _cachedSelectedObjects = null;
    }
}