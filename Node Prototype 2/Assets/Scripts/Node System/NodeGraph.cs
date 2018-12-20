using System;
using System.Collections.Generic;
using System.Linq;
using RPG.Base;
using RPG.Dialogue;
using RPG.Nodes.Base;
using RPG.Other;
using UnityEditor;
using UnityEngine;

namespace RPG.Nodes
{
    [Serializable]
    public abstract class NodeGraph : BaseScriptableObject
    {
        [SerializeField, HideInInspector] protected Counter _nodeCounter = new Counter();
        [SerializeField, HideInInspector] protected Counter _connectionCounter = new Counter();

        [SerializeField]
        private Flow _flow = null;
        public Flow Flow
        {
            get { return _flow; }
            set { _flow = value; }
        }

        private VariableInventory _variableInventory = null;
        public void _SetLocalVariableInventory(VariableInventory variableInventory)
        {
            _variableInventory = variableInventory;
        }
        public VariableInventory LocalVariableInventory { get { return _variableInventory; } }

        [SerializeField] private List<Node> _nodes = new List<Node>();
        [SerializeField] private List<Connection> _connections = new List<Connection>();

        public T CreateAndAddNode<T>() where T : Node
        {
            return (T)CreateAndAddNode(typeof(T));
        }
        public virtual Node CreateAndAddNode(Type type)
        {
            Node node = (Node)CreateInstance(type);
            node.ID = _nodeCounter.Get();
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

        public void SendNodeBackward(Node node)
        {
            if (node == null) return;
            int index = _nodes.IndexOf(node);
            if (index == 0) return;
            _nodes.RemoveAt(index);
            _nodes.Insert(index - 1, node);
        }

        public void SendNodeForward(Node node)
        {
            if (node == null) return;
            int index = _nodes.IndexOf(node);
            if (index == NodeCount - 1) return;
            _nodes.RemoveAt(index);
            _nodes.Insert(index + 1, node);
        }

        public void SendNodeToBack(Node node)
        {
            if (node == null) return;
            _nodes.Remove(node);
            _nodes.Insert(0, node);
        }

        public void SendNodeToFront(Node node)
        {
            if (node == null) return;
            _nodes.Remove(node);
            _nodes.Add(node);
        }

        public virtual Connection CreateAndAddConnection()
        {
            Connection connection = CreateInstance<Connection>();
            connection.ID = _connectionCounter.Get();
            connection.name = "Connection " + connection.ID;
            AddConnection(connection);
            return connection;
        }
        public virtual void AddConnection(Connection connection)
        {
            _connections.Add(connection);
            connection.Graph = this;
        }

        public virtual Connection CopyConnection(Connection original)
        {
            Connection connection = ScriptableObject.Instantiate(original);
            connection.Disconnect();
            AddConnection(connection);
            return connection;
        }

        public void RemoveConnection(Connection connection)
        {
            connection.Disconnect();
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


        public void SendConnectionBackward(Connection connection)
        {
            if (connection == null) return;
            int index = _connections.IndexOf(connection);
            if (index == 0) return;
            _connections.RemoveAt(index);
            _connections.Insert(index - 1, connection);
        }

        public void SendConnectionForward(Connection connection)
        {
            if (connection == null) return;
            int index = _connections.IndexOf(connection);
            if (index == ConnectionCount - 1) return;
            _connections.RemoveAt(index);
            _connections.Insert(index + 1, connection);
        }

        public void SendConnectionToBack(Connection connection)
        {
            if (connection == null) return;
            _connections.Remove(connection);
            _connections.Insert(0, connection);
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

        public Node FindStartNode()
        {
            return _nodes.Find(n => n is StartNode);
        }
    }
}
