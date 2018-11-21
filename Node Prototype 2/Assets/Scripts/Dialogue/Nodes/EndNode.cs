namespace RPG.Dialogue
{
    public sealed class EndNode : Node, IInput
    {
        private InputPort _inputPort = null;

        public InputPort InputPort
        {
            get
            {
                if (_inputPort == null)
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
