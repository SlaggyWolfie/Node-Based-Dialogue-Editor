using System;
using System.Linq;
using WolfEditor.Base;
using UnityEditor;
using UnityEngine;
using WolfEditor.Nodes;
using WolfEditor.Nodes.Base;
using Object = UnityEngine.Object;

namespace WolfEditor.Editor.Nodes
{
    [CustomNodeGraphEditor(typeof(NodeGraph))]
    public class NodeGraphEditor : BaseForCustomEditors<NodeGraphEditor, NodeGraph, CustomNodeGraphEditorAttribute>
    {
        public Rect Rect { get; set; }

        public virtual void OnGUI() { }

        protected void OnNodeRemoval(Node node)
        {
            //Debug.Log("On Node Removal");
            node.PortHandler.AttemptInputAction(input =>
            {
                //Debug.Log("On Node Removal Input");
                var inputConnections = input.InputPort.GetConnections();
                for (int i = inputConnections.Count - 1; i >= 0; i--)
                {
                    //Debug.Log(i);
                    Connection connection = inputConnections[i];
                    //if (connection != null)
                    RemoveConnection(connection);
                }
            });

            node.PortHandler.AttemptOutputAction(output =>
            {
                //Debug.Log("On Node Removal Output");
                var outputConnection = output.OutputPort.Connection;
                //if (outputConnection != null)
                RemoveConnection(outputConnection);
            });

            node.PortHandler.AttemptMultipleOutputAction(output =>
            {
                var outputs = output.GetOutputs();
                foreach (OutputPort outputPort in outputs)
                {
                    var outputConnection = outputPort.Connection;
                    //if (outputConnection != null)
                    RemoveConnection(outputConnection);
                }
            });
        }

        protected void OnConnectionRemoval(Connection connection)
        {
            if (connection == null) return;
            foreach (Instruction connectionModifier in connection.GetModifiers())
                if (connectionModifier != null) RemoveConnectionModifier(connectionModifier);
        }

        public void RemoveNode(Node node)
        {
            //Do not trust Unity.
            OnNodeRemoval(node);
            Target.RemoveNode(node);
            DestroyHelper.Destroy(node);
            //Object.DestroyImmediate(node, true);

            //TODO: Expand Undo Functionality.
            //Undo.DestroyObjectImmediate(node);
        }
        public void RemoveConnection(Connection connection)
        {
            OnConnectionRemoval(connection);
            Target.RemoveConnection(connection);
            DestroyHelper.Destroy(connection);
            //Object.DestroyImmediate(connection, true);
        }
        public void RemoveConnectionModifier(Instruction instruction)
        {
            instruction.Connection.RemoveModifier(instruction);
            DestroyHelper.Destroy(instruction);
            //Object.DestroyImmediate(connectionModifier, true);
        }

        public virtual string GetNodeMenuName(Type type)
        {
            CreateNodeMenuAttribute attribute;
            return ReflectionUtilities.GetAttribute(type, out attribute) ? attribute.menuName :
                UnityEditor.ObjectNames.NicifyVariableName(type.ToString().Replace('.', '/'));
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CustomNodeGraphEditorAttribute : Attribute, IEditorAttribute
    {
        private readonly Type _inspectedType;
        public CustomNodeGraphEditorAttribute(Type inspectedType) { _inspectedType = inspectedType; }
        public Type GetInspectedType() { return _inspectedType; }
    }
}
