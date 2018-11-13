using System;
using System.Collections.Generic;

namespace RPG.Nodes
{
    public class InputPort : Port
    {
        private List<Connection> _connections = new List<Connection>();
        public int ConnectionsCount
        {
            get { return _connections.Count; }
        }
        public Connection GetConnection(int index)
        {
            //if (index < 0 || index >= ConnectionsCount) return null;
            return _connections[index];
        }
        public void RemoveConnection(int index)
        {
            if (index < 0 || index >= ConnectionsCount) return;
            RemoveConnection(_connections[index]);
        }
        public void RemoveConnection(Connection connection)
        {
            if (UnityEngine.Application.isPlaying) Destroy(connection);
            _connections.Remove(connection);
        }
        public void AddConnection(Connection connection)
        {
            connection.End = this;
            _connections.Add(connection);
        }

        public Connection CreateConnection()
        {
            Connection inputConnection = new Connection();
            AddConnection(inputConnection);
            return inputConnection;
        }

        public bool CanConnect(OutputPort output)
        {
            return output != null &&
                   output.Node != null &&
                   output.Node != Node;
        }

        public void Connect(OutputPort output)
        {
            if (CanConnect(output)) output.Connection.End = this;
        }

        public override bool CanConnect(Port port)
        {
            return !(port is InputPort) && CanConnect(port as OutputPort);
        }

        public override void Connect(Port port)
        {
            if (CanConnect(port)) ((OutputPort)port).Connection.End = this;
        }

        public override void ClearConnections()
        {
            if (UnityEngine.Application.isPlaying)
                for (int i = 0; i >= 0; i--)
                    Destroy(_connections[i]);

            _connections.Clear();
        }

        //public override void Reconnect(List<Node> oldNodes, List<Node> newNodes)
        //{
        //    foreach (Connection connection in _connections)
        //    {
        //        int index = oldNodes.IndexOf(connection.Start.Node);
        //        if (index >= 0) Connect(newNodes[index]);
        //    }
        //}

        //public void Redirect(List<Node> newNodes)
        //{
        //    foreach (Node newNode in newNodes)
        //    {
        //        ISingleOutput sOutput = newNode as ISingleOutput;
        //        if (sOutput != null) 
        //    }
        //}
    }
}
