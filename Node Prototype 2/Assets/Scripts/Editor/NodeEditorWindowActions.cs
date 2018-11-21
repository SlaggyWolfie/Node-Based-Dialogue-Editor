using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using RPG.Nodes.Base;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using MySObject = RPG.Nodes.Base.ScriptableObjectWithID;

namespace RPG.Nodes.Editor
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
                case EventType.KeyDown: KeyDown(); break;
                case EventType.ValidateCommand: ValidateCommand(); break;
                case EventType.Ignore: Ignore(); break;
                case EventType.ContextClick: break;
            }
        }

        #region User Input & Event Handling
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
                    bool gridSnap = true;

                    //Vector2 mousePosition = _mousePosition;
                    Vector2 mousePosition = WindowToGridPosition(_mousePosition);

                    //Move selected nodes with offset
                    Node[] selectedNodes = GetSelected<Node>();
                    foreach (var node in selectedNodes)
                    {
                        Vector2 initial = node.Position;
                        node.Position = mousePosition + _dragOffset[node];

                        if (gridSnap)
                        {
                            Vector2 newPosition;
                            newPosition.x = Mathf.Round((node.Position.x + 8) / 16) * 16 - 8;
                            newPosition.y = Mathf.Round((node.Position.y + 8) / 16) * 16 - 8;
                            node.Position = newPosition;
                        }

                        Vector2 offset = node.Position - initial;
                        if (offset.sqrMagnitude <= 0) continue;
                        node.OffsetPorts(offset);
                    }

                    ShouldRepaint();
                }
                else if (_currentActivity == Activity.HoldingGrid)
                {
                    _currentActivity = Activity.DraggingGrid;
                    _cachedSelectedObjects = new List<Object>(Selection.objects);
                    //_dragStart = _mousePosition;
                    _dragStart = WindowToGridPosition(_mousePosition);
                    ShouldRepaint();
                }
                else if (_currentActivity == Activity.DraggingGrid)
                {
                    //Vector2 boxStartPosition = _dragStart;
                    Vector2 boxStartPosition = GridToWindowPosition(_dragStart);
                    Vector2 mousePosition = _mousePosition;
                    //Vector2 mousePosition = WindowToGridPosition(_mousePosition);
                    Vector2 boxSize = mousePosition - boxStartPosition;
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
            else if (IsHoveringConnection)
            {
                if (!Selection.Contains(_hoveredConnection))
                    Select(_hoveredConnection, _cachedEvent.control || _cachedEvent.shift);
                else if (_cachedEvent.control || _cachedEvent.shift) Deselect(_hoveredConnection);

                _cachedEvent.Use();
                _currentActivity = Activity.Holding;
            }
            else if (IsHoveringNode)
            {
                if (!Selection.Contains(_hoveredNode))
                    Select(_hoveredNode, _cachedEvent.control || _cachedEvent.shift);
                else if (_cachedEvent.control || _cachedEvent.shift) Deselect(_hoveredNode);

                _cachedEvent.Use();
                _currentActivity = Activity.Holding;
            }
            else
            {
                //If the mouse was held down on the grid background, deselect everything
                _currentActivity = Activity.HoldingGrid;
                if (_cachedEvent.control || _cachedEvent.shift) return;
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
                    Node[] nodes = GetSelected<Node>();
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
                    Select(_hoveredNode, false);
                }

                ShouldRepaint();
                _currentActivity = Activity.Idle;
            }
            else if (_rightMouseButtonUsed || _middleMouseButtonUsed)
            {
                if (IsPanning)
                {
                    IsPanning = false;
                    return;
                }
                
                if (!IsDraggingPort && IsHoveringPort)
                {
                    ShowPortContextMenu(HoveredPort);
                }
                else if (IsHoveringConnection)
                {
                    if (!Selection.Contains(_hoveredConnection)) Select(_hoveredConnection, false);
                    ShowConnectionContextMenu();
                }
                else if (IsHoveringNode)
                {
                    if (!Selection.Contains(_hoveredNode)) Select(_hoveredNode, false);
                    ShowNodeContextMenu();
                }
                else
                {
                    ShowGraphContextMenu();
                }
            }
        }
        private void Ignore()
        {
            //If releasing the mouse outside the window
            if (_cachedEvent.rawType != EventType.MouseUp || _currentActivity != Activity.DraggingGrid) return;
            ShouldRepaint();
            _currentActivity = Activity.Idle;
        }
        private void ValidateCommand()
        {
            if (_cachedEvent.commandName == "SoftDelete")
                RemoveSelectedNodes();
            else if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX && _cachedEvent.commandName == "Delete")
                RemoveSelectedNodes();
            else if (_cachedEvent.commandName == "Duplicate")
                DuplicateSelectedNodes();
            ShouldRepaint();
        }
        private void KeyDown()
        {
            if (EditorGUIUtility.editingTextField) return;
            if (_cachedEvent.keyCode == KeyCode.F)
                Home();
            else if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX && _cachedEvent.keyCode == KeyCode.Return)
                RenameSelectedNode();
            else if (_cachedEvent.keyCode == KeyCode.F2)
                RenameSelectedNode();
        }
        #endregion

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
            Vector2 mousePosition = WindowToGridPosition(_mousePosition);
            //Vector2 mousePosition = _mousePosition;

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

            NodeRendering.DrawConnection(start, end, color, NodePreferences.CONNECTION_WIDTH);
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

                    NodeRendering.DrawConnection(start, end, NodePreferences.CONNECTION_PORT_COLOR, NodePreferences.CONNECTION_WIDTH);
                    DrawModifiers((start + end) / 2, connection);
                }
            }
        }

        private void DrawModifiers(Vector2 position, Connection connection) { throw new NotImplementedException(); }

        private void DrawNodes()
        {
            if (_isLayoutEvent) _culledNodes = new List<Node>();

            Color oldColor = GUI.color;
            NodeUtilities.BeginZoom(position, Zoom, TopPadding);

            for (int i = 0; i < _graph.NodeCount; i++)
            {
                Node node = _graph.GetNode(i);
                if (node == null) continue;
                if (i >= _graph.NodeCount) return;

                if (_isLayoutEvent)
                {
                    if (!Selection.Contains(node) && ShouldBeCulled(node))
                    {
                        _culledNodes.Add(node);
                        continue;
                    }
                }
                else if (_culledNodes.Contains(node)) continue;

                NodeEditor nodeEditor = NodeEditor.GetEditor(node);

                GUILayout.BeginArea(new Rect(node.Position, new Vector2(NodePreferences.STANDARD_NODE_SIZE.x, 4000)));

                bool selected = Selection.objects.Contains(node);

                if (selected)
                {
                    //TODO
                    //different drawing when selected
                    GUILayout.BeginVertical();
                    GUILayout.BeginVertical();
                }
                else
                {
                    GUILayout.BeginVertical();
                }

                GUI.color = oldColor;
                EditorGUI.BeginChangeCheck();

                nodeEditor.OnHeaderGUI();
                nodeEditor.OnBodyGUI();

                if (EditorGUI.EndChangeCheck())
                {
                    NodeEditor._UpdateNode(node);
                    EditorUtility.SetDirty(node);
                    nodeEditor.SerializedObject.ApplyModifiedProperties();
                }

                GUILayout.EndVertical();
                if (selected) GUILayout.EndVertical();

                GUILayout.EndArea();

                //Draw Ports
                var inputNode = node as IInput;
                if (inputNode != null)
                {
                    InputPort input = inputNode.InputPort;
                    Rect inputRect = nodeEditor.GetPortRect(input);
                    //inputRect = GridToWindowRectNotClipped(inputRect);
                    NodeRendering.DrawPort(inputRect, null, null, NodePreferences.CONNECTION_PORT_COLOR, NodePreferences.CONNECTION_PORT_COLOR);
                }

                ISingleOutput sOutputNode = node as ISingleOutput;
                if (sOutputNode != null)
                {
                    OutputPort output = sOutputNode.OutputPort;
                    Rect outputRect = nodeEditor.GetPortRect(output);
                    //outputRect = GridToWindowRectNotClipped(outputRect);
                    NodeRendering.DrawPort(outputRect, null, null, NodePreferences.CONNECTION_PORT_COLOR, NodePreferences.CONNECTION_PORT_COLOR);
                }

                IMultipleOutput mOutputNode = node as IMultipleOutput;
                if (mOutputNode != null)
                {
                    List<OutputPort> outputs = mOutputNode.GetOutputs();
                    foreach (OutputPort output in outputs)
                    {
                        Rect outputRect = nodeEditor.GetPortRect(output);
                        //outputRect = GridToWindowRectNotClipped(outputRect);
                        NodeRendering.DrawPort(outputRect, null, null, NodePreferences.CONNECTION_PORT_COLOR, NodePreferences.CONNECTION_PORT_COLOR);
                    }
                }
            }

            NodeUtilities.EndZoom(position, Zoom, TopPadding);
        }

        private void DrawGrid()
        {
            NodeRendering.DrawGrid(position, Zoom, PanOffset);
        }

        public void DrawSelectionBox()
        {
            if (_currentActivity != Activity.DraggingGrid) return;

            //Vector2 mousePosition = _mousePosition;
            Vector2 mousePosition = WindowToGridPosition(_mousePosition);
            Vector2 size = mousePosition - _dragStart;
            Rect rect = new Rect(_dragStart, size);
            rect.position = GridToWindowPosition(rect.position);
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

        #region Object Manipulation 
        private static void Select(MySObject obj, bool add)
        {
            if (add)
            {
                List<Object> selection = new List<Object>(Selection.objects) { obj };
                Selection.objects = selection.ToArray();
            }
            else Selection.objects = new Object[] { obj };
        }

        private static void Deselect(MySObject obj)
        {
            List<Object> selection = new List<Object>(Selection.objects);
            selection.Remove(obj);
            Selection.objects = selection.ToArray();
        }

        #region Nodes
        //private static void SelectNode(Node node, bool add)
        //{
        //    if (add)
        //    {
        //        List<Object> selection = new List<Object>(Selection.objects) { node };
        //        Selection.objects = selection.ToArray();
        //    }
        //    else Selection.objects = new Object[] { node };
        //}

        //private static void DeselectNode(Node node)
        //{
        //    List<Object> selection = new List<Object>(Selection.objects);
        //    selection.Remove(node);
        //    Selection.objects = selection.ToArray();
        //}

        private void CreateNode<T>(Vector2 position) where T : Node
        {
            CreateNode(typeof(T), position);
        }

        private void CreateNode(Type type, Vector2 position)
        {
            if (NodeReflection.IsOfType(type, typeof(Node))) return;

            Node node = _graph.AddNode(type);
            node.Position = position;
            node.name = ObjectNames.NicifyVariableName(type.Name);

            AssetDatabase.AddObjectToAsset(node, _graph);
            NodeUtilities.AutoSaveAssets();
            Repaint();
        }

        private void RemoveSelectedNodes()
        {
            foreach (Node node in GetSelected<Node>())
                GraphEditor.RemoveNode(node);
        }

        private void SendNodeToFront(Node node)
        {
            Graph.SendNodeToFront(node);
        }

        public void DuplicateSelectedNodes()
        {
            //TODO: add the option to Duplicate Connections if they have also been selected
            Node[] oldSelectedNodes = GetSelected<Node>();
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
        #region Connections
        private void CreateConnection()
        {
            Connection connection = _graph.AddConnection();
            AssetDatabase.AddObjectToAsset(connection, _graph);
            NodeUtilities.AutoSaveAssets();
            Repaint();
        }

        private void RemoveSelectedConnections()
        {
            foreach (Connection connection in GetSelected<Connection>())
                GraphEditor.RemoveConnection(connection);
        }

        private void SendConnectionToFront(Connection connection)
        {
            Graph.SendConnectionToFront(connection);
        }

        public void DuplicateSelectedConnections()
        {
            //TODO: add the option to Duplicate Connections if they have also been selected
            Node[] oldSelectedNodes = GetSelected<Node>();
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
        #endregion
        #endregion

        #region Other
        private bool IsHoveringTitle(Node node)
        {
            //TODO
            //Vector2 mousePosition = _mousePosition;
            //Vector2 nodePosition = node.Position;
            Vector2 mousePosition = WindowToGridPosition(_mousePosition);
            Vector2 nodePosition = GridToWindowPosition(node.Position);
            Vector2 size = NodePreferences.STANDARD_NODE_SIZE;
            //if (nodeSizes.TryGetValue(node, out size)) width = size.x;
            //else width = 200;
            Rect windowRect = new Rect(nodePosition, new Vector2(size.x / Zoom, NodePreferences.HEADER_HEIGHT / Zoom));
            return windowRect.Contains(mousePosition);
        }

        private void RecalculateDragOffsets()
        {
            Node[] nodes = GetSelected<Node>();
            _dragOffset = new Dictionary<Node, Vector2>();
            foreach (var node in nodes)
            {
                //_dragOffset[node] = node.Position - _mousePosition;
                _dragOffset[node] = node.Position - WindowToGridPosition(_mousePosition);
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

        private void CheckHoveringAndSelection()
        {
            if (_cachedEvent.type != EventType.Layout) return;

            //Vector2 mousePosition = _mousePosition;
            Vector2 mousePosition = WindowToGridPosition(_mousePosition);
            List<Object> boxSelected = new List<Object>(_boxSelectedObjects);

            for (int i = 0; i < Graph.NodeCount; i++)
            {
                Node node = Graph.GetNode(i);
                NodeEditor nodeEditor = NodeEditor.GetEditor(node);

                Vector2 nodeSize = GUILayoutUtility.GetLastRect().size;
                Rect nodeRect = new Rect(node.Position, nodeSize);
                if (nodeRect.Contains(mousePosition)) _hoveredNode = node;

                if (_currentActivity == Activity.DraggingGrid && nodeRect.Overlaps(_selectionRect)) boxSelected.Add(node);

                //Check hovering over ports
                var inputNode = node as IInput;
                if (inputNode != null)
                {
                    InputPort input = inputNode.InputPort;
                    Rect inputRect = nodeEditor.GetPortRect(input);
                    //inputRect = GridToWindowRectNotClipped(inputRect);
                    if (inputRect.Contains(mousePosition)) HoveredPort = input;
                }

                ISingleOutput sOutputNode = node as ISingleOutput;
                if (sOutputNode != null)
                {
                    OutputPort output = sOutputNode.OutputPort;
                    Rect outputRect = nodeEditor.GetPortRect(output);
                    //outputRect = GridToWindowRectNotClipped(outputRect);
                    if (outputRect.Contains(mousePosition)) HoveredPort = output;
                }

                IMultipleOutput mOutputNode = node as IMultipleOutput;
                if (mOutputNode != null)
                {
                    List<OutputPort> outputs = mOutputNode.GetOutputs();
                    foreach (OutputPort output in outputs)
                    {
                        Rect outputRect = nodeEditor.GetPortRect(output);
                        //outputRect = GridToWindowRectNotClipped(outputRect);
                        if (outputRect.Contains(mousePosition)) HoveredPort = output;
                    }
                }
            }

            for (int i = 0; i < Graph.ConnectionCount; i++)
            {
                Connection connection = Graph.GetConnection(i);
                Vector2 start = NodeEditor.FindPortRect(connection.Start).position;
                Vector2 end = NodeEditor.FindPortRect(connection.End).position;

                if (NodeUtilities.PointOverlapBezier(mousePosition, start, end, NodePreferences.CONNECTION_WIDTH)) _hoveredConnection = connection;

                //TODO: Add range overlap check, as just overlapping might be too annoying.
                if (_currentActivity == Activity.DraggingGrid &&
                    (_selectionRect.Contains(start) || _selectionRect.Contains(end)))
                    boxSelected.Add(connection);
            }

            if (!_isLayoutEvent && _currentActivity == Activity.DraggingGrid) Selection.objects = boxSelected.ToArray();
        }
    }
}
