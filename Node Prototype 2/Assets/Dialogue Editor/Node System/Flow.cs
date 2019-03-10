using System;
using System.Collections.Generic;
using UnityEngine;

namespace WolfEditor.Nodes
{
    [Serializable]
    public class Flow
    {
        [SerializeField]
        private Node _currentNode = null;
        [SerializeField]
        private List<Node> _traversedNodes = new List<Node>();

        public Node CurrentNode { get { return _currentNode; } }

        public void SetStartNode(Node startNode)
        {
            _currentNode = startNode;
            EnterNode(_currentNode);
        }

        public Node NextNode()
        {
            if (_currentNode == null) return null;
            ExitNode(_currentNode);
            _traversedNodes.Add(_currentNode);

            _currentNode = _currentNode.NextNode();
            EnterNode(_currentNode);
            return _currentNode;
        }

        private static void EnterNode(Node node)
        {
            if (node != null && node.onEnter != null) node.onEnter();
        }

        private static void ExitNode(Node node)
        {
            if (node != null && node.onExit != null) node.onExit();
        }

        public void Clear()
        {
            _traversedNodes.Clear();
        }
    }
}
