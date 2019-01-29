using System;
using System.Collections.Generic;
using System.Linq;
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
            get { return this.DefaultGetInputPort(ref _inputPort); }
            set { this.ReplaceInputPort(ref _inputPort, value); }
        }

        private Branch _choice = null;

        [SerializeField] private List<Branch> _branches = new List<Branch>();
        public int BranchCount { get { return _branches.Count; } }
        public Branch GetBranch(int index) { return _branches[index]; }
        public void RemoveBranch(int index) { _branches.RemoveAt(index); }
        public void RemoveBranch(Branch branch) { if (branch != null) _branches.Remove(branch); }
        public void AddBranch(Branch branch) { _branches.Add(branch); }

        public Branch CreateBranch()
        {
            Branch branch = new Branch(this);
            AddBranch(branch);
            return branch;
        }

        public List<Branch> GetAllBranches() { return new List<Branch>(_branches); }
        public List<Branch> GetAvailableBranches() { return _branches.FindAll(branch => branch.IsAvailable); }

        public void PickBranch(Branch branch) { _choice = branch; }
        public void PickBranch(int index) { PickBranch(GetBranch(index)); }

        public IEnumerable<OutputPort> GetOutputs()
        {
            List<OutputPort> outputs = new List<OutputPort>();
            foreach (Branch branch in _branches) outputs.Add(branch.OutputPort);
            return outputs;
        }

        public void ReplacePort(OutputPort oldPort, OutputPort newPort)
        {
            foreach (Branch branch in _branches)
            {
                if (oldPort != branch.OutputPort) continue;
                branch.OutputPort = newPort;
                break;
            }
        }

        public new Node NextNode() { return _choice != null ? _choice.DialogueNode : null; }
        public void ResetOutputPorts() { _branches.ForEach(branch => branch.OutputPort = new OutputPort()); }
    }
    
    //[Serializable]
    //public sealed class ChoiceNode : Node, IInput, IMultipleOutput
    //{
    //    [SerializeField]
    //    private InputPort _inputPort = null;
    //    public InputPort InputPort
    //    {
    //        get { return this.DefaultGetInputPort(ref _inputPort); }
    //        set { this.ReplaceInputPort(ref _inputPort, value); }
    //    }

    //    private Branch _choice = null;
    //    [SerializeField]
    //    private List<Branch> _branches = new List<Branch>();

    //    #region List Wrap
    //    public int BranchCount { get { return _branches.Count; } }
    //    public Branch GetBranch(int index) { return _branches[index]; }
    //    public void RemoveBranch(int index) { _branches.RemoveAt(index); }
    //    public void RemoveBranch(Branch branch) { if (branch != null) _branches.Remove(branch); }
    //    public void AddBranch(Branch branch) { _branches.Add(branch); }
    //    public Branch CreateBranch()
    //    {
    //        Branch branch = new Branch(this);
    //        AddBranch(branch);
    //        return branch;
    //    }

    //    public List<Branch> GetAllBranches() { return new List<Branch>(_branches); }
    //    public List<Branch> GetAvailableBranches() { return _branches.FindAll(branch => branch.IsAvailable); }
    //    #endregion

    //    public void PickBranch(Branch branch) { _choice = branch; }
    //    public void PickBranch(int index) { PickBranch(GetBranch(index)); }

    //    //public void ClearOutputs()
    //    //{
    //    //    foreach (Branch branch in _branches) branch.OutputPort.Disconnect();
    //    //}

    //    public IEnumerable<OutputPort> GetOutputs()
    //    {
    //        List<OutputPort> outputs = new List<OutputPort>();
    //        _branches.ForEach(branch => outputs.Add(branch.OutputPort));
    //        return outputs;
    //    }

    //    public new Node NextNode()
    //    {
    //        return _choice != null ? _choice.DialogueNode : null;
    //    }

    //    public void KillOutputPorts() { _branches.ForEach(branch => branch.OutputPort = null); }
    //}
}
