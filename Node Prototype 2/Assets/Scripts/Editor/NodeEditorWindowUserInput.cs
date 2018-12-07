using System;
using System.Collections.Generic;
using RPG.Other;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using ScrObject = RPG.Nodes.Base.BaseScriptableObject;
//using ScrObj = UnityEngine.Ba;

namespace RPG.Nodes.Editor
{
    public sealed partial class NodeEditorWindow
    {
        private void HandleEvents()
        {
            wantsMouseMove = true;

            switch (_cachedEvent.type)
            {
                //case EventType.MouseMove: break;
                case EventType.ScrollWheel: ScrollWheel(); break;
                //case EventType.ContextClick: ContextClick(); break;
                case EventType.MouseDrag: MouseDrag(); break;
                case EventType.MouseDown: MouseDown(); break;
                case EventType.MouseUp: MouseUp(); break;
                case EventType.KeyDown: KeyDown(); break;
                case EventType.ValidateCommand: ValidateCommand(); break;
                case EventType.Ignore: Ignore(); break;
                    //case EventType.Repaint: break;
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
                        if (!DraggedPort.IsConnectedTo(HoveredInput))
                            DraggedPortTarget = HoveredInput;
                    }
                    else _draggedOutputTarget = null;
                    Repaint();
                }
                else if (_currentActivity == Activity.Holding)
                {
                    RecalculateDragOffsets();
                    _currentActivity = Activity.Dragging;
                    Repaint();
                }

                switch (_currentActivity)
                {
                    case Activity.Dragging:
                        {
                            //Holding CTRL inverts grid snap
                            //bool gridSnap = NodeEditorPreferences.GetSettings().gridSnap;
                            bool gridSnap = false;
                            if (_cachedEvent.control) gridSnap = !gridSnap;

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
                                    const float gridOffset = NodePreferences.GRID_SIZE / 8.0f;
                                    const float rounding = NodePreferences.GRID_SIZE / 4.0f;
                                    newPosition.x = Mathf.Round((node.Position.x + gridOffset) / rounding) * rounding -
                                                    gridOffset;
                                    newPosition.y = Mathf.Round((node.Position.y + gridOffset) / rounding) * rounding -
                                                    gridOffset;
                                    node.Position = newPosition;
                                }

                                Vector2 offset = node.Position - initial;
                                if (offset.sqrMagnitude <= 0) continue;
                                node.OffsetPorts(offset);
                            }

