using System;
using RPG.Editor.Nodes;
using RPG.Nodes.Base;
using UnityEditor;
using UnityEngine;

namespace RPG.Editor
{
    [CustomConnectionModifierEditor(typeof(ConnectionModifier))]
    public class ConnectionModifierEditor : BaseForCustomEditors<ConnectionModifierEditor, ConnectionModifier, CustomConnectionModifierEditorAttribute>
    {
        public virtual void OnGUI()
        {
            DefaultConnectionModifierGUI();
        }

        protected void DefaultConnectionModifierGUI()
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
    public class CustomConnectionModifierEditorAttribute : Attribute, IEditorAttribute
    {
        private readonly Type _inspectedType;
        public CustomConnectionModifierEditorAttribute(Type inspectedType) { _inspectedType = inspectedType; }
        public Type GetInspectedType() { return _inspectedType; }
    }
}
