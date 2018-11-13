using System.Collections;
using System.Collections.Generic;
using RPG.Nodes.Base;
using UnityEngine;

namespace RPG.Nodes
{
    public class Connection : ScriptableObjectWithID
    {
        public List<ConnectionModifier> modifiers = new List<ConnectionModifier>();

        private OutputPort _start = null;
        public OutputPort Start
        {
            get { return _start; }
            set { _start = value; }
        }

        private InputPort _end = null;
        public InputPort End
        {
            get { return _end; }
            set { _end = value; }
        }

        public void Traverse()
        {
            if (modifiers.Count > 0) modifiers.ForEach(am => am.Execute());
        }

        public void RemoveSelf()
        {
            throw new System.NotImplementedException();
        }
    }
}
