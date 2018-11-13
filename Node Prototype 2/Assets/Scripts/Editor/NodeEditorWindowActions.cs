using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using RPG.Nodes;
using Object = UnityEngine.Object;

namespace RPG.Nodes
{
    public sealed partial class NodeEditorWindow
    {
        private void HandleEvents(Event e)
        {
            throw new NotImplementedException();
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
