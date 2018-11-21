using System.Collections.Generic;
using RPG.Nodes.Base;
using UnityEngine;

namespace RPG.Dialogue
{
    public sealed class ConditionNode : Node, IInput, IMultipleOutput
    {
        private InputPort _inputPort = null;

        private OutputPort _evaluatedOutputPort = null;

        private OutputPort _ifOutputPort = null;
        private OutputPort _elseOutputPort = null;

        private List<Condition> _conditions = new List<Condition>();
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

        #region List Wrapping Interface

        #region Standard Index Stuff
        public int ConditionCount
        {
            get { return _conditions.Count; }
        }

        public Condition GetCondition(int index)
        {
            return _conditions[index];
        }

        public void RemoveCondition(int index)
        {
            _conditions.RemoveAt(index);
        }
        #endregion

        public void AddCondition(Condition condition)
        {
            _conditions.Add(condition);
        }

        public void RemoveCondition(Condition condition)
        {
            _conditions.Remove(condition);
        }
        #endregion

        public Condition CreateDefaultCondition()
        {
            Condition condition = new Condition
            {
                ComparisonType = ComparisonType.IsEqual,
                VariableType = VariableType.Boolean,
                IsOutsideVariable = false
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
                VariableType = VariableType.Boolean,
                IsOutsideVariable = false
            };

            AddCondition(condition);

            return condition;
        }

        #region IO
        public InputPort InputPort
        {
            get { return _inputPort; }
            set { _inputPort = value; }
        }

        public OutputPort IfOutputPort
        {
            get { return _ifOutputPort; }
            set { _ifOutputPort = value; }
        }

        public OutputPort ElseOutputPort
        {
            get { return _elseOutputPort; }
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
            return new List<OutputPort>() { _ifOutputPort, _elseOutputPort };
        }

        public new Node NextNode()
        {
            return EvaluatedOutputPort != null ?EvaluatedOutputPort.Connection.End.Node : null;
        }

        public void AssignNodesToOutputPorts(Node node)
        {
            _ifOutputPort.Node = node;
            _elseOutputPort.Node = node;
        }

        public void OffsetMultiplePorts(Vector2 offset)
        {
            IfOutputPort.Position += offset;
            ElseOutputPort.Position += offset;
        }

        #endregion

        public void ClearMultipleConnections()
        {
            //Destroy(IfOutputPort);
            //Destroy(ElseOutputPort);
            IfOutputPort = null;
            ElseOutputPort = null;
            _evaluatedOutputPort = null;
        }
    }
}
