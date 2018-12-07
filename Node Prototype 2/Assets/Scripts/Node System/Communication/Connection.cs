using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Nodes.Base;
using UnityEngine;

namespace RPG.Nodes
{
    [System.Serializable]
    public class Connection : BaseScriptableObject
    {
        [SerializeField]
        //[HideInInspector]
        private NodeGraph _graph = null;
        public NodeGraph Graph
        {
            get { return _graph; }
            set { _graph = value; }
        }

        [SerializeField]
        private List<ConnectionModifier> _modifiers = new List<ConnectionModifier>();

        [SerializeField]
        //[HideInInspector]
        private OutputPort _start = null;
        public OutputPort Start
        {
            get { return _start; }
            set
            {
                _start = value;
                //_start.Connection = this;
            }
        }

        [SerializeField]
        //[HideInInspector]
        private InputPort _end = null;
        public InputPort End
        {
            get { return _end; }
            set
            {
                _end = value;
                //_end.ConnectionsCount = this;
            }
        }

        public void Traverse()
        {
            if (_modifiers.Count > 0) _modifiers.ForEach(m => m.Execute());
        }

        public void RemoveSelf()
        {
            throw new System.NotImplementedException();
        }

        public void ClearConnections()
        {
            DisconnectStart();
            DisconnectEnd();
        }

        public void DisconnectPort(Port port)
        {
            if (port == Start) DisconnectStart();
            else if (port == End) DisconnectEnd(); ;
        }

        public void DisconnectStart()
        {
            Start.Connection = null;
            Start = null;
        }
        public void DisconnectEnd()
        {
            End.RemoveConnection(this);
            End = null;
        }
        public void DisconnectInput() { DisconnectEnd(); }
        public void DisconnectOutput() { DisconnectStart(); }

        public T AddModifier<T>() where T : ConnectionModifier
        {
            return (T)AddModifier(typeof(T));
        }
        public virtual ConnectionModifier AddModifier(Type type)
        {
            ConnectionModifier mod = (ConnectionModifier)Activator.CreateInstance(type);
            AddModifier(mod);
            return mod;
        }
        public virtual void AddModifier(ConnectionModifier node)
        {
            _modifiers.Add(node);
            node.Connection = this;
        }

        public T CopyModifier<T>(T original) where T : ConnectionModifier
        {
            return (T)CopyModifier((ConnectionModifier)original);
        }
        public virtual ConnectionModifier CopyModifier(ConnectionModifier original)
        {
            //ConnectionModifier mod = original.ShallowCopy();
            ConnectionModifier mod = original.DeepCopy();
            _modifiers.Add(mod);
            mod.Connection = this;
            return mod;
        }

        public void RemoveModifier(ConnectionModifier mod)
        {
            _modifiers.Remove(mod);
        }
        public void RemoveModifier(int index)
        {
            RemoveModifier(GetModifier(index));
        }
        public void ClearModifiers()
        {
            _modifiers.Clear();
        }
        public int ModifierCount { get { return _modifiers.Count; } }

        public ConnectionModifier GetModifier(int index)
        {
            if (index < 0 || index >= ModifierCount) return null;
            return _modifiers[index];
        }

        public static void Connect(Connection connection, InputPort input, OutputPort output)
        {
            connection.Start = output;
            connection.End = input;
            output.Connection = connection;
            input.AddConnection(connection);
        }
    }
}
