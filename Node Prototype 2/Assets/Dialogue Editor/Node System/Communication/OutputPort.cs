using System;
using UnityEngine;

namespace WolfEditor.Nodes
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
                //SetupTraversal();
                return _connection;
            }
            set
            {
                if (_connection == value) return;
                //KillTraversal();

                _connection = value;

                if (value == null) return;
                _connection.Start = this;
                //SetupTraversal();
            }
        }

        private bool _traversalSetup = false;

        public void OnExit() { if (_connection != null) _connection.Traverse(); }

        //private void SetupTraversal()
        //{
        //    if (_traversalSetup) return;
        //    if (node == null || _connection == null) return;
        //    node.onExit += _connection.Traverse;
        //    //Debug.Log("RIP");
        //    _traversalSetup = true;
        //}

        //private void KillTraversal()
        //{
        //    if (!_traversalSetup) return;
        //    if (node == null || _connection == null) return;
        //    if (node.onExit != null) Node.onExit -= _connection.Traverse;
        //    _traversalSetup = false;
        //}

        //public bool CanConnect(InputPort input)
        //{
        //    return input != null &&
        //           input.Node != null &&
        //           input.Node != Node;
        //}

        //public void Connect(InputPort input)
        //{
        //    if (CanConnect(input)) input.Connect(this);
        //}

        //public override bool CanConnect(Port port)
        //{
        //    return !(port is OutputPort) && CanConnect(port as InputPort);
        //}

        //public override void Connect(Port port)
        //{
        //    if (CanConnect(port)) ((InputPort)port).Connect(this);
        //}

        ////public override void Disconnect(Port port)
        ////{
        ////    if (IsConnected) Disconnect(port as InputPort);
        ////}

        ////public void Disconnect(InputPort input)
        ////{
        ////    if (!IsConnectedTo(input)) return;
        ////    input.RemoveConnection(Connection);
        ////    Connection.End = null;
        ////}

        public override bool IsConnected { get { return Connection != null; } }
        public override bool IsConnectedTo(Port port) { return IsConnectedTo(port as InputPort); }
        public bool IsConnectedTo(InputPort input) { return IsConnected && input != null && Connection.End == input; }

        public override void Disconnect()
        {
            //if (Application.isPlaying) UnityEngine.Object.Destroy(Connection);
            if (Connection == null) return;
            Connection.DisconnectStart();
        }

        public override void ClearConnections() { Connection = null; }
        //public override void OnEnable() { SetupTraversal(); }
        //public override void OnDisable() { KillTraversal(); }
    }
}