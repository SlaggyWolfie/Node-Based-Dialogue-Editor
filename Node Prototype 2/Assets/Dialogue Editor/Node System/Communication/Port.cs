using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Base;
using UnityEngine;

namespace RPG.Nodes
{
    [Serializable]
    public abstract class Port : BaseObject//, IEqualityComparer, IEqualityComparer<Port>
    {
        [SerializeField]
        protected Node node = null;
        public Node Node
        {
            get { return node; }
            set { node = value; }
        }

        public abstract void ClearConnections();
        public abstract void Disconnect();
        //public abstract bool CanConnect(Port port);
        //public abstract void Connect(Port port);
        //public abstract void Disconnect(Port port);
        public abstract bool IsConnected { get; }
        public abstract bool IsConnectedTo(Port port);

        public virtual void OnDestroy() { Disconnect(); }

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        //public int GetHashCode(object obj) { return GetHashCode((Port)obj); }
        //public new bool Equals(object x, object y) { return Equals((Port)x, (Port)y); }
        //public int GetHashCode(Port obj) { return ID; }
        //public bool Equals(Port x, Port y)
        //{
        //    if (x == null || y == null) return false;
        //    return x.GetHashCode() == y.GetHashCode();
        //}
    }
}
