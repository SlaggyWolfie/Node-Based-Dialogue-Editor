using UnityEditor;
using UnityEngine;
using WolfEditor.Nodes.Base;
using WolfEditor.Variables;

[CustomPropertyDrawer(typeof(Condition))]
public class ConditionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
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

        EditorGUI.PropertyField(position,
            useConstant.boolValue ? constantValue : variable,
            GUIContent.none);

        if (EditorGUI.EndChangeCheck()) property.serializedObject.ApplyModifiedProperties();

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}