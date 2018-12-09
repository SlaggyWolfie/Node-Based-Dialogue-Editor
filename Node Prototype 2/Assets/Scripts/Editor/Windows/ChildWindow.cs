using UnityEngine;

namespace RPG.Editor
{
    public abstract class ChildWindow
    {
        protected ParentEditorWindow _parent = null;
        public ParentEditorWindow Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

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
            Rect = GUILayout.Window(1, Rect, OnGUI, Name);
        }
        public virtual void OnGUI(int unusedWindowID)
        {
            GUI.DragWindow();
        }

        protected void Repaint()
        {
            if (_parent != null) _parent.Repaint();
        }
    }
}
