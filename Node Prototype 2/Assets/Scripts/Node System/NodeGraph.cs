using System;
using System.Collections.Generic;
using System.Linq;
using RPG.Base;
using RPG.Nodes.Base;
using UnityEditor;
using UnityEngine;

namespace RPG.Nodes
{
    [Serializable]
    public abstract class NodeGraph : BaseScriptableObject
    {
        [SerializeField]
        private Flow _flow = null;
        public Flow Flow
        {
            get { return _flow; }
            set { _flow = value; }
        }

        //[SerializeField]
        //private VariableInventory _localVariableInventory = null;
        //public VariableInventory LocalVariableInventory { get { return _localVariableInventory ?? (_localVariableInventory = new VariableInventory()); } }


        private VariableInventory _variableInventory = null;
        public void _SetLocalVariableInventory(VariableInventory variableInventory)
        {
            _variableInventory = variableInventory;
        }
        public VariableInventory LocalVariableInventory { get { return _variableInventory; } }

        [SerializeField]
        private List<Node> _nodes = new List<Node>();

        [SerializeField]
        private List<Connection> _connections = new List<Connection>();

        public T AddNode<T>() where T : Node
        {
            return (T)AddNode(typeof(T));
        }
        public virtual Node AddNode(Type type)
        {
            Node node = (Node)CreateInstance(type);
            AddNode(node);
            return node;
        }
        public virtual void AddNode(Node node)
        {
            _nodes.Add(node);
            node.Graph = this;
        }

        public T CopyNode<T>(T original) where T : Node
        {
            return (T)CopyNode((Node)original);
        }
        public virtual Node CopyNode(Node original)
        {
            Node node = ScriptableObject.Instantiate(original);
            node.ClearConnections();
            _nodes.Add(node);
            node.Graph = this;
            return node;
        }

        public void RemoveNode(Node node)
        {
            node.ClearConnections();
            _nodes.Remove(node);
            if (Application.isPlaying) Destroy(node);
        }
        public void RemoveNode(int index)
        {
            RemoveNode(GetNode(index));
        }
        public void ClearNodes()
        {
            if (Application.isPlaying)
                foreach (var node in _nodes)
                    Destroy(node);
            _nodes.Clear();
        }
        public int NodeCount { get { return _nodes.Count; } }
        public Node GetNode(int index)
        {
            if (index < 0 || index >= NodeCount) return null;
            return _nodes[index];
        }

        public void SendNodeToFront(Node node)
        {
            if (node == null) return;
            _nodes.Remove(node);
            _nodes.Add(node);
        }

        //public virtual Connection AddConnection()
        //{
        //    Connection connection = CreateInstance<Connection>();
        //    AddConnection(connection);
        //    return connection;
        //}
        public virtual void AddConnection(Connection connection)
        {
            _connections.Add(connection);
            connection.Graph = this;
        }

        public virtual Connection CopyConnection(Connection original)
        {
            Connection connection = ScriptableObject.Instantiate(original);
            connection.ClearConnections();
            AddConnection(connection);
            return connection;
        }

        public void RemoveConnection(Connection connection)
        {
            connection.ClearConnections();
            _connections.Remove(connection);
            if (Application.isPlaying) Destroy(connection);
        }
        public void RemoveConnection(int index)
        {
            RemoveConnection(GetConnection(index));
        }
        public void ClearConnections()
        {
            if (Application.isPlaying)
                foreach (Connection connection in _connections)
                    Destroy(connection);
            _connections.Clear();
        }
        public int ConnectionCount { get { return _connections.Count; } }
        public Connection GetConnection(int index)
        {
            if (index < 0 || index >= NodeCount) return null;
            return _connections[index];
        }

        public void SendConnectionToFront(Connection connection)
        {
            if (connection == null) return;
            _connections.Remove(connection);
            _connections.Add(connection);
        }

        public NodeGraph Copy()
        {
            NodeGraph graph = Instantiate(this);
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i] == null) continue;
                Node node = Instantiate(_nodes[i]);
                node.Graph = graph;
                graph._nodes[i] = node;
            }

            foreach (Node node in graph._nodes)
            {
                if (node == null) continue;
                throw new NotImplementedException();
                foreach (Port port in node.GetAllPorts())
                {
                    InputPort input = port as InputPort;
                    if (input != null)
                    {
                        //input.Redirect(graph._nodes); 
                    }

                }
            }

            return graph;
        }

        private void OnDestroy()
        {
            ClearConnections();
            ClearNodes();
        }
    }
}
