﻿using System;
using RPG.Nodes;
using UnityEngine;

namespace RPG.Editor.Nodes
{
    [Serializable, CustomNodeGraphEditor(typeof(NodeGraph))]
    public class NodeGraphEditor : BaseEditor<NodeGraphEditor, NodeGraph, CustomNodeGraphEditorAttribute>
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
            Target.RemoveNode(node);
            UnityEngine.Object.DestroyImmediate(node, true);
            
            //TODO: Expand Undo Functionality.
            //Undo.DestroyObjectImmediate(node);
        }

        public void RemoveConnection(Connection connection)
        {
            Target.RemoveConnection(connection);
            UnityEngine.Object.DestroyImmediate(connection, true);
        }

        public virtual void OnGUI() { }

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
