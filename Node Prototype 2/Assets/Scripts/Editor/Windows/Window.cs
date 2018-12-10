using System;
using System.Collections.Generic;
using System.Linq;
using RPG.Editor.Nodes;
using UnityEditor;
using UnityEngine;

namespace RPG.Editor
{
    public abstract class Window : EditorWindow
    {
        private static Dictionary<WindowEditor, UtilityWindow> _uWindows =
            new Dictionary<WindowEditor, UtilityWindow>();
        private static Dictionary<WindowEditor, ChildWindow> _cWindows =
            new Dictionary<WindowEditor, ChildWindow>();

        public static UtilityWindow GetUtilityWindow<T>(params object[] parameters)
            where T : WindowEditor
        {
            var window = UtilityWindow.OpenWindow<T>(parameters);
            _uWindows[window.Editor] = window;
            return window;
        }
        public static ChildWindow GetChildWindow<T>(Vector2 position, params object[] parameters)
            where T : WindowEditor
        {
            var window =  CurrentWindow.OpenChildWindow<T>(position, parameters);
            _cWindows[window.Editor] = window;
            return window;
        }

        public static void CloseWindow(ChildWindow childWindow)
        {
            childWindow.ParentWindow.CloseChildWindow(childWindow);
        }
        public static void CloseWindow(UtilityWindow utilityWindow)
        {
            utilityWindow.Close();
        }
        public static void CloseWindow(WindowEditor editor)
        {
            ChildWindow cWindow;
            if (_cWindows.TryGetValue(editor, out cWindow))
            {
                CloseWindow(cWindow);
                return;
            }

            UtilityWindow uWindow;
            if (_uWindows.TryGetValue(editor, out uWindow))
            {
                CloseWindow(uWindow);
            }
        }

        private static Window _currentWindow = null;
        public static Window CurrentWindow { get { return _currentWindow; } }

        protected List<ChildWindow> _childWindows = new List<ChildWindow>();

        protected virtual void OnFocus()
        {
            _currentWindow = this;
        }

        protected virtual ChildWindow OpenChildWindow<T>(Vector2 position, params object[] parameters) where T : WindowEditor
        {
            ChildWindow window = ChildWindow.OpenWindow<T>(position, parameters);
            window.ParentWindow = this;
            _childWindows.Add(window);
            return window;
        }

        protected virtual void CloseChildWindow(ChildWindow childWindow)
        {
            if (childWindow != null)
                _childWindows.Remove(childWindow);
        }

        public virtual void DrawChildWindows()
        {
            BeginWindows();

            for (int i = _childWindows.Count - 1; i >= 0; i--)
            {
                //ChildWindow childWindow = _childWindows[i];
                //Debug.Log(string.Format("{0}: {1}", childWindow.Name, childWindow.Rect));
                _childWindows[i].OnGUI();
            }

            EndWindows();
        }

        public virtual void OnGUI()
        {
            DrawChildWindows();
        }
    }
}
