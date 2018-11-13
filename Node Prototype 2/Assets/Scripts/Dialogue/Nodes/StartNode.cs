namespace RPG.Nodes
{
    public sealed class StartNode : Node, ISingleOutput
    {
        private OutputPort _outputPort = null;
        
        public OutputPort OutputPort
        {
            get
            {
                if (_outputPort == null)
                    _outputPort = new OutputPort() { Node = this };
                return _outputPort;
            }
            set
            {
                _outputPort = value;
                _outputPort.Node = this;
            }
        }
    }
}
