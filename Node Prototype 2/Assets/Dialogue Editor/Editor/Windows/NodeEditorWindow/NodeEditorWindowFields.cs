using System;
using System.Collections.Generic;
using System.Reflection;
//using NaughtyAttributes;
using RPG.Base;
using RPG.Nodes;
using UnityEditor;
using UnityEngine;
using RPG.Nodes.Base;
using RPG.Other;
using Object = UnityEngine.Object;

namespace RPG.Editor.Nodes
{
    public sealed partial class NodeEditorWindow
    {
        //[ShowNonSerializedField]
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

        #region Data?
        //[ShowNonSerializedField]
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

        //[ShowNonSerializedField]
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

        //[ShowNativeProperty]
        public bool IsPanning { get; private set; }
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
        //[ShowNativeProperty]
        private bool IsDocked { get { return IsDockedMethod(); } }
        //[ShowNativeProperty]
        private float TopPadding { get { return IsDocked ? 19 : 22; } }
        #endregion

        #region Runtime Cache
        private Event _cachedEvent = null;
        private Matrix4x4 _cachedMatrix = Matrix4x4.identity;

        private enum Activity { Idle, Holding, Dragging, HoldingGrid, DraggingGrid }
        [/*ReadOnly, */SerializeField]
        private Activity _currentActivity = Activity.Idle;

        private bool _leftMouseButtonUsed = false;
        private bool _rightMouseButtonUsed = false;
        private bool _middleMouseButtonUsed = false;

        private bool _isLayoutEvent = false;
        private bool _isRepaintEvent = false;
        //[ShowNonSerializedField]
        private Vector2 _mousePosition = Vector2.zero;
        #endregion

        #region Ports
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
                if (value != null)
                {
                    _draggedPort = value;
                    _draggedInput = value as InputPort;
                    _draggedOutput = value as OutputPort;
                }
                else
                {
                    _draggedPort = null;
                    _draggedInput = null;
                    _draggedOutput = null;
                }
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
            get { return _hoveredPort; }
            set
            {
                if (value != null)
                {
                    _hoveredPort = value;
                    _hoveredInput = value as InputPort;
                    _hoveredOutput = value as OutputPort;
                }
                else
                {
                    _hoveredPort = null;
                    _hoveredInput = null;
                    _hoveredOutput = null;
                }
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

        //[ShowNativeProperty]
        private bool IsDraggingPort { get { return _draggedPort != null; } }
        //[ShowNativeProperty]
        private bool IsDraggingInput { get { return DraggedInput != null; } }
        //[ShowNativeProperty]
        private bool IsDraggingOutput { get { return DraggedOutput != null; } }
        //[ShowNativeProperty]
        private bool IsHoveringPort { get { return HoveredPort != null; } }
        //[ShowNativeProperty]
        private bool IsHoveringInput { get { return HoveredInput != null; } }
        //[ShowNativeProperty]
        private bool IsHoveringOutput { get { return HoveredOutput != null; } }
        #endregion
        #region Nodes
        //[ShowNonSerializedField]
        private Node _draggedNode = null;
        //[ShowNonSerializedField]
        private Node _hoveredNode = null;

        //[ShowNativeProperty]
        private bool IsDraggingNode { get { return _draggedNode != null; } }
        //[ShowNativeProperty]
        private bool IsHoveringNode { get { return _hoveredNode != null; } }
        #endregion

        //private bool _shouldRepaint = false;
        //[ShowNonSerializedField]
        private Rect _selectionRect = default(Rect);

        private Dictionary<Node, Vector2> _dragOffset = new Dictionary<Node, Vector2>();
        //[ShowNonSerializedField]
        private Vector2 _dragStart = Vector2.zero;

        //New and unsorted
        //--------------------------------------------------------------------------------------------------------//

        private static T[] GetSelected<T>() where T : BaseScriptableObject
        {
            //return Selection.objects.OfType<T>().ToArray();
            return Selection.GetFiltered<T>(SelectionMode.Unfiltered);
        }

        private static bool DifferentObjectsSelected()
        {
            int counter = 0;
            if (GetSelected<Node>().Length > 0) counter++;
            if (GetSelected<Connection>().Length > 0) counter++;
            if (GetSelected<ConnectionModifier>().Length > 0) counter++;
            return counter >= 2;
        }

        private List<Object> _cachedSelectedObjects = new List<Object>();
        //private Object[] _boxSelectedObjects = new Object[] { };

        private List<Node> _culledNodes = null;
        private Type[] _nodeTypes = null;
        private Type[] NodeTypes { get { return _nodeTypes ?? (_nodeTypes = ReflectionUtilities.GetDerivedTypes<Node>()); } }

        private Connection _hoveredConnection = null;
        //[ShowNativeProperty]
        private bool IsHoveringConnection { get { return _hoveredConnection != null; } }
        private ConnectionModifier _hoveredConnectionModifier = null;
        //[ShowNativeProperty]
        private bool IsHoveringConnectionModifier { get { return _hoveredConnectionModifier != null; } }

        private Dictionary<Node, Vector2> _nodeSizes = null;

        private List<ConnectionModifier> _culledMods = null;
        //private Dictionary<ConnectionModifier, Vector2> _modifierSizes = null;
        //private Dictionary<ConnectionModifier, Vector2> _modifierSizes = new Dictionary<ConnectionModifier, Vector2>();
    }
}