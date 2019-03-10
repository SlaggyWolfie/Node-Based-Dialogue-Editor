using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WolfEditor.Nodes;
using WolfEditor.Nodes.Base;
using WolfEditor.Utility;
using WolfEditor.Utility.Editor;
using Object = UnityEngine.Object;
using ScrObject = WolfEditor.Base.BaseScriptableObject;
//using ScrObj = UnityEngine.Ba;

namespace WolfEditor.Editor.Nodes
{
    public sealed partial class NodeEditorWindow
    {
        private void HandleGUIEvents()
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
                    //case EventType.MouseMove: break;
                    //case EventType.KeyUp: break;
                    //case EventType.Repaint: break;
                    //case EventType.Layout: break;
                    //case EventType.DragUpdated: break;
                    //case EventType.DragPerform: break;
                    //case EventType.DragExited: break;
                    //case EventType.Used: break;
                    //case EventType.ExecuteCommand: break;
                    //case EventType.ContextClick: break;
                    //case EventType.MouseEnterWindow: break;
                    //case EventType.MouseLeaveWindow: break;
                    //default: throw new ArgumentOutOfRangeException();
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

                                node.Position = ApplyGridSnap(gridSnap, node.Position);

                                Vector2 offset = node.Position - initial;
                                if (offset.sqrMagnitude <= 0) continue;
                                //node.OffsetPorts(offset);
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

        private static Vector2 ApplyGridSnap(bool gridSnap, Vector2 point)
        {
            if (!gridSnap) return point;

            const float gridOffset = NodePreferences.GRID_SIZE / 8.0f;
            const float rounding = NodePreferences.GRID_SIZE / 4.0f;
            point.x = Mathf.Round((point.x + gridOffset) / rounding) * rounding - gridOffset;
            point.y = Mathf.Round((point.y + gridOffset) / rounding) * rounding - gridOffset;

            return point;
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

                if (IsHoveringConnectionModifier && IsHoveringTitle(_hoveredInstruction))
                {
                    if (!Selection.Contains(_hoveredInstruction))
                        Select(_hoveredInstruction, controlOrShift);
                    else if (controlOrShift) Deselect(_hoveredInstruction);

                    _cachedEvent.Use();
                    _currentActivity = Activity.Holding;
                }
                else if (IsHoveringConnection)
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
                EditorUtilities.AutoSaveAssets();
            }
            else if (_currentActivity == Activity.Dragging)
            {
                Node[] nodes = GetSelected<Node>();
                foreach (Node node in nodes) EditorUtility.SetDirty(node);
                EditorUtilities.AutoSaveAssets();
            }
            else if (!IsHoveringConnectionModifier)
            {
                //If clicking outside the Con. Mod, release the field focus
                if (!IsPanning) EditorGUI.FocusTextInControl(null);
                EditorUtilities.AutoSaveAssets();
            }
            else if (!IsHoveringNode)
            {
                //If clicking outside the node, release the field focus
                if (!IsPanning) EditorGUI.FocusTextInControl(null);
                EditorUtilities.AutoSaveAssets();
            }

            if (_currentActivity == Activity.Holding && !(_cachedEvent.control || _cachedEvent.shift))
            {
                if (IsHoveringNode)
                    Select(_hoveredNode, false);
                else if (IsHoveringConnectionModifier)
                    Select(_hoveredInstruction, false);
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
            else if (DifferentObjectsSelected())
            {
                ShowDifferentObjectsContextMenu();
            }
            else if (IsHoveringConnectionModifier && IsHoveringTitle(_hoveredInstruction))
            {
                if (!Selection.Contains(_hoveredInstruction)) Select(_hoveredInstruction, false);
                ShowConnectionModifierContextMenu();
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
            //if (_cachedEvent.commandName == "SoftDelete") RemoveSelectedNodes();
            //else if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX && _cachedEvent.commandName == "Delete")
            //    RemoveSelectedNodes();
            //else if (_cachedEvent.commandName == "Duplicate") DuplicateSelectedNodes();
            if (_cachedEvent.commandName == "SoftDelete") RemoveSelected();
            else if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX && _cachedEvent.commandName == "Delete")
                RemoveSelected();
            else if (_cachedEvent.commandName == "Duplicate") DuplicateSelected(); ;
            Repaint();
        }
        private void KeyDown()
        {
            if (EditorGUIUtility.editingTextField) return;
            if (_cachedEvent.keyCode == KeyCode.F) Home();
            else if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX && _cachedEvent.keyCode == KeyCode.Return)
                RenameSelectedNode();
            else if (_cachedEvent.keyCode == KeyCode.F2) RenameSelectedNode();
        }
        #endregion
        
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
        public Vector2 WindowToWindowPositionNotClipped(Vector2 windowPosition)
        {
            return windowPosition * Zoom;
        }

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
            //DONE
            Vector2 mousePosition = _mousePosition;
            Vector2 nodePosition = GridToWindowPosition(node.Position);
            Vector2 size;
            if (!_nodeSizes.TryGetValue(node, out size)) size = NodePreferences.STANDARD_NODE_SIZE;
            Rect titleRect = new Rect(nodePosition, new Vector2(size.x / Zoom, NodePreferences.PROPERTY_HEIGHT / Zoom));
            return titleRect.Contains(mousePosition);
        }

        private bool IsHoveringTitle(Instruction mod)
        {
            Vector2 mousePosition = _mousePosition;
            Connection connection = mod.Connection;
            int index = connection.GetIndex(mod);

            Rect[] rects;
            bool found = _connectionModifierRects.TryGetValue(connection, out rects);
            if (!found || index >= rects.Length) return false;

            Rect modRect = GridToWindowRect(rects[index]);
            Rect titleRect = new Rect(modRect.position, new Vector2(modRect.width / Zoom, NodePreferences.PROPERTY_HEIGHT / Zoom));
            return titleRect.Contains(mousePosition);
        }

        private void CheckHoveringAndSelection()
        {
            if (_isLayoutEvent) return;

            ResetHover();

            bool isDraggingGrid = _currentActivity == Activity.DraggingGrid;

            Rect selectionRect = _selectionRect;
            Vector2 mousePosition = _mousePosition;
            List<Object> boxSelected = new List<Object>();

            //TODO Investigate reverse recognition not working!?
            //Never mind it works, it's just my architecture works bottom-top,
            //instead of top-bottom-or-stop
            //Why? Cause hovering
            //TODO Investigate alternatives for conversion
            for (int i = 0; i < Graph.NodeCount; i++)
            {
                Node node = Graph.GetNode(i);
                NodeEditor nodeEditor = NodeEditor.GetEditor(node);

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
                    //inputRect.position += node.Position;
                    inputRect = GridToWindowRect(inputRect);
                    if (inputRect.Contains(mousePosition)) HoveredPort = input;
                }

                IOutput sOutputNode = node as IOutput;
                if (sOutputNode != null)
                {
                    OutputPort output = sOutputNode.OutputPort;
                    Rect outputRect = nodeEditor.GetPortRect(output);
                    //outputRect.position += node.Position;
                    outputRect = GridToWindowRect(outputRect);
                    if (outputRect.Contains(mousePosition)) HoveredPort = output;
                }

                IMultipleOutput mOutputNode = node as IMultipleOutput;
                if (mOutputNode != null)
                {
                    var outputs = mOutputNode.GetOutputs();
                    foreach (OutputPort output in outputs)
                    {
                        Rect outputRect = nodeEditor.GetPortRect(output);
                        //outputRect.position += node.Position;
                        outputRect = GridToWindowRect(outputRect);
                        if (outputRect.Contains(mousePosition)) HoveredPort = output;
                    }
                }
            }

            for (int i = 0; i < Graph.ConnectionCount; i++)
            {
                Connection connection = Graph.GetConnection(i);
                if (connection == null)
                {
                    Debug.Log("Null connection at index " + i);
                    OnNull(Graph);
                    continue;
                }

                Vector2 start = NodeEditor.FindPortRect(connection.Start).center;
                Vector2 end = NodeEditor.FindPortRect(connection.End).center;
                start = GridToWindowPosition(start);
                end = GridToWindowPosition(end);

                //if (OtherUtilities.PointOverlapBezier(mousePosition, start, end, NodePreferences.CONNECTION_WIDTH))
                if (LineSegment.WideLineSegmentPointCheck(mousePosition, start, end, NodePreferences.CONNECTION_WIDTH * 2 / Zoom))
                    _hoveredConnection = connection;

                //DONE: Add range percentage overlap check, as just overlapping might be too annoying.
                if (isDraggingGrid && LineSegment.LineRectOverlapPercentageCheck(selectionRect, start, end) > 0.3f)
                    boxSelected.Add(connection);


                Rect[] modifierRects;
                if (!_connectionModifierRects.TryGetValue(connection, out modifierRects)) continue;

                for (int j = 0; j < connection.ModifierCount; j++)
                {
                    Instruction mod = connection.GetModifier(j);
                    Rect rect = GridToWindowRect(modifierRects[j]);
                    if (rect.Contains(mousePosition)) _hoveredInstruction = mod;
                    if (isDraggingGrid && rect.Overlaps(selectionRect)) boxSelected.Add(mod);
                }
            }

            //return;
            if (isDraggingGrid)
            {
                if (_cachedEvent.control || _cachedEvent.shift) boxSelected.AddRange(_cachedSelectedObjects);
                Selection.objects = boxSelected.ToArray();
            }
            else _selectionRect = Rect.zero;
        }

        private void ResetHover()
        {
            if (_isLayoutEvent) return;
            _hoveredNode = null;
            _hoveredConnection = null;
            _hoveredInstruction = null;
            HoveredPort = null;
        }

        private void ResetHoverAndDrag()
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
