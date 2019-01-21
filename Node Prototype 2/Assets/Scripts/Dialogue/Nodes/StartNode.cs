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
            get { return this.DefaultGetOutputPort(ref _outputPort); }
            set { this.ReplaceOutputPort(ref _outputPort, value); }
        }
    }
}
