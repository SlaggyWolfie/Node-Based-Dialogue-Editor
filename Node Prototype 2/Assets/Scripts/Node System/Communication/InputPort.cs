using System;
using System.Collections.Generic;

namespace RPG.Nodes
{
    public class InputPort : Port
    {
        private List<Connection> _connections = new List<Connection>();

        #region Connection Interface

        #region Standard Index Stuff
        public int ConnectionsCount
        {
            get { return _connections.Count; }
        }

        public Connection GetConnection(int index)
        {
            return _connections[index];
        }

        public void RemoveConnection(int index)
        {
            _connections.RemoveAt(index);
        }
        #endregion

        public Connection CreateConnection()
        {
            Connection inputConnection = new Connection();
            AddConnection(inputConnection);

            return inputConnection;
        }

        public void AddConnection(Connection inputConnection)
        {
            inputConnection.End = this;
            //Node.onNodeStart += ;

            _connections.Add(inputConnection);
        }

        public void RemoveConnection(Connection inputConnection)
        {
            //Destroy(inputConnection);
            inputConnection.End = null;

            _connections.Remove(inputConnection);
        }
        #endregion

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
