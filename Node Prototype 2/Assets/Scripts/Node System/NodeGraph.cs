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
        [SerializeField] protected Counter nodeCounter = new Counter();
        [SerializeField] protected Counter connectionCounter = new Counter();
        [SerializeField] public Counter connectionModifierCounter = new Counter();
        [SerializeField] public Counter portCounter = new Counter();

        [SerializeField]
        private Flow _flow = null;
        public Flow Flow
        {
            get { return _flow; }
            set { _flow = value; }
        }

        private VariableInventory _variableInventory = null;
        public Action _missingVariableInventory;
        public void _SetLocalVariableInventory(VariableInventory variableInventory)
        {
            _variableInventory = variableInventory;
        }
        public VariableInventory GetVariableInventory()
        {
            if (!_variableInventory && _missingVariableInventory != null) _missingVariableInventory.Invoke();
            //if (_variableInventory == null && _missingVariableInventory != null) _missingVariableInventory.Invoke();
            return _variableInventory;
        }

        [SerializeField] private List<Node> _nodes = new List<Node>();
        [SerializeField] private List<Connection> _connections = new List<Connection>();

        public T CreateAndAddNode<T>() where T : Node
        {
            return (T)CreateAndAddNode(typeof(T));
        }
        public virtual Node CreateAndAddNode(Type type)
        {
            Node node = (Node)CreateInstance(type);
            InitNode(node);
            return node;
        }
        public virtual void AddNode(Node node)
        {
            _nodes.Add(node);
            node.Graph = this;
        }

        public void RemoveNode(Node node)
        {
            node.Disconnect();
            _nodes.Remove(node);
        }
        public void RemoveNode(int index)
        {
            RemoveNode(GetNode(index));
        }
        public int NodeCount { get { return _nodes.Count; } }
        public Node GetNode(int index)
        {
            return _nodes[index];
        }

        public void SendNodeBackward(Node node)
        {
            int index = _nodes.IndexOf(node);
            if (index == 0) return;
            _nodes.RemoveAt(index);
            _nodes.Insert(index - 1, node);
        }

        public void SendNodeForward(Node node)
        {
            int index = _nodes.IndexOf(node);
            if (index == NodeCount - 1) return;
            _nodes.RemoveAt(index);
            _nodes.Insert(index + 1, node);
        }

        public void SendNodeToBack(Node node)
        {
            _nodes.Remove(node);
            _nodes.Insert(0, node);
        }

        public void SendNodeToFront(Node node)
        {
            _nodes.Remove(node);
            _nodes.Add(node);
        }

        public virtual Connection CreateAndAddConnection()
        {
            Connection connection = CreateInstance<Connection>();
            InitConnection(connection);
            return connection;
        }
        public virtual void AddConnection(Connection connection)
        {
            if (connection == null) return;
            _connections.Add(connection);
            connection.Graph = this;
        }

        public void RemoveConnection(Connection connection)
        {
            if (connection == null) return;
            connection.Disconnect();
            _connections.Remove(connection);
        }
        public void RemoveConnection(int index) { RemoveConnection(GetConnection(index)); }
        public int ConnectionCount { get { return _connections.Count; } }
        public Connection GetConnection(int index) { return _connections[index]; }

        public void SendConnectionBackward(Connection connection)
        {
            int index = _connections.IndexOf(connection);
            if (index == 0) return;
            _connections.RemoveAt(index);
            _connections.Insert(index - 1, connection);
        }

        public void SendConnectionForward(Connection connection)
        {
            int index = _connections.IndexOf(connection);
            if (index == ConnectionCount - 1) return;
            _connections.RemoveAt(index);
            _connections.Insert(index + 1, connection);
        }

        public void SendConnectionToBack(Connection connection)
        {
            _connections.Remove(connection);
            _connections.Insert(0, connection);
        }

        public void SendConnectionToFront(Connection connection)
        {
            _connections.Remove(connection);
            _connections.Add(connection);
        }

        //TODO
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
            foreach (var connection in _connections)
                DestroyHelper.Destroy(connection);
                //DestroyImmediate(connection, true);
            foreach (var node in _nodes)
                DestroyHelper.Destroy(node);
                //DestroyImmediate(node, true);

            _connections.Clear();
            _nodes.Clear();
        }

        public Node FindStartNode() { return _nodes.Find(n => n is StartNode); }

        public void RemoveNullConnections()
        {
            //_connections.RemoveAll(connection => connection == null);
            for (int i = ConnectionCount - 1; i >= 0; i--)
            {
                if (_connections[i] != null) continue;
                Debug.Log(string.Format("Null connection in graph at index {0}", i));
                _connections.RemoveAt(i);
            }
        }

        public void InitNode(Node node)
        {
            node.ID = nodeCounter.Get();
            AddNode(node);
            node.Init();
            //Debug.Log("Node Init" + node.ID);
        }
        public void InitConnection(Connection connection)
        {
            connection.ID = connectionCounter.Get();
            AddConnection(connection);
            connection.name = "Connection " + connection.ID;
            //Debug.Log("Connection Init: " + connection.ID);
        }
    }
}
