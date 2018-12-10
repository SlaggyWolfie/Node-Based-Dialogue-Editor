using System;
using System.Collections.Generic;
using System.Linq;
using RPG.Editor.Nodes;
using UnityEditor;
using UnityEngine;

namespace RPG.Editor
{
    public sealed class UtilityWindow : EditorWindow, IWindowEditorContainer
    {
        public static UtilityWindow OpenWindow<TBehaviour>(params object[] parameters)
            where TBehaviour : WindowEditor
        {
            string name = ObjectNames.NicifyVariableName(typeof(TBehaviour).Name).Replace("Editor", "");
            var window = GetWindow<UtilityWindow>(true, name);
            window.wantsMouseMove = true;
            TBehaviour behaviour = (TBehaviour)Activator.CreateInstance(typeof(TBehaviour), parameters);
            window.name = behaviour.Name;
            window._editor = behaviour;
            return window;
        }

        private WindowEditor _editor = null;
        public WindowEditor Editor { get { return _editor; } }

        private void OnGUI()
        {
            Editor.OnGUI();
            //position = new Rect(position.position, Behaviour.Rect.size);
        }
    }
}
