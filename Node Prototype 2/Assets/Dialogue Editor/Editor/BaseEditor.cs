using System;
using System.Collections.Generic;
using System.Linq;
using RPG.Base;
using RPG.Nodes.Base;
using UnityEditor;

namespace RPG.Editor
{
    public abstract class BaseEditor<TEditor, TTarget>
        where TEditor : BaseEditor<TEditor, TTarget>
        where TTarget : BaseScriptableObject
    {
        private static Dictionary<TTarget, TEditor> _editors = new Dictionary<TTarget, TEditor>();

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
            
            editor = (TEditor)Activator.CreateInstance(typeof(TEditor));
            editor._target = target;
            editor._serializedObject = new SerializedObject(target);

            _editors[target] = editor;

            editor.OnEnable();
            return editor;
        }
        
        private TTarget _target = null;
        public TTarget Target { get { return _target; } }

        private SerializedObject _serializedObject = null;
        public SerializedObject SerializedObject { get { return _serializedObject; } }

        protected virtual void OnEnable() { }
    }
}
