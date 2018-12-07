using System;
using RPG.Nodes;
using UnityEngine;

namespace RPG.Dialogue
{
    [Serializable]
    public sealed class StartNode : Node, IOutput
    {
        [SerializeField]
        private OutputPort _outputPort = null;

        public OutputPort OutputPort
        {
            get { return _outputPort ?? (_outputPort = new OutputPort { Node = this }); }
            set
            {
                _outputPort = value;
                _outputPort.Node = this;
            }
        }
    }
}
