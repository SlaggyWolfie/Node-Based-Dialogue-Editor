using System;

namespace RPG.Nodes
{
    public class OutputPort : Port
    {
        public Action onNodeExit;

        private Connection _connection = null;
        public Connection Connection
        {
            get
            {
                if (_connection != null) return _connection;

                _connection = new Connection {Start = this};
                onNodeExit += _connection.Traverse;
                return _connection;
            }
            set
            {
                if (onNodeExit != null) onNodeExit -= _connection.Traverse;
                _connection = value;
                _connection.Start = this;
                onNodeExit += _connection.Traverse;
            }
        }

        public void Exit()
        {
            if (onNodeExit != null) onNodeExit();
        }

        public bool CanConnect(InputPort input)
        {
            return input != null &&
                   input.Node != null &&
                   input.Node != Node;
        }

        public void Connect(InputPort input)
        {
            if (CanConnect(input)) input.Connect(this);
        }

        public override bool CanConnect(Port port)
        {
            return !(port is OutputPort) && CanConnect(port as InputPort);
        }

        public override void Connect(Port port)
        {
            if (CanConnect(port)) ((InputPort)port).Connect(this);
        }
    }
}