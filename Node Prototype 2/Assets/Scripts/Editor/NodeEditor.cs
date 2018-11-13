using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RPG.Nodes
{
    [CustomNodeEditor(typeof(Node))]
    public abstract class NodeEditor : BaseEditor<NodeEditor, Node, CustomNodeEditorAttribute>
    {
        public static Action<Node> onUpdateNode;

        private Rect _rectangle;
        public Rect Rectangle
        {
            get { return _rectangle; }
            set { _rectangle = value; }
        }

        protected virtual void OnValidate() { }

        public virtual void OnHeaderGUI()
        {
            string title = Target.name;
            //throw new NotImplementedException();
            GUILayout.Label(title, GUILayout.Height(30));
        }

        public virtual void OnBodyGUI()
        {
            DrawDefaultNodeEditorBody();
        }

        protected void DrawDefaultNodeEditorBody()
        {
            SerializedObject.Update();

            bool enterChildren = true;
            for (SerializedProperty i = SerializedObject.GetIterator();
                i.NextVisible(enterChildren);
                enterChildren = false)
                EditorGUILayout.PropertyField(i, true);

            SerializedObject.ApplyModifiedProperties();
        }

        public abstract Rect GetPortRect(Port port);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CustomNodeEditorAttribute : Attribute, IEditorAttribute
    {
        private readonly Type _inspectedType;
        public CustomNodeEditorAttribute(Type inspectedType) { _inspectedType = inspectedType; }
        public Type GetInspectedType() { return _inspectedType; }
    }
}
