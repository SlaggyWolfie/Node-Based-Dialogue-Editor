using UnityEngine;

namespace RPG.Editor
{
    public abstract class WindowEditor
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

        public abstract void OnGUI();
    }
}
