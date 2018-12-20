using System;
using System.Collections.Generic;
using RPG.Nodes;
using RPG.Nodes.Base;
using UnityEngine;

namespace RPG.Dialogue
{
    [Serializable]
    public sealed class ConditionNode : Node, IInput, IMultipleOutput
    {
        [SerializeField]
        private InputPort _inputPort = null;

        private OutputPort _evaluatedOutputPort = null;

        [SerializeField]
        private OutputPort _ifOutputPort = null;
        [SerializeField]
        private OutputPort _elseOutputPort = null;

        [SerializeField]
        private List<Value> _values = new List<Value>();
        [SerializeField]
        private List<Condition> _conditions = new List<Condition>();
        [SerializeField]
        private bool _isAnd = true;

        private OutputPort EvaluatedOutputPort
        {
            get
            {
                if (_evaluatedOutputPort == null) Evaluate();
                return _evaluatedOutputPort;
            }
        }

        public bool IsAnd
        {
            get { return _isAnd; }
            set { _isAnd = value; }
        }
        #region List Wrapping Interface Conditions

        #region Standard Index Stuff
        public int ConditionCount
        {
            get { return _conditions.Count; }
        }

        public Condition GetCondition(int index)
        {
            if (index <= -1 || index >= ConditionCount) return null;
            return _conditions[index];
        }

        public void RemoveCondition(int index)
        {
            if (index <= -1 || index >= ConditionCount) return;
            _conditions.RemoveAt(index);
        }
        #endregion

        public void AddCondition(Condition condition)
        {
            if (condition != null)
                _conditions.Add(condition);
        }

        public void RemoveCondition(Condition condition)
        {
            if (condition != null)
                _conditions.Remove(condition);
        }
        #endregion

        #region List Wrapping Interface Values
        #region Standard Index Stuff
        public int ValueCount
        {
            get { return _values.Count; }
        }

        public Value GetValue(int index)
        {
            if (index <= -1 || index >= ConditionCount) return null;
            return _values[index];
        }

        public void RemoveValue(int index)
        {
            if (index <= -1 || index >= ConditionCount) return;
            _values.RemoveAt(index);
        }
        #endregion

        public void AddValue(Value value)
        {
            if (value != null) _values.Add(value);
        }

        public void RemoveValue(Value value)
        {
            if (value != null) _values.Remove(value);
        }
        #endregion

        public Condition CreateDefaultCondition()
        {
            Condition condition = new Condition
            {
                ComparisonType = ComparisonType.IsEqual,
                UsingBuiltInValue = false
            };

            AddCondition(condition);

            return condition;
        }

        public Condition _CreateDefaultCondition()
        {
            Condition condition = new Condition
            {
                Variable = new Variable() { BoolValue = true },
                LocalValue = new Value() { BoolValue = true },
                ComparisonType = ComparisonType.IsEqual,
                UsingBuiltInValue = false
            };

            AddCondition(condition);

            return condition;
        }

        #region IO
        public InputPort InputPort
        {
            get { return _inputPort ?? (_inputPort = new InputPort() { Node = this }); }
            set
            {
                _inputPort = value;
                _inputPort.Node = this;
            }
        }

        public OutputPort IfOutputPort
        {
            get { return _ifOutputPort ?? (_ifOutputPort = new OutputPort() { Node = this }); }
            set { _ifOutputPort = value; }
        }

        public OutputPort ElseOutputPort
        {
            get { return _elseOutputPort ?? (_elseOutputPort = new OutputPort() { Node = this }); }
            set { _elseOutputPort = value; }
        }
        #endregion

        #region Node Stuff

        //public void Evaluate()
        //{
        //    bool result = false;

        //    if (_isAnd)
        //    {
        //        result = true;

        //        foreach (Condition condition in _conditions)
        //            result &= condition.Evaluate();
        //    }
        //    else
        //    {
        //        result = false;

        //        foreach (Condition condition in _conditions)
        //            result |= condition.Evaluate();
        //    }

        //    _evaluatedOutputPort = result ? IfOutputPort : ElseOutputPort;
        //}

        public void Evaluate()
        {
            bool result;

            if (_isAnd)
            {
                result = true;

                foreach (Condition condition in _conditions)
                {
                    if (condition.Evaluate()) continue;
                    result = false;
                    break;
                }
            }
            else
            {
                result = false;

                foreach (Condition condition in _conditions)
                {
                    if (!condition.Evaluate()) continue;
                    result = true;
                    break;
                }
            }

            _evaluatedOutputPort = result ? IfOutputPort : ElseOutputPort;
        }


        public List<OutputPort> GetOutputs()
        {
            return new List<OutputPort>() { IfOutputPort, ElseOutputPort };
        }

        public new Node NextNode()
        {
            _evaluatedOutputPort = null;
            return EvaluatedOutputPort != null ? EvaluatedOutputPort.Connection.End.Node : null;
        }

        public void AssignNodesToOutputPorts(Node node)
        {
            IfOutputPort.Node = node;
            ElseOutputPort.Node = node;
        }

        public void OffsetMultiplePorts(Vector2 offset)
        {
            //IfOutputPort.Position += offset;
            //ElseOutputPort.Position += offset;
        }

        #endregion

        public void ClearOutputs()
        {
            //Destroy(IfOutputPort);
            //Destroy(ElseOutputPort);
            IfOutputPort = null;
            ElseOutputPort = null;
            _evaluatedOutputPort = null;
        }
    }
}