                            Repaint();
                            break;
                        }
                    case Activity.HoldingGrid:
                        {
                            _currentActivity = Activity.DraggingGrid;
                            _cachedSelectedObjects = new List<Object>(Selection.objects);
                            //_dragStart = _mousePosition;
                            _dragStart = WindowToGridPosition(_mousePosition);
                            Repaint();
                            break;
                        }
                    case Activity.DraggingGrid:
                        {
                            //Vector2 boxStartPosition = _dragStart;
                            Vector2 boxStartPosition = GridToWindowPosition(_dragStart);
                            //Vector2 boxStartPosition = GridToWindowPositionNotClipped(_dragStart);
                            Vector2 mousePosition = _mousePosition;
                            //Vector2 mousePosition = WindowToGridPosition(_mousePosition);
                            Vector2 boxSize = mousePosition - boxStartPosition;
                            if (boxSize.x < 0) { boxStartPosition.x += boxSize.x; boxSize.x = Mathf.Abs(boxSize.x); }
                            if (boxSize.y < 0) { boxStartPosition.y += boxSize.y; boxSize.y = Mathf.Abs(boxSize.y); }
                            _selectionRect = new Rect(boxStartPosition, boxSize);
                            Repaint();
                            break;
                        }
                }
            }
            else if (_rightMouseButtonUsed || _middleMouseButtonUsed)
            {
                Vector2 temporaryOffset = PanOffset;
                temporaryOffset += _cachedEvent.delta * Zoom;

                //Round value to increase crispiness of UI text
                temporaryOffset.x = Mathf.Round(temporaryOffset.x);
                temporaryOffset.y = Mathf.Round(temporaryOffset.y);

                PanOffset = temporaryOffset;
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
            else
            {
                bool controlOrShift = _cachedEvent.control || _cachedEvent.shift;

                if (IsHoveringConnection)
                {
                    if (!Selection.Contains(_hoveredConnection))
                        Select(_hoveredConnection, controlOrShift);
                    else if (controlOrShift) Deselect(_hoveredConnection);

                    _cachedEvent.Use();
                    _currentActivity = Activity.Holding;
                }
                else if (IsHoveringNode && IsHoveringTitle(_hoveredNode))
                {
                    if (!Selection.Contains(_hoveredNode))
                        Select(_hoveredNode, controlOrShift);
                    else if (controlOrShift) Deselect(_hoveredNode);

                    _cachedEvent.Use();
                    _currentActivity = Activity.Holding;
                }
                else if (!IsHoveringNode)
                {
                    //If the mouse was held down on the grid background, deselect everything
                    _currentActivity = Activity.HoldingGrid;
                    if (controlOrShift) return;
                    Selection.objects = new Object[] { };
                    Selection.activeObject = null;
                }
            }
        }
        private void MouseUp()
        {
            if (!_leftMouseButtonUsed)
            {
                if (_rightMouseButtonUsed || _middleMouseButtonUsed) ContextClick();
                return;
            }

            if (IsDraggingPort)
            {
                //If the connection is valid, save it
                if (DraggedOutputTarget != null)
                {
                    //DraggedOutput.Connect(DraggedOutputTarget)
                    if (Graph.NodeCount != 0) Connect(DraggedOutput, DraggedOutputTarget);
                    NodeEditor.UpdateCallback(DraggedOutputTarget.Node);
                    NodeEditor.UpdateCallback(DraggedOutput.Node);
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
                //If clicking outside the node, release the field focus
                if (!IsPanning) EditorGUI.FocusTextInControl(null);
                NodeUtilities.AutoSaveAssets();
            }

            if (_currentActivity == Activity.Holding && !(_cachedEvent.control || _cachedEvent.shift))
            {
                Select(_hoveredNode, false);
            }

            Repaint();
            _currentActivity = Activity.Idle;
        }

        private void ContextClick()
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
            else if (IsHoveringNode && IsHoveringTitle(_hoveredNode))
            {
                if (!Selection.Contains(_hoveredNode)) Select(_hoveredNode, false);
                ShowNodeContextMenu();
            }
            else if (!IsHoveringNode)
            {
                ShowGraphContextMenu();
            }
        }
        private void Ignore()
        {
            //If releasing the mouse outside the window
            if (_cachedEvent.rawType != EventType.MouseUp || _currentActivity != Activity.DraggingGrid) return;
            Repaint();
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
            Repaint();
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

        #region Other

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
        
        private bool IsHoveringTitle(Node node)
        {
            //TODO
            Vector2 mousePosition = _mousePosition;
            Vector2 nodePosition = GridToWindowPosition(node.Position);
            Vector2 size = NodePreferences.STANDARD_NODE_SIZE;
            //if (nodeSizes.TryGetValue(node, out size)) width = size.x;
            //else width = 200;
            Rect windowRect = new Rect(nodePosition, new Vector2(size.x / Zoom, NodePreferences.PROPERTY_HEIGHT / Zoom));
            return windowRect.Contains(mousePosition);
        }

        private void CheckHoveringAndSelection()
        {
            if (_isLayoutEvent) return;
            //Debug.Log("Check Hover");
            //Debug.Log("Node Sizes Count: " + _nodeSizes.Count);
            //Debug.Log("Selection Rect: " + _selectionRect);

            //Reset
            _hoveredNode = null;
            _hoveredConnection = null;
            HoveredPort = null;

            bool isDraggingGrid = _currentActivity == Activity.DraggingGrid;

            //Rect selectionRect = new Rect(WindowToGridPosition(_selectionRect.position), _selectionRect.size);
            Rect selectionRect = _selectionRect;
            Vector2 mousePosition = _mousePosition;
            //Vector2 mousePosition = WindowToGridPosition(_mousePosition);
            //if (isDraggingGrid) _boxSelected = new List<Object>();
            List<Object> boxSelected = new List<Object>();

            for (int i = 0; i < Graph.NodeCount; i++)
            {
                Node node = Graph.GetNode(i);
                NodeEditor nodeEditor = NodeEditor.GetEditor(node);

                //Vector2 nodeSize = _nodeRects[node].size;
                //Rect nodeRect = new Rect(node.Position, new Vector2(nodeEditor.GetWidth(), 4000));
                //Rect nodeRect = new Rect(node.Position, nodeSize);
                Vector2 size;
                if (_nodeSizes.TryGetValue(node, out size))
                {
                    Rect nodeRect = new Rect(node.Position, size);
                    nodeRect = GridToWindowRect(nodeRect);
                    if (nodeRect.Contains(mousePosition)) _hoveredNode = node;
                    if (isDraggingGrid && nodeRect.Overlaps(selectionRect)) boxSelected.Add(node);
                }

                //Check hovering over ports
                var inputNode = node as IInput;
                if (inputNode != null)
                {
                    InputPort input = inputNode.InputPort;
                    Rect inputRect = nodeEditor.GetPortRect(input);
                    inputRect.position += node.Position;
                    inputRect = GridToWindowRect(inputRect);
                    //inputRect.position += node.Position;
                    if (inputRect.Contains(mousePosition)) HoveredPort = input;
                }

                IOutput sOutputNode = node as IOutput;
                if (sOutputNode != null)
                {
                    OutputPort output = sOutputNode.OutputPort;
                    Rect outputRect = nodeEditor.GetPortRect(output);
                    outputRect.position += node.Position;
                    outputRect = GridToWindowRect(outputRect);
                    if (outputRect.Contains(mousePosition)) HoveredPort = output;
                }

                IMultipleOutput mOutputNode = node as IMultipleOutput;
                if (mOutputNode != null)
                {
                    List<OutputPort> outputs = mOutputNode.GetOutputs();
                    foreach (OutputPort output in outputs)
                    {
                        Rect outputRect = nodeEditor.GetPortRect(output);
                        outputRect.position += node.Position;
                        outputRect = GridToWindowRect(outputRect);
                        if (outputRect.Contains(mousePosition)) HoveredPort = output;
                    }
                }
            }

            for (int i = 0; i < Graph.ConnectionCount; i++)
            {
                continue;
                Connection connection = Graph.GetConnection(i);
                Vector2 start = NodeEditor.FindPortRect(connection.Start).center;
                Vector2 end = NodeEditor.FindPortRect(connection.End).center;

                if (NodeUtilities.PointOverlapBezier(mousePosition, start, end, NodePreferences.CONNECTION_WIDTH))
                    _hoveredConnection = connection;

                //TODO: Add range overlap check, as just overlapping might be too annoying.
                if (isDraggingGrid && (_selectionRect.Contains(start) || _selectionRect.Contains(end)))
                    boxSelected.Add(connection);
            }

            //return;
            if (isDraggingGrid)
            {
                if (_cachedEvent.control || _cachedEvent.shift)
                    boxSelected.AddRange(_cachedSelectedObjects);

                Selection.objects = boxSelected.ToArray();

                //string result = "Box Selected: ";
                //boxSelected.ForEach(o => result += "\n" + o.name);
                //Debug.Log(result);
            }
            else _selectionRect = Rect.zero;

            //_selectionRect = Rect.zero;
            //Debug.Log("Hovered Node: " + _hoveredNode);
            //Debug.Log("Dragged Node: " + _draggedNode);
        }

        private void ResetHover()
        {
            if (_isLayoutEvent) return;
            _hoveredNode = null;
            _hoveredConnection = null;
            HoveredPort = null;
        }

        private void NullifyStuff()
        {
            HoveredPort = null;
            _hoveredNode = null;
            _hoveredConnection = null;
            DraggedPort = null;
            DraggedPortTarget = null;
            _draggedNode = null;
        }
        #endregion
    }
}
