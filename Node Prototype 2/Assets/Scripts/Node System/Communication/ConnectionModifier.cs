using System;
using RPG.Base;
using RPG.Nodes.Base;
using UnityEngine;

namespace RPG.Nodes.Base
{
    [Serializable]
    public abstract class ConnectionModifier : BaseObject
    {
        [SerializeField]
        [HideInInspector]
        private Connection _connection = null;

        public Connection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public abstract void Execute();
        public ConnectionModifier ShallowCopy() { return (ConnectionModifier)MemberwiseClone(); }
        public abstract ConnectionModifier DeepCopy();
    }
}
