using System;
using UnityEditor;
using UnityEngine;
using WolfEditor.Editor.Nodes;
using WolfEditor.Nodes.Base;

namespace WolfEditor.Editor
{
    [CustomInstructionEditor(typeof(Instruction))]
    public class InstructionEditor : BaseForCustomEditors<InstructionEditor,
        Instruction, CustomInstructionEditorAttribute>
    {
        public virtual void OnHeaderGUI()
        {
            EditorGUILayout.LabelField(Target.name, NodeResources.Styles.nodeHeader, GUILayout.Height(NodePreferences.PROPERTY_HEIGHT));
        }

        public virtual void OnBodyGUI()
        {
            DefaultConnectionModifierBodyGUI();
        }

        protected void DefaultConnectionModifierBodyGUI()
        {
            SerializedObject.Update();

            bool enterChildren = true;
            EditorGUIUtility.labelWidth = 84;
            for (SerializedProperty i = SerializedObject.GetIterator();
                i.NextVisible(enterChildren);
                enterChildren = false)
            {
                if (i.name == "m_Script") continue;
                EditorGUILayout.PropertyField(i, true, GUILayout.MinWidth(NodePreferences.PROPERTY_MIN_WIDTH));
            }

            SerializedObject.ApplyModifiedProperties();
        }

        public virtual float GetWidth()
        {
            return NodePreferences.STANDARD_NODE_SIZE.x;
        }
        public virtual Color GetTint()
        {
            return Color.white;
        }
        public virtual GUIStyle GetBodyStyle()
        {
            return NodeResources.Styles.nodeBody;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CustomInstructionEditorAttribute : Attribute, IEditorAttribute
    {
        private readonly Type _inspectedType;
        public CustomInstructionEditorAttribute(Type inspectedType) { _inspectedType = inspectedType; }
        public Type GetInspectedType() { return _inspectedType; }
    }
}
