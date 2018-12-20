using UnityEditor;
using UnityEngine;

namespace RPG.Other
{
    [CustomPropertyDrawer(typeof(ResizableTextAreaAttribute))]
    public class ResizableTextAreaPropertyDrawer : PropertyDrawer
    {
        private static readonly float minimumTextBoxHeight = EditorGUIUtility.singleLineHeight * 2;
        private static readonly float minimumHeight = minimumTextBoxHeight + EditorGUIUtility.singleLineHeight +
                                                      2 * EditorGUIUtility.standardVerticalSpacing;
        private static readonly GUIStyle style = new GUIStyle(EditorStyles.textArea) { wordWrap = true };

        private float _height = minimumHeight;
        private float _textBoxRectHeight = minimumTextBoxHeight;

        //private Rect _labelRect, _textBoxRect;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String) return;

            Rect labelRect = position;
            Rect textBoxRect = position;

            labelRect.height = EditorGUIUtility.singleLineHeight;
            textBoxRect.y += labelRect.height + EditorGUIUtility.standardVerticalSpacing;
            Recalculate(position, property, ref textBoxRect, labelRect);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PrefixLabel(labelRect, label);
            EditorGUI.BeginChangeCheck();
            string textAreaValue = EditorGUI.TextArea(textBoxRect, property.stringValue, style);
            //string textAreaValue = EditorGUILayout.TextArea(property.stringValue, GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 2));
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = textAreaValue;
                Recalculate(position, property, ref textBoxRect, labelRect);
            }
            EditorGUI.EndProperty();
        }

        private void Recalculate(Rect position, SerializedProperty property, ref Rect textBoxRect, Rect labelRect)
        {
            RecalculateHeight(position, property);
            textBoxRect.height = _textBoxRectHeight;
            _height = labelRect.height + textBoxRect.height + 2 * EditorGUIUtility.standardVerticalSpacing;
        }

        private void RecalculateHeight(Rect position, SerializedProperty property)
        {
            float calculatedHeight = style.CalcHeight(new GUIContent(property.stringValue), position.width);
            _textBoxRectHeight = Mathf.Max(minimumTextBoxHeight, calculatedHeight);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _height;
        }

        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return true;
        }
    }
}