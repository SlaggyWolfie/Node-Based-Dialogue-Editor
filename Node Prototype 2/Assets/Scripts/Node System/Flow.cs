using System.Collections.Generic;

namespace RPG.Nodes
{
    public class Flow
    {
        private Node _currentNode = null;
        private List<Node> _traversedNode = new List<Node>();

        public Node CurrentNode { get { return _currentNode; } }

        public void SetStartNode(Node startNode)
        {
            _currentNode = startNode;
            EnterNode(_currentNode);
        }

        public void NextNode()
        {
            if (_currentNode == null) return;
            ExitNode(_currentNode);
            _traversedNode.Add(_currentNode);

            _currentNode = _currentNode.NextNode();
            EnterNode(_currentNode);
        }

        private static void EnterNode(Node node)
        {
            if (node != null && node.onEnter != null) node.onEnter();
        }

        private static void ExitNode(Node node)
        {
            if (node != null && node.onExit != null) node.onExit();
        }
    }
}
