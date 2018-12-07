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
            get { return _inputPort ?? (_inputPort = new InputPort() { Node = this }); }
            set
            {
                _inputPort = value;
                _inputPort.Node = this;
            }
        }
    }
}
