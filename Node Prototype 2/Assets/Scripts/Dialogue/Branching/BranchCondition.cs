using System.Collections.Generic;
using RPG.Nodes.Base;

namespace RPG.Nodes
{
    public class BranchCondition : ObjectWithID
    {
        private List<Condition> _conditions = new List<Condition>();
        private bool _isAnd = true;

        public bool Evaluate()
        {
            bool result = false;

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

            return result;
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
    }
}
