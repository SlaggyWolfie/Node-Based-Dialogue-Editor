using System;

namespace RPG.Nodes
{
    public sealed class OutputPort : Port
    {
        private Connection _connection = null;
        public Connection Connection
        {
            get
            {
                if (_connection == null) return null;

                _connection.Start = this;
                SetupTraversal();
                return _connection;
            }
            set
            {
                if (_connection == value) return;

                _traversalSetup = false;
                _connection = value;
                _connection.Start = this;
                SetupTraversal();
            }
        }

        private bool _traversalSetup = false;
        private void SetupTraversal()
        {
            if (_traversalSetup) return;
            if (Node == null) return;

            if (Node.onExit != null) Node.onExit -= _connection.Traverse;
            Node.onExit += _connection.Traverse;

            _traversalSetup = true;
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

        public override void ClearConnections()
        {
            if (UnityEngine.Application.isPlaying) Destroy(_connection);
            _connection = null;
        }
    }
}