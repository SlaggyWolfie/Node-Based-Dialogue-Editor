using UnityEditor;
using UnityEngine;

namespace WolfEditor.Variables.Editor
{
    [CustomPropertyDrawer(typeof(BoolReference))]
    public sealed class BoolReferenceDrawer : PropertyDrawer
    {
        /// <summary>
        /// Options to display in the popup to select constant or variable.
        /// </summary>
        private static readonly string[] _popupOptions = { "Use Constant", "Use Variable" };

        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle _popupStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_popupStyle == null)
            {
                _popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"))
                { imagePosition = ImagePosition.ImageOnly };
            }

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            //Get properties
            SerializedProperty useConstant = property.FindPropertyRelative("_useConstant");
            SerializedProperty constantValue = property.FindPropertyRelative("_constantValue");
            SerializedProperty variable = property.FindPropertyRelative("_variable");

            //Calculate rect for configuration button
            Rect buttonRect = new Rect(position);
            buttonRect.yMin += _popupStyle.margin.top;
            buttonRect.width = _popupStyle.fixedWidth + _popupStyle.margin.right;
            position.xMin = buttonRect.xMax;

            //Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            int result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 0 : 1, _popupOptions, _popupStyle);

            useConstant.boolValue = result == 0;

            if (useConstant.boolValue)
                constantValue.boolValue = DrawTrueAndFalseToggle(position, constantValue.boolValue, GUI.enabled);
            else EditorGUI.PropertyField(position, variable, GUIContent.none);

            if (EditorGUI.EndChangeCheck()) property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private bool DrawTrueAndFalseToggle(Rect position, bool value, bool enabledGUI)
        {
            Rect left, right; left = right = position;

            left.xMax -= (left.width + EditorGUIUtility.standardVerticalSpacing) / 2;
            right.xMin += (right.width + EditorGUIUtility.standardVerticalSpacing) / 2;

            //Draw Left Toggle
            if (value) GUI.enabled = false;
            value = GUI.Toggle(left, value, "True", GUI.skin.button);
            GUI.enabled = enabledGUI;

            //Draw Right Toggle
            if (!value) GUI.enabled = false;
            value = !GUI.Toggle(right, !value, "False", GUI.skin.button);
            GUI.enabled = enabledGUI;

            return value;
        }
    }
}