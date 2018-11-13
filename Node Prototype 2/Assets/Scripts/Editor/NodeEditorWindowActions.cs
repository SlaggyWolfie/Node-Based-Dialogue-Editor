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
        private void HandleEvents(Event e)
        {
            wantsMouseMove = true;

            switch (_cachedEvent.type)
            {
                case EventType.MouseMove: break;
                case EventType.ScrollWheel: ScrollWheel(); break;
                case EventType.MouseDrag: MouseDrag(); break;
                case EventType.MouseDown: MouseDown();break;
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
                    else if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX && e.commandName == "Delete") RemoveSelectedNodes();
                    else if (_cachedEvent.commandName == "Duplicate") DuplicateSelectedNodes();
                    _shouldRepaint = true;
                    break;
                case EventType.Ignore:
                    //If releasing the mouse outside the window
                    if (_cachedEvent.rawType == EventType.MouseUp && _currentActivity == Activity.DraggingGrid)
                    {
                        _shouldRepaint = true;
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
            if (_leftClick)
            {
                if (IsDraggingPort)
                {
                    if (IsHoveringPort && _hoveredPort is InputPort)
                    {
                        if (!_draggedPort.IsConnectedTo(_hoveredPort))
                            _draggedOutputTarget = _hoveredPort;
                    }
                    else
                    {
                        _draggedOutputTarget = null;
                    }
                    _shouldRepaint = true;
                }
                else if (_currentActivity == Activity.HoldingNode)
                {
                    RecalculateDragOffsets(_cachedEvent);
                    _currentActivity = Activity.DraggingNode;
                    _shouldRepaint = true;
                }

                if (_currentActivity == Activity.DraggingNode)
                {
                    //Holding CTRL inverts grid snap
                    //bool gridSnap = NodeEditorPreferences.GetSettings().gridSnap;
                    //if (_cachedEvent.control) gridSnap = !gridSnap;

                    Vector2 mousePosition = _cachedEvent.mousePosition;
                    //Vector2 mousePosition = WindowToGridPosition(_cachedEvent.mousePosition);
                    //Move selected nodes with offset

                    Node[] selectedNodes = SelectedNodes;
                    for (int i = 0; i < selectedNodes.Length; i++)
                    {
                        Node node = selectedNodes[i];
                        Vector2 initial = node.Position;
                        node.Position = mousePosition + dragOffset[i];

                        //if (gridSnap)
                        //{
                        //    node.position.x = (Mathf.Round((node.position.x + 8) / 16) * 16) - 8;
                        //    node.position.y = (Mathf.Round((node.position.y + 8) / 16) * 16) - 8;
                        //}

                        //Offset portConnectionPoints instantly if a node is dragged so they aren't delayed by a frame.
                        Vector2 offset = node.Position - initial;
                        if (offset.sqrMagnitude <= 0) continue;

                        foreach (NodePort output in node.Outputs)
                        {
                            Rect rect;
                            if (portConnectionPoints.TryGetValue(output, out rect))
                            {
                                rect.position += offset;
                                portConnectionPoints[output] = rect;
                            }
                        }

                        foreach (NodePort input in node.Inputs)
                        {
                            Rect rect;
                            if (portConnectionPoints.TryGetValue(input, out rect))
                            {
                                rect.position += offset;
                                portConnectionPoints[input] = rect;
                            }
                        }
                    }

                    // Move selected reroutes with offset
                    for (int i = 0; i < selectedReroutes.Count; i++)
                    {
                        Vector2 pos = mousePosition + dragOffset[Selection.objects.Length + i];
                        if (gridSnap)
                        {
                            pos.x = (Mathf.Round(pos.x / 16) * 16);
                            pos.y = (Mathf.Round(pos.y / 16) * 16);
                        }
                        selectedReroutes[i].SetPoint(pos);
                    }
                    _shouldRepaint = true;
                }
                else if (_currentActivity == Activity.HoldingGrid)
                {
                    _currentActivity = Activity.DraggingGrid;
                    preBoxSelection = Selection.objects;
                    preBoxSelectionReroute = selectedReroutes.ToArray();
                    _dragStart = _cachedEvent.mousePosition;
                    //_dragBoxStart = WindowToGridPosition(_cachedEvent.mousePosition);
                    _shouldRepaint = true;
                }
                else if (_currentActivity == Activity.DraggingGrid)
                {
                    Vector2 boxStartPosition = _dragStart;
                    //Vector2 boxStartPosition = GridToWindowPosition(_dragBoxStart);
                    Vector2 boxSize = _cachedEvent.mousePosition - boxStartPosition;
                    if (boxSize.x < 0) { boxStartPosition.x += boxSize.x; boxSize.x = Mathf.Abs(boxSize.x); }
                    if (boxSize.y < 0) { boxStartPosition.y += boxSize.y; boxSize.y = Mathf.Abs(boxSize.y); }
                    _selectionRect = new Rect(boxStartPosition, boxSize);
                    _shouldRepaint = true;
                }
            }
            else if (_rightClick || _middleClick)
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
            if (!_leftClick) return;

            draggedOutputReroutes.Clear();

            if (IsHoveringPort)
            {
                if (_hoveredPort is OutputPort)
                {
                    _draggedPort = _hoveredPort;
                }
                else
                {
                    //hoveredPort.VerifyConnections();
                    if (_hoveredPort.IsConnected)
                    {
                        Node node = _hoveredPort.Node;
                        NodePort output = hoveredPort.Connection;
                        int outputConnectionIndex = output.GetConnectionIndex(hoveredPort);
                        draggedOutputReroutes = output.GetReroutePoints(outputConnectionIndex);
                        _hoveredPort.Disconnect(output);
                        _draggedOutput = output;
                        draggedOutputTarget = hoveredPort;
                        if (NodeEditor.onUpdateNode != null) NodeEditor.onUpdateNode(node);
                    }
                }
            }
            else if (IsHoveringNode && IsHoveringTitle(_hoveredNode))
            {
                if (!Selection.Contains(_hoveredNode))
                {
                    SelectNode(_hoveredNode, _cachedEvent.control || _cachedEvent.shift);
                    if (!_cachedEvent.control && !_cachedEvent.shift) selectedReroutes.Clear();
                }
                else if (_cachedEvent.control || _cachedEvent.shift) DeselectNode(_hoveredNode);

                _cachedEvent.Use();
                _currentActivity = Activity.HoldingNode;
            }
            else if (IsHoveringReroute)
            {
                //If reroute isn't selected
                if (!selectedReroutes.Contains(hoveredReroute))
                {
                    // Add it
                    if (_cachedEvent.control || _cachedEvent.shift) selectedReroutes.Add(hoveredReroute);
                    // Select it
                    else
                    {
                        selectedReroutes = new List<RerouteReference>() { hoveredReroute };
                        Selection.activeObject = null;
                    }

                }
                //Deselect
                else if (_cachedEvent.control || _cachedEvent.shift) selectedReroutes.Remove(hoveredReroute);
                _cachedEvent.Use();
                _currentActivity = Activity.HoldingNode;
            }
            //If mousedown on grid background, deselect all
            else if (!IsHoveringNode)
            {
                _currentActivity = Activity.HoldingGrid;
                if (!_cachedEvent.control && !_cachedEvent.shift)
                {
                    selectedReroutes.Clear();
                    Selection.activeObject = null;
                }
            }
        }

        private void MouseUp()
        {
            if (_cachedEvent.button == 0)
            {
                if (IsDraggingPort)
                {
                    //If connection is valid, save it
                    if (draggedOutputTarget != null)
                    {
                        Node node = draggedOutputTarget.node;
                        if (graph.nodes.Count != 0) draggedOutput.Connect(draggedOutputTarget);

                        // ConnectionIndex can be -1 if the connection is removed instantly after creation
                        int connectionIndex = draggedOutput.GetConnectionIndex(draggedOutputTarget);
                        if (connectionIndex != -1)
                        {
                            draggedOutput.GetReroutePoints(connectionIndex).AddRange(draggedOutputReroutes);
                            if (NodeEditor.onUpdateNode != null) NodeEditor.onUpdateNode(node);
                            EditorUtility.SetDirty(graph);
                        }
                    }
                    //Release dragged connection
                    draggedOutput = null;
                    draggedOutputTarget = null;
                    EditorUtility.SetDirty(graph);
                    NodeUtilities.AutoSaveAssets();
                }
                else if (_currentActivity == Activity.DraggingNode)
                {
                    IEnumerable<Node> nodes = Selection.objects.Where(x => x is Node).Select(x => x as Node);
                    foreach (Node node in nodes) EditorUtility.SetDirty(node);
                    NodeUtilities.AutoSaveAssets();
                }
                else if (!IsHoveringNode)
                {
                    // If click outside node, release field focus
                    if (!isPanning)
                    {
                        EditorGUI.FocusTextInControl(null);
                    }
                    NodeUtilities.AutoSaveAssets();
                }

                // If click node header, select it.
                if (_currentActivity == Activity.HoldingNode && !(_cachedEvent.control || e.shift))
                {
                    selectedReroutes.Clear();
                    SelectNode(hoveredNode, false);
                }

                // If click reroute, select it.
                if (IsHoveringReroute && !(_cachedEvent.control || e.shift))
                {
                    selectedReroutes = new List<RerouteReference>() { hoveredReroute };
                    Selection.activeObject = null;
                }

                _shouldRepaint = true;
                _currentActivity = Activity.Idle;
            }
            else if (_leftClick || _rightClick)
            {
                if (!IsPanning)
                {
                    if (IsDraggingPort)
                    {
                        draggedOutputReroutes.Add(WindowToGridPosition(_cachedEvent.mousePosition));
                    }
                    else if (_currentActivity == Activity.DraggingNode && Selection.activeObject == null && selectedReroutes.Count == 1)
                    {
                        selectedReroutes[0].InsertPoint(selectedReroutes[0].GetPoint());
                        selectedReroutes[0] = new RerouteReference(selectedReroutes[0].port, selectedReroutes[0].connectionIndex, selectedReroutes[0].pointIndex + 1);
                    }
                    else if (IsHoveringReroute)
                    {
                        NodeContextMenus.ShowRerouteContextMenu(hoveredReroute);
                    }
                    else if (IsHoveringPort)
                    {
                        NodeContextMenus.ShowPortContextMenu(hoveredPort);
                    }
                    else if (IsHoveringNode && IsHoveringTitle(hoveredNode))
                    {
                        if (!Selection.Contains(hoveredNode)) SelectNode(hoveredNode, false);
                        NodeContextMenus.ShowNodeContextMenu();
                    }
                    else if (!IsHoveringNode)
                    {
                        NodeContextMenus.ShowGraphContextMenu();
                    }
                }
                IsPanning = false;
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

        //private bool ShouldBeCulled(Node node)
        //{

        //    Vector2 nodePos = GridToWindowPosition(node.position);
        //    if (nodePos.x / _zoom > position.width) return true; // Right
        //    else if (nodePos.y / _zoom > position.height) return true; // Bottom
        //    else if (nodeSizes.ContainsKey(node))
        //    {
        //        Vector2 size = nodeSizes[node];
        //        if (nodePos.x + size.x < 0) return true; // Left
        //        else if (nodePos.y + size.y < 0) return true; // Top
        //    }
        //    return false;
        //}
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
            //node.position = position;
            node.name = ObjectNames.NicifyVariableName(type.Name);

            AssetDatabase.AddObjectToAsset(node, _graph);
            NodeUtilities.AutoSaveAssets();
            Repaint();
        }

        public void RemoveSelectedNodes()
        {
        }

        public void SendToFront(Node node)
        {
        }

        public void DuplicateSelectedNodes()
        {
        }
        #endregion
    }
}
