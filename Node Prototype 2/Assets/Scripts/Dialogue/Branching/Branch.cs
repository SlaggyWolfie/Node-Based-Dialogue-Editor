using System;
using RPG.Base;
using RPG.Dialogue;
using RPG.Nodes.Base;
using UnityEngine;

namespace RPG.Nodes
{
    [Serializable]
    public sealed class Branch : BaseObject
    {
        [SerializeField]
        private OutputPort _outputPort = null;
        public OutputPort OutputPort
        {
            get { return _outputPort; }
            set { _outputPort = value; }
        }

        [SerializeField]
        private BranchCondition _branchCondition = null;
        public BranchCondition BranchCondition
        {
            get { return _branchCondition; }
            set { _branchCondition = value; }
        }

        private DialogueNode _dialogueNode = null;
        public DialogueNode DialogueNode
        {
            get
            {
                if (_dialogueNode != null || _outputPort == null || _outputPort.Connection == null ||
                    _outputPort.Connection.End == null || _outputPort.Connection.End.Node == null) return _dialogueNode;

                Node node = _outputPort.Connection.End.Node;
                _dialogueNode = node as DialogueNode;

                return _dialogueNode;
            }
        }

        public bool IsAvailable
        {
            get
            {
                if (_outputPort == null) return false;
                if (BranchCondition != null) return BranchCondition.Evaluate();
                return true;
            }
        }

        public Branch(Node node)
        {
            _outputPort = new OutputPort { Node = node };
        }
    }
}
