using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Nodes
{
    [Serializable]
    public sealed class InputPort : Port
    {
        [SerializeField]
        private List<Connection> _connections = new List<Connection>();

        public int ConnectionsCount { get { return _connections.Count; } }
        public List<Connection> GetConnections() { return new List<Connection>(_connections); }
        public Connection GetConnection(int index) { return _connections[index]; }
        public void RemoveConnection(int index) { RemoveConnection(_connections[index]); }
        public void RemoveConnection(Connection connection){_connections.Remove(connection);}
        public void AddConnection(Connection connection)
        {
            connection.End = this;
            _connections.Add(connection);
        }

        public override void ClearConnections() { _connections.Clear();}

        //public bool CanConnect(OutputPort output)
        //{
        //    return output != null &&
        //           output.Node != null &&
        //           output.Node != Node;
        //}

        //public void Connect(OutputPort output)
        //{
        //    if (!CanConnect(output)) return;
        //    Connection connection = output.Connection;
        //    Connection.FinalizeConnection(connection, this, output);
        //    //Connection.Connect(connection, this, output);
        //    //AddConnection(output.Connection);
        //}

        //public override bool CanConnect(Port port)
        //{
        //    return !(port is InputPort) && CanConnect(port as OutputPort);
        //}

        //public override void Connect(Port port)
        //{
        //    if (CanConnect(port)) ((OutputPort)port).Connect(this);
        //}

        public override bool IsConnected { get { return ConnectionsCount != 0; } }
        public override bool IsConnectedTo(Port port) { return IsConnectedTo(port as OutputPort); }
        public bool IsConnectedTo(OutputPort output)
        {
            if (output == null) return false;

            bool result = false;
            foreach (Connection connection in _connections)
            {
                if (connection.Start != output) continue;
                result = true;
                break;
            }

            return result;
        }

        public override void Disconnect()
        {
            _connections.ForEach(connection => connection.DisconnectEnd());
            ClearConnections();
        }

        public void RemoveNullConnections()
        {
            _connections.RemoveAll(connection => connection == null);
        }
    }
}
