using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Nodes.Editor
{
    [CustomNodeGraphEditor(typeof(NodeGraph))]
    public abstract class NodeGraphEditor : BaseEditor<NodeGraphEditor, NodeGraph, CustomNodeGraphEditorAttribute>
    {
        private Rect _rectangle;
        public Rect Rectangle
        {
            get { return _rectangle; }
            set { _rectangle = value; }
        }

        public Node CopyNode(Node original)
        {
            Node node = Target.CopyNode(original);
            node.name = original.name;
            UnityEditor.AssetDatabase.AddObjectToAsset(node, Target);
            return node;
        }
        
        public void RemoveNode(Node node)
        {
            UnityEngine.Object.DestroyImmediate(node, true);
            Target.RemoveNode(node);
        }

        public void RemoveConnection(Connection connection)
        {
            UnityEngine.Object.DestroyImmediate(connection, true);
            Target.RemoveConnection(connection);
        }

        public virtual void OnGUI() { }

        public virtual string GetNodeMenuName(Type type)
        {
            CreateNodeMenuAttribute attribute;
            return NodeReflection.GetAttribute(type, out attribute) ? attribute.menuName : 
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
