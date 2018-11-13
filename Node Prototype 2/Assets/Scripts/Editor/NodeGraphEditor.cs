using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Nodes
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
            DestroyImmediate(node, true);
            Target.RemoveNode(node);
        }

        public virtual void OnGUI() { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CustomNodeGraphEditorAttribute : Attribute, IEditorAttribute
    {
        private readonly Type _inspectedType;
        public CustomNodeGraphEditorAttribute(Type inspectedType) { _inspectedType = inspectedType; }
        public Type GetInspectedType() { return _inspectedType; }
    }
}
