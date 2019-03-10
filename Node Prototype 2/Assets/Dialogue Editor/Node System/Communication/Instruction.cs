using System;
using WolfEditor.Nodes.Base;
using WolfEditor.Other;
using UnityEngine;
using WolfEditor.Base;

namespace WolfEditor.Nodes.Base
{
    [Serializable]
    //public abstract class ConnectionModifier : BaseObject//, ICopyable<ConnectionModifier>
    public abstract class Instruction : BaseScriptableObject//, ICopyable<ConnectionModifier>//, IComplexCopyable<ConnectionModifier>
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
    }
}
