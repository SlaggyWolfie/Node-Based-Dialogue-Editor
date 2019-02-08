using System;
using System.Collections.Generic;
using System.Linq;
using RPG.Editor.Nodes;
using UnityEditor;
using UnityEngine;

namespace RPG.Editor
{
    public sealed class ChildWindow : IWindowEditorContainer
    {
        public static ChildWindow OpenWindow<T>(Vector2 position, params object[] parameters)
            where T : WindowEditor
        {
            ChildWindow window = Activator.CreateInstance<ChildWindow>();
            window._editor = (T)Activator.CreateInstance(typeof(T), parameters);
            window._editor.Rect = new Rect(position, NodePreferences.STANDARD_NODE_SIZE);
            return window;
        }

        public static ChildWindow OpenWindow<T>(Window parent,Vector2 position, params object[] parameters)
            where T : WindowEditor
        {
            ChildWindow window = OpenWindow<T>(position, parameters);
            window.ParentWindow = parent;
            return window;
        }

        private Window _parentWindow = null;
        public Window ParentWindow
        {
            get { return _parentWindow; }
            set { _parentWindow = value; }
        }

        private WindowEditor _editor = null;
        public WindowEditor Editor { get { return _editor; } }

        public void OnGUI()
        {
            Editor.Rect = GUILayout.Window(1, Editor.Rect, OnGUI, Editor.Name);
        }
        public void OnGUI(int unusedWindowID)
        {
            Editor.OnGUI();
            GUI.DragWindow();
        }
    }

    //public sealed class ChildWindow<T> : ChildWindow, IWindowBehaviourContainer<T> where T : WindowBehaviour
    //{
    //    //public static ChildWindow<TBehaviour> OpenWindow<TBehaviour>(Vector2 position, params object[] parameters)
    //    //    where TBehaviour : WindowBehaviour
    //    //{
    //    //    ChildWindow<TBehaviour> window = (ChildWindow<TBehaviour>)Activator.CreateInstance(typeof(ChildWindow<TBehaviour>));
    //    //    window._behaviour = (TBehaviour)Activator.CreateInstance(typeof(TBehaviour), parameters);
    //    //    window._behaviour.Rect = new Rect(position, NodePreferences.STANDARD_NODE_SIZE);
    //    //    return window;
    //    //}

    //    //public static ChildWindow<TBehaviour> OpenWindow<TBehaviour>(Window parent, Vector2 position, params object[] parameters)
    //    //    where TBehaviour : WindowBehaviour
    //    //{
    //    //    var window = OpenWindow<TBehaviour>(position, parameters);
    //    //    window._parentWindow = parent;
    //    //    return window;
    //    //}

    //    //private T _behaviour = null;
    //    //public T Behaviour { get { return _behaviour; } }

    //    //public override void OnGUI() { Behaviour.OnGUI(); }
    //}
}
