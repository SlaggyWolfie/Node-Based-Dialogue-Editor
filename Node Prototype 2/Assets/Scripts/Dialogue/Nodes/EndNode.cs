using System;
using RPG.Nodes;
using UnityEngine;

namespace RPG.Dialogue
{
    [Serializable]
    public sealed class EndNode : Node, IInput
    {
        [SerializeField]
        private InputPort _inputPort = null;
        public InputPort InputPort
        {
            get
            {
                if (_inputPort != null) return _inputPort;
                //Debug.Log("Why?");
                _inputPort = new InputPort() { Node = this };
                return _inputPort;
            }
            set
            {
                _inputPort = value;
                _inputPort.Node = this;
            }
        }
    }
}
