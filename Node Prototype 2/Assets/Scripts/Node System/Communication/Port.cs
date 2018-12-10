using System;
using System.Collections.Generic;
using RPG.Base;
using RPG.Nodes.Base;
using UnityEngine;

namespace RPG.Nodes
{
    [Serializable]
    public abstract class Port : BaseObject
    {
        //[SerializeField]
        ////[HideInInspector]
        //private Vector2 _position = Vector2.zero;
        //public Vector2 Position
        //{
        //    get { return _position; }
        //    set { _position = value; }
        //}

        [SerializeField]
        //[HideInInspector]
        private Node _node = null;
        public Node Node
        {
            get { return _node; }
            set { _node = value; }
        }

        public abstract void ClearConnections();
        public abstract bool CanConnect(Port port);
        public abstract void Connect(Port port);
        //public abstract void Disconnect(Port port);
        public abstract bool IsConnected { get; }
        public abstract bool IsConnectedTo(Port port);

        public virtual void OnDestroy()
        {
            ClearConnections();
        }
        //public abstract void Reconnect(List<Node> oldNodes, List<Node> newNodes);
    }
}
