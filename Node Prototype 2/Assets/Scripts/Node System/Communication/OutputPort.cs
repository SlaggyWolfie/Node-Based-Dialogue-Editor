using System;
using UnityEngine;

namespace RPG.Nodes
{
    [Serializable]
    public sealed class OutputPort : Port
    {
        [SerializeField, HideInInspector]
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
                _connection = value;
                _traversalSetup = false;

                if (value == null) return;
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

        //public override void Disconnect(Port port)
        //{
        //    if (IsConnected) Disconnect(port as InputPort);
        //}

        //public void Disconnect(InputPort input)
        //{
        //    if (!IsConnectedTo(input)) return;
        //    input.RemoveConnection(Connection);
        //    Connection.End = null;
        //}

        public override bool IsConnected { get { return Connection != null; } }
        public override bool IsConnectedTo(Port port) { return IsConnectedTo(port as InputPort); }
        public bool IsConnectedTo(InputPort input) { return IsConnected && input != null && Connection.End == input; }

        public override void ClearConnections()
        {
            if (UnityEngine.Application.isPlaying)
                UnityEngine.Object.Destroy(Connection);
            Connection = null;
        }
    }
}