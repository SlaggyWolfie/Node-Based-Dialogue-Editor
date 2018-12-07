using System;
using System.Collections.Generic;
using RPG.Nodes;
using UnityEngine;

namespace RPG.Dialogue
{
    [Serializable]
    public sealed class ChoiceNode : Node, IInput, IMultipleOutput
    {
        private Branch _pickedBranch = null;
        [SerializeField]
        private List<Branch> _branches = new List<Branch>();

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

        #region List Wrapping Interface

        #region Standard Index Stuff
        public int BranchCount
        {
            get { return _branches.Count; }
        }

        public Branch GetBranch(int index)
        {
            return _branches[index];
        }

        public void RemoveBranch(int index)
        {
            _branches.RemoveAt(index);
        }
        #endregion

        public void RemoveBranch(Branch branch)
        {
            _branches.Remove(branch);
        }
        #endregion

        public Branch CreateBranch()
        {
            Branch branch = new Branch(this);

            _branches.Add(branch);

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

        public void PickBranch(Branch branch)
        {
            _pickedBranch = branch;
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
            return _pickedBranch != null ? _pickedBranch.DialogueNode : null;
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
