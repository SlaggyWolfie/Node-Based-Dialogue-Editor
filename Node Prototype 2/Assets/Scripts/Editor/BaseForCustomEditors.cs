using System;
using System.Collections.Generic;
using RPG.Base;
using UnityEditor;

namespace RPG.Editor
{
    public abstract class BaseForCustomEditors<TEditor, TTarget, TAttribute>
        where TEditor : BaseForCustomEditors<TEditor, TTarget, TAttribute>
        where TTarget : BaseScriptableObject
        where TAttribute : IEditorAttribute
    {
        private static Dictionary<TTarget, TEditor> _editors = new Dictionary<TTarget, TEditor>();
        private static Dictionary<Type, Type> _editorTypes = null;
        //private static Dictionary<Type, Type> _editorTypes = new Dictionary<Type, Type>();

        public static TEditor GetEditor(TTarget target)
        {
            if (target == null) return null;

            TEditor editor;
            if (_editors.TryGetValue(target, out editor))
            {
                if (editor._target == null) editor._target = target;
                if (editor._serializedObject == null) editor._serializedObject = new SerializedObject(target);
                return editor;
            }

            Type editorType = GetEditorType(target.GetType());
            editor = (TEditor)Activator.CreateInstance(editorType);
            editor._target = target;
            editor._serializedObject = new SerializedObject(target);

            _editors[target] = editor;

            editor.OnEnable();
            return editor;
        }

        protected static Type GetEditorType(Type targetType)
        {
            if (targetType == null) return null;
            if (_editorTypes == null || _editorTypes.Count == 0) _editorTypes = GetCustomEditors();
            //UnityEngine.Debug.Log("ET: " + (_editorTypes.Count));

            Type result;
            if (_editorTypes.TryGetValue(targetType, out result)) return result;

            //If type isn't found, try base type
            return GetEditorType(targetType.BaseType);
        }

        private static Dictionary<Type, Type> GetCustomEditors()
        {
            Dictionary<Type, Type> dictionary = new Dictionary<Type, Type>();

            Type[] editorTypes = ReflectionUtilities.GetDerivedTypes<TEditor>();
            //Type[] editorTypes = NodeReflection.GetDerivedTypes(typeof(TEditor));
            foreach (Type type in editorTypes)
            {
                if (type.IsAbstract) continue;
                object[] attributes = type.GetCustomAttributes(typeof(TAttribute), false);
                if (attributes.Length == 0) continue;

                TAttribute attribute = (TAttribute)attributes[0];
                dictionary[attribute.GetInspectedType()] = type;
            }

            return dictionary;
        }

        private TTarget _target = null;
        public TTarget Target { get { return _target; } }

        private SerializedObject _serializedObject = null;
        public SerializedObject SerializedObject { get { return _serializedObject; } }

        protected virtual void OnEnable() { }
    }

    public interface IEditorAttribute { Type GetInspectedType(); }
}
