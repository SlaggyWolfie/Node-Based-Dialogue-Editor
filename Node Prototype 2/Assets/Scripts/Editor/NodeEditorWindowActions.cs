using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using RPG.Nodes;
using UnityEngine.Experimental.UIElements;
using Object = UnityEngine.Object;

namespace RPG.Nodes
{
    public sealed partial class NodeEditorWindow
    {
        private void HandleEvents()
        {
            wantsMouseMove = true;

            switch (_cachedEvent.type)
            {
                case EventType.MouseMove: break;
                case EventType.ScrollWheel: ScrollWheel(); break;
                case EventType.MouseDrag: MouseDrag(); break;
                case EventType.MouseDown: MouseDown(); break;
                case EventType.MouseUp: MouseUp(); break;
                case EventType.KeyDown:
                    if (EditorGUIUtility.editingTextField) break;
                    else if (_cachedEvent.keyCode == KeyCode.F) Home();
                    if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX &&
                        _cachedEvent.keyCode == KeyCode.Return) RenameSelectedNode();
                    if (_cachedEvent.keyCode == KeyCode.F2) RenameSelectedNode();
                    break;
                case EventType.ValidateCommand:
                    if (_cachedEvent.commandName == "SoftDelete") RemoveSelectedNodes();
                    else if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX && _cachedEvent.commandName == "Delete") RemoveSelectedNodes();
                    else if (_cachedEvent.commandName == "Duplicate") DuplicateSelectedNodes();
                    ShouldRepaint();
                    break;
                case EventType.Ignore:
                    //If releasing the mouse outside the window
                    if (_cachedEvent.rawType == EventType.MouseUp && _currentActivity == Activity.DraggingGrid)
                    {
                        ShouldRepaint();
                        _currentActivity = Activity.Idle;
                    }
                    break;
            }
        }

        private void ScrollWheel()
        {
            float zoomAmount = 0.1f * Zoom;
            if (_cachedEvent.delta.y <= 0) zoomAmount *= -1;
            Zoom += zoomAmount;
        }

        private void MouseDrag()
        {
            if (_leftMouseButtonUsed)
            {
                if (IsDraggingPort)
                {
                    if (IsHoveringInput)
                    {
                        if (!DraggedPort.IsConnectedTo(HoveredPort))
                            DraggedPortTarget = HoveredPort;
                    }
                    else _draggedOutputTarget = null;
                    ShouldRepaint();
                }
                else if (_currentActivity == Activity.Holding)
                {
                    RecalculateDragOffsets();
                    _currentActivity = Activity.Dragging;
                    ShouldRepaint();
                }

                if (_currentActivity == Activity.Dragging)
                {
                    //Holding CTRL inverts grid snap
                    //bool gridSnap = NodeEditorPreferences.GetSettings().gridSnap;
                    //if (_cachedEvent.control) gridSnap = !gridSnap;

                    Vector2 mousePosition = _cachedEvent.mousePosition;
                    //Vector2 mousePosition = WindowToGridPosition(_cachedEvent.mousePosition);

                    //Move selected nodes with offset
                    Node[] selectedNodes = GetSelectedNodes();
                    foreach (var node in selectedNodes)
                    {
                        Vector2 initial = node.Position;
                        node.Position = mousePosition + _dragOffset[node];

                        //if (gridSnap)
                        //{
                        //    node.position.x = (Mathf.Round((node.position.x + 8) / 16) * 16) - 8;
                        //    node.position.y = (Mathf.Round((node.position.y + 8) / 16) * 16) - 8;
                        //}

                        //Offset portConnectionPoints instantly if a node is dragged so they aren't delayed by a frame.
                        Vector2 offset = node.Position - initial;
                        if (offset.sqrMagnitude <= 0) continue;
                        node.OffsetPorts(offset);
                    }

                    ShouldRepaint();
                }
                else if (_currentActivity == Activity.HoldingGrid)
                {
                    _currentActivity = Activity.DraggingGrid;
                    _cachedSelectedObjects = Selection.objects;
                    _dragStart = _cachedEvent.mousePosition;
                    //_dragStart = WindowToGridPosition(_cachedEvent.mousePosition);
                    ShouldRepaint();
                }
                else if (_currentActivity == Activity.DraggingGrid)
                {
                    Vector2 boxStartPosition = _dragStart;
                    //Vector2 boxStartPosition = GridToWindowPosition(_dragBoxStart);
                    Vector2 boxSize = _cachedEvent.mousePosition - boxStartPosition;
                    if (boxSize.x < 0) { boxStartPosition.x += boxSize.x; boxSize.x = Mathf.Abs(boxSize.x); }
                    if (boxSize.y < 0) { boxStartPosition.y += boxSize.y; boxSize.y = Mathf.Abs(boxSize.y); }
                    _selectionRect = new Rect(boxStartPosition, boxSize);
                    ShouldRepaint();
                }
            }
            else if (_rightMouseButtonUsed || _middleMouseButtonUsed)
            {
                Vector2 tempOffset = PanOffset;
                tempOffset += _cachedEvent.delta * Zoom;
                //Round value to increase crispiness of UI text
                tempOffset.x = Mathf.Round(tempOffset.x);
                tempOffset.y = Mathf.Round(tempOffset.y);

                PanOffset = tempOffset;
                IsPanning = true;
            }
        }

        private void MouseDown()
        {
            Repaint();
            if (!_leftMouseButtonUsed) return;

            //draggedOutputReroutes.Clear();

            if (IsHoveringPort)
            {
                DraggedPort = HoveredPort;
                //if (IsHoveringOutput)
                //{
                //    //HoveredOutput.Connection.DisconnectInput();
                //    DraggedPort = HoveredPort;
                //}
                //else if (IsHoveringInput)
                //{
                //    //hoveredPort.VerifyConnections();
                //    if (HoveredInput.IsConnected)
                //    {
                //        Node node = HoveredPort.Node;
                //        ContextMenu

                //        //if (NodeEditor.onUpdateNode != null) NodeEditor._UpdateNode(node);
                //    }
                //    else DraggedPort = HoveredPort;
                //}
            }
            else if (IsHoveringNode)
            {
                if (!Selection.Contains(_hoveredNode))
                    SelectNode(_hoveredNode, _cachedEvent.control || _cachedEvent.shift);
                else if (_cachedEvent.control || _cachedEvent.shift) DeselectNode(_hoveredNode);

                _cachedEvent.Use();
                _currentActivity = Activity.Holding;
            }
            else
            {
                //If mousedown on grid background, deselect all
                _currentActivity = Activity.HoldingGrid;
                if (_cachedEvent.control || _cachedEvent.shift) return;
                //selectedReroutes.Clear();
                Selection.objects = new Object[] { };
                Selection.activeObject = null;
            }
        }

        private void MouseUp()
        {
            if (_leftMouseButtonUsed)
            {
                if (IsDraggingPort)
                {
                    //If the connection is valid, save it
                    if (DraggedOutputTarget != null)
                    {
                        if (Graph.NodeCount != 0) DraggedOutput.Connect(DraggedOutputTarget);
                        NodeEditor._UpdateNode(DraggedOutputTarget.Node);
                        NodeEditor._UpdateNode(DraggedOutput.Node);
                    }

                    //Release the dragged connection
                    DraggedOutput = null;
                    DraggedOutputTarget = null;
                    EditorUtility.SetDirty(Graph);
                    NodeUtilities.AutoSaveAssets();
                }
                else if (_currentActivity == Activity.Dragging)
                {
                    IEnumerable<Node> nodes = Selection.objects.OfType<Node>();
                    foreach (Node node in nodes) EditorUtility.SetDirty(node);
                    NodeUtilities.AutoSaveAssets();
                }
                else if (!IsHoveringNode)
                {
                    // If clicking outside the node, release the field focus
                    if (!IsPanning) EditorGUI.FocusTextInControl(null);
                    NodeUtilities.AutoSaveAssets();
                }

                if (_currentActivity == Activity.Holding && !(_cachedEvent.control || _cachedEvent.shift))
                {
                    SelectNode(_hoveredNode, false);
                }

                ShouldRepaint();
                _currentActivity = Activity.Idle;
            }
            else if (_rightMouseButtonUsed || _middleMouseButtonUsed)
            {
                if (IsPanning) return;

                if (!IsDraggingPort && IsHoveringPort)
                {
                    NodeContextMenus.ShowPortContextMenu(HoveredPort);
                }
                else if (IsHoveringNode)
                {
                    if (!Selection.Contains(_hoveredNode)) SelectNode(_hoveredNode, false);
                    NodeContextMenus.ShowNodeContextMenu();
                }
                else
                {
                    NodeContextMenus.ShowGraphContextMenu();
                }
            }
        }

        #region Drawing
        private void DrawHeldConnection()
        {
            if (!IsDraggingPort) return;

            Color color = NodePreferences.CONNECTION_PORT_COLOR;
            Vector2 start, end;
            Node node = _draggedPort.Node;
            NodeEditor nodeEditor = NodeEditor.GetEditor(node);

            color.a *= 0.6f;

            Vector2 portPosition = nodeEditor.GetPortRect(_draggedPort).position;
            Vector2 mousePosition = _cachedEvent.mousePosition;

            if (_draggedPort is InputPort)
            {
                start = mousePosition;
                end = portPosition;
            }
            else
            {
                start = portPosition;
                end = mousePosition;
            }

            NodeRendering.DrawConnection(start, end, color);
            //NodeUtilities.DrawPort(new Rect());
            //DrawModifiers((start + end) / 2);
        }

        private void DrawConnections()
        {
            for (int i = 0; i < _graph.NodeCount; i++)
            {
                Node node = _graph.GetNode(i);
                IInput inputNode = node as IInput;
                InputPort input = inputNode != null ? inputNode.InputPort : null;
                if (input == null) continue;

                Vector2 end = NodeEditor.GetEditor(node).GetPortRect(input).center;
                for (int j = 0; j < input.ConnectionsCount; j++)
                {
                    Connection connection = input.GetConnection(j);

                    OutputPort output = connection.Start;
                    Vector2 start = NodeEditor.GetEditor(output.Node).GetPortRect(output).center;

                    NodeRendering.DrawConnection(start, end, NodePreferences.CONNECTION_PORT_COLOR);
                    DrawModifiers((start + end) / 2, connection);
                }
            }
        }

        private void DrawModifiers(Vector2 position, Connection connection) { throw new NotImplementedException(); }

        private void DrawNodes()
        {
            for (int i = 0; i < _graph.NodeCount; i++)
            {
                Node node = _graph.GetNode(i);
                DrawNode(node);
            }
        }

        private static void DrawNode(Node node)
        {
            NodeEditor nodeEditor = NodeEditor.GetEditor(node);

            nodeEditor.OnHeaderGUI();
            nodeEditor.OnBodyGUI();
        }

        private void DrawGrid()
        {
            NodeRendering.DrawGrid(position, Zoom, PanOffset);
        }

        public void DrawSelectionBox()
        {
            if (_currentActivity != Activity.DraggingGrid) return;

            Vector2 cursorPosition = _cachedEvent.mousePosition;
            //Vector2 cursorPosition = WindowToGridPosition(_cachedEvent.mousePosition);
            Vector2 size = cursorPosition - _dragStart;
            Rect rect = new Rect(_dragStart, size);
            //rect.position = GridToWindowPosition(rect.position);
            rect.size /= Zoom;
            Handles.DrawSolidRectangleWithOutline(rect,
                NodePreferences.SELECTION_FACE_COLOR, NodePreferences.SELECTION_BORDER_COLOR);
        }

        private bool ShouldBeCulled(Node node)
        {
            Vector2 nodePosition = node.Position;
            //Vector2 nodePosition = GridToWindowPosition(node.Position);
            if (nodePosition.x / Zoom > position.width) return true; // Right
            if (nodePosition.y / Zoom > position.height) return true; // Bottom

            throw new NotImplementedException();
            if (nodePosition.x + NodePreferences.STANDARD_NODE_SIZE.x < 0) return true; // Left
            if (nodePosition.y + NodePreferences.STANDARD_NODE_SIZE.y < 0) return true; // Top

            return false;
        }
        #endregion

        #region Grid Positioning
        public Vector2 WindowToGridPosition(Vector2 windowPosition)
        {
            return (windowPosition - position.size * 0.5f) * Zoom - PanOffset;
        }

        public Vector2 GridToWindowPosition(Vector2 gridPosition)
        {
            return position.size * 0.5f + (PanOffset + gridPosition) / Zoom;
        }

        public Rect GridToWindowRectNotClipped(Rect gridRect)
        {
            gridRect.position = GridToWindowPositionNotClipped(gridRect.position);
            return gridRect;
        }

        public Rect GridToWindowRect(Rect gridRect)
        {
            gridRect.position = GridToWindowPosition(gridRect.position);
            gridRect.size /= Zoom;
            return gridRect;
        }

        public Vector2 GridToWindowPositionNotClipped(Vector2 gridPosition)
        {
            return position.size * 0.5f * Zoom + PanOffset + gridPosition;
        }
        #endregion

        #region Node Manipulation
        public static void SelectNode(Node node, bool add)
        {
            if (add)
            {
                List<Object> selection = new List<Object>(Selection.objects) { node };
                Selection.objects = selection.ToArray();
            }
            else Selection.objects = new Object[] { node };
        }

        public static void DeselectNode(Node node)
        {
            List<Object> selection = new List<Object>(Selection.objects);
            selection.Remove(node);
            Selection.objects = selection.ToArray();
        }

        public void CreateNode<T>(Vector2 position) where T : Node
        {
            CreateNode(typeof(T), position);
        }

        public void CreateNode(Type type, Vector2 position)
        {
            if (NodeReflection.IsOfType(type, typeof(Node))) return;

            Node node = _graph.AddNode(type);
            node.Position = position;
            node.name = ObjectNames.NicifyVariableName(type.Name);

            AssetDatabase.AddObjectToAsset(node, _graph);
            NodeUtilities.AutoSaveAssets();
            Repaint();
        }

        public void RemoveSelectedNodes()
        {
            foreach (Node node in GetSelectedNodes())
                GraphEditor.RemoveNode(node);
        }

        public void SendToFront(Node node)
        {
            Graph.SendToFront(node);
        }

        public void DuplicateSelectedNodes()
        {
            //TODO: add the option to Duplicate Connections if they have also been selected
            Node[] oldSelectedNodes = GetSelectedNodes();
            Object[] newNodes = new Object[oldSelectedNodes.Length];

            Dictionary<Node, Node> substitutes = new Dictionary<Node, Node>();
            for (int i = 0; i < oldSelectedNodes.Length; i++)
            {
                Node oldNode = oldSelectedNodes[i];
                if (oldNode.Graph != Graph) continue;
                Node newNode = GraphEditor.CopyNode(oldNode);
                substitutes[oldNode] = newNode;
                newNode.Position = oldNode.Position + NodePreferences.DUPLICATION_OFFSET;
                newNode.ClearConnections();
                newNodes[i] = newNode;
            }

            // Walk through the selected nodes again, recreate connections, using the new nodes
            foreach (var oldNode in oldSelectedNodes)
            {
                if (oldNode.Graph != Graph) continue;
            }

            Selection.objects = newNodes;
        }

        private void RenameSelectedNode()
        {
            if (Selection.objects.Length != 1 || !(Selection.activeObject is Node)) return;
            Node node = (Node)Selection.activeObject;
            NodeEditor.GetEditor(node).InitiateRename();
        }
        #endregion

        #region Other
        private bool IsHoveringTitle(Node node)
        {
            Vector2 mousePosition = _cachedEvent.mousePosition;
            //Get node position
            Vector2 nodePosition = node.Position;
            //Vector2 nodePosition = GridToWindowPosition(node.Position);
            Vector2 size = NodePreferences.STANDARD_NODE_SIZE;
            //if (nodeSizes.TryGetValue(node, out size)) width = size.x;
            //else width = 200;
            Rect windowRect = new Rect(nodePosition, new Vector2(size.x / Zoom, NodePreferences.HEADER_HEIGHT / Zoom));
            return windowRect.Contains(mousePosition);
        }

        private void RecalculateDragOffsets()
        {
            Node[] nodes = GetSelectedNodes();
            _dragOffset = new Dictionary<Node, Vector2>();
            foreach (var node in nodes)
            {
                _dragOffset[node] = node.Position - _cachedEvent.mousePosition;
                //_dragOffset[node] = node.position - WindowToGridPosition(_cachedEvent.mousePosition);
            }
        }

        private void Home()
        {
            Zoom = 2;
            PanOffset = Vector2.zero;
        }

        private void ShouldRepaint(bool should = true)
        {
            _shouldRepaint = should;
        }
        #endregion
    }
}
