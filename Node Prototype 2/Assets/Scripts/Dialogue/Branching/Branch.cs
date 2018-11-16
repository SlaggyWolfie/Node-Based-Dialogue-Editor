using RPG.Nodes.Base;

namespace RPG.Nodes
{
    public sealed class Branch : ScriptableObjectWithID
    {
        private OutputPort _outputPort = null;
        private BranchCondition _branchCondition = null;
        private DialogueNode _dialogueNode = null;

        public OutputPort OutputPort
        {
            get { return _outputPort; }
            set { _outputPort = value; }
        }

        public BranchCondition BranchCondition
        {
            get { return _branchCondition; }
            set { _branchCondition = value; }
        }

        public bool IsAvailable
        {
            get
            {
                if (_outputPort == null) return false;
                if (_branchCondition != null) return _branchCondition.Evaluate();
                return true;
            }
        }

        public DialogueNode DialogueNode
        {
            get
            {
                if (_dialogueNode == null &&
                    _outputPort != null &&
                    _outputPort.Connection != null &&
                    _outputPort.Connection.End != null &&
                    _outputPort.Connection.End.Node != null)
                {
                    Node node = _outputPort.Connection.End.Node;
                    _dialogueNode = node as DialogueNode;
                }

                return _dialogueNode;
            }
        }

        public Branch(Node node)
        {
            _outputPort = new OutputPort { Node = node };
        }
    }
}
