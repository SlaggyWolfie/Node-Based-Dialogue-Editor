using System;
using System.Collections.Generic;
using RPG.Nodes.Base;
using UnityEngine;

namespace RPG.Nodes
{
    public class NodeGraph : ScriptableObjectWithID
    {
        private Flow _flow = null;
        public Flow Flow
        {
            get { return _flow; }
            set { _flow = value; }
        }

        private VariableRepository _localVariableRepository = null;
        public VariableRepository LocalVariableRepository
        {
            get
            {
                return _localVariableRepository ?? (_localVariableRepository = new VariableRepository());
            }
        }

        private List<Node> _nodes = new List<Node>();

        public T AddNode<T>() where T : Node
        {
            return (T)AddNode(typeof(T));
        }
        public virtual Node AddNode(Type type)
        {
            Node node = (Node)CreateInstance(type);
            _nodes.Add(node);
            node.Graph = this;
            return node;
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

        public void Clear()
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
            Clear();
        }
    }
}
