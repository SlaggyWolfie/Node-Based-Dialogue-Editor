using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RPG.Nodes.Editor
{
    public abstract class SubWindow
    {
        protected string _name = null;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        protected Rect _rect = Rect.zero;
        public Rect Rect
        {
            get { return _rect; }
            set { _rect = value; }
        }

        public virtual void OnGUI()
        {
            _rect = GUILayout.Window(1, Rect, OnGUI, Name);
        }
        public virtual void OnGUI(int unusedWindowID)
        {
            GUI.DragWindow();
        }
    }
}
