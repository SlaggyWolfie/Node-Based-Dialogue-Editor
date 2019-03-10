using System;
using UnityEngine;
using WolfEditor.Nodes;

namespace WolfEditor.Dialogue
{
    [Serializable]
    public sealed class EndNode : Node, IInput
    {
        [SerializeField]
        private InputPort _inputPort = null;
        public InputPort InputPort
        {
            get { return this.DefaultGetInputPort(ref _inputPort); }
            set { this.ReplaceInputPort(ref _inputPort, value);}
        }
    }
}
