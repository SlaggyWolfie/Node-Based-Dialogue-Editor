using System;
using System.Collections.Generic;
using RPG.Nodes;
using UnityEngine;

namespace RPG.Dialogue
{
    [Serializable]
    public sealed class ChoiceNode : Node, IInput, IMultipleOutput
    {
        [SerializeField]
        private InputPort _inputPort = null;
        public InputPort InputPort
        {
            get { return _inputPort ?? (_inputPort = new InputPort() { Node = this }); }
            set
            {
                _inputPort = value;
                _inputPort.Node = this;
            }
        }

        private Branch _choice = null;
        [SerializeField]
        private List<Branch> _branches = new List<Branch>();

        #region List Wrap
        public int BranchCount
        {
            get { return _branches.Count; }
        }

        public Branch GetBranch(int index)
        {
            if (index < 0 || index >= BranchCount) return null;
            return _branches[index];
        }

        public void RemoveBranch(int index)
        {
            if (index < 0 || index >= BranchCount) return;
            _branches.RemoveAt(index);
        }
        public void RemoveBranch(Branch branch)
        {
            if (branch != null)
                _branches.Remove(branch);
        }

        public void AddBranch(Branch branch)
        {
            _branches.Add(branch);
        }

        public Branch CreateBranch()
        {
            Branch branch = new Branch(this);
            AddBranch(branch);
            return branch;
        }

        public List<Branch> GetAllBranches()
        {
            return new List<Branch>(_branches);
        }

        public List<Branch> GetAvailableBranches()
        {
            return _branches.FindAll(branch => branch.IsAvailable);
        }
        #endregion

        public void PickBranch(Branch branch)
        {
            _choice = branch;
        }

        public void PickBranch(int index)
        {
            PickBranch(GetBranch(index));
        }

        public void ClearOutputs()
        {
            foreach (Branch branch in _branches) branch.OutputPort.ClearConnections();
        }

        public List<OutputPort> GetOutputs()
        {
            List<OutputPort> outputs = new List<OutputPort>();
            _branches.ForEach(branch => outputs.Add(branch.OutputPort));
            return outputs;
        }

        public new Node NextNode()
        {
            return _choice != null ? _choice.DialogueNode : null;
        }

        public void AssignNodesToOutputPorts(Node node)
        {
            _branches.ForEach(branch => branch.OutputPort.Node = this);
        }

        public void OffsetMultiplePorts(Vector2 offset)
        {
            //foreach (Branch branch in _branches)
            //    branch.OutputPort.Position += offset;
        }
    }
}
