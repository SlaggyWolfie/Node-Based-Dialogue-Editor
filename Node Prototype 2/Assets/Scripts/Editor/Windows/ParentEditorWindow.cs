using System;
using System.Collections.Generic;
using System.Linq;
using RPG.Editor.Nodes;
using UnityEditor;
using UnityEngine;

namespace RPG.Editor
{
    public class ParentEditorWindow : EditorWindow
    {
        protected List<ChildWindow> _subWindows = new List<ChildWindow>();

        public virtual void OpenSubWindow<T>(Vector2 position, params object[] parameters) where T : ChildWindow
        {
            T subWindow = (T)Activator.CreateInstance(typeof(T), parameters);
            subWindow.Rect = new Rect(position, NodePreferences.STANDARD_NODE_SIZE);
            subWindow.Parent = this;

            _subWindows.Add(subWindow);
        }

        public virtual void OpenSubWindow(Type type, Vector2 position, params object[] parameters)
        {
            if (!ReflectionUtilities.IsOfType(type, typeof(ChildWindow))) return;

            ChildWindow subWindow = (ChildWindow)Activator.CreateInstance(type, parameters);
            subWindow.Rect = new Rect(position, NodePreferences.STANDARD_NODE_SIZE);
            subWindow.Parent = this;

            _subWindows.Add(subWindow);
        }

        public virtual void CloseSubWindow(ChildWindow childWindow)
        {
            _subWindows.Remove(childWindow);
        }

        public virtual void DrawSubWindows()
        {
            BeginWindows();

            for (int i = _subWindows.Count - 1; i >= 0; i--)
            {
                ChildWindow childWindow = _subWindows[i];
                //Debug.Log(string.Format("{0}: {1}", childWindow.Name, childWindow.Rect));
                childWindow.OnGUI();
            }

            EndWindows();
        }
    }
}
