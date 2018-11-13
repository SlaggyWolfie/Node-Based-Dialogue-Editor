using System.Collections.Generic;
using RPG.Nodes.Base;

namespace RPG.Nodes
{
    public abstract class Port : DataObject
    {
        private Node _node = null;

        public Node Node
        {
            get { return _node; }
            set { _node = value; }
        }

        public void ClearConnections()
        {
            InputPort input = this as InputPort;
            if (input != null) input.ClearConnections();

            OutputPort output = this as OutputPort;
            if (output != null) output.ClearConnections();
        }

        public abstract bool CanConnect(Port port);
        public abstract void Connect(Port port);

        //public abstract void Reconnect(List<Node> oldNodes, List<Node> newNodes);
    }
}
