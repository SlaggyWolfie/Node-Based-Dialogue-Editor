using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RPG.Nodes.Base;
using UnityEditor;
using UnityEngine;

namespace RPG.Nodes
{
    public abstract class BaseEditor<TEditor, TTarget, TAttribute> : ScriptableObjectWithID
        where TEditor : BaseEditor<TEditor, TTarget, TAttribute>
        where TTarget : ScriptableObjectWithID
        where TAttribute : IEditorAttribute
    {
        private static Dictionary<TTarget, TEditor> _editors = new Dictionary<TTarget, TEditor>();
        private static Dictionary<Type, Type> _editorTypes = null;

        public static TEditor GetEditor(TTarget target)
        {
            if (target == null) return null;

            TEditor editor;
            if (_editors.TryGetValue(target, out editor))
            {
                //if (editor._target == null) editor._target = target;
                //if (editor._serializedObject == null) editor._serializedObject = new SerializedObject(target);
                return editor;
            }
            
            Type editorType = GetEditorType(target.GetType());
            editor = (TEditor)Activator.CreateInstance(editorType);
            editor._target = target;
            editor._serializedObject = new SerializedObject(target);

            _editors[target] = editor;
            return editor;
        }

        protected static Type GetEditorType(Type targetType)
        {
            if (targetType == null) return null;
            if (_editorTypes == null) _editorTypes = GetCustomEditors();

            Type result;
            if (_editorTypes.TryGetValue(targetType, out result)) return result;

            //If type isn't found, try base type
            return GetEditorType(targetType.BaseType);
        }

        private static Dictionary<Type, Type> GetCustomEditors()
        {
            var dictionary = new Dictionary<Type, Type>();

            Type[] editorTypes = NodeReflection.GetDerivedTypes<TEditor>();
            foreach (Type type in editorTypes)
            {
                if (type.IsAbstract) continue;
                object[] attributes = type.GetCustomAttributes(typeof(TAttribute), false);
                if (attributes.Length == 0) continue;

                TAttribute attribute = (TAttribute)attributes.First();
                dictionary[attribute.GetInspectedType()] = type;
            }

            return dictionary;
        }

        private TTarget _target = null;
        public TTarget Target { get { return _target; } }

        private SerializedObject _serializedObject = null;
        public SerializedObject SerializedObject { get { return _serializedObject; } }
    }

    public interface IEditorAttribute { Type GetInspectedType(); }
}
