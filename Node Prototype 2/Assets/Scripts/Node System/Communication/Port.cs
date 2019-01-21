using System;
using RPG.Base;
using UnityEngine;

namespace RPG.Nodes
{
    [Serializable]
    public abstract class Port : BaseObject
    {
        [SerializeField]
        private Node _node = null;
        public Node Node
        {
            get { return _node; }
            set { _node = value; }
        }

        public abstract void ClearConnections();
        public abstract void Disconnect();
        //public abstract bool CanConnect(Port port);
        //public abstract void Connect(Port port);
        //public abstract void Disconnect(Port port);
        public abstract bool IsConnected { get; }
        public abstract bool IsConnectedTo(Port port);

        public virtual void OnDestroy() { Disconnect(); }
    }
}
