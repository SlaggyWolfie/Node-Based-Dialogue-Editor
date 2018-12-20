using System;
using System.Collections.Generic;
using RPG.Base;
using RPG.Nodes.Base;
using UnityEngine;

namespace RPG.Nodes
{
    [Serializable]
    public class BranchCondition : BaseObject
    {
        [SerializeField]
        private List<Condition> _conditions = new List<Condition>();
        [SerializeField]
        private bool _allConditionsMustBeTrue = true;

        public bool Evaluate()
        {
            //AND &&
            if (_allConditionsMustBeTrue)
            {
                foreach (Condition condition in _conditions)
                {
                    if (!condition.Evaluate()) return false;
                }
                return true;
            }

            //OR ||
            foreach (Condition condition in _conditions)
            {
                if (condition.Evaluate()) return true;
            }
            return false;
        }

        public bool AllConditionsMustBeTrue
        {
            get { return _allConditionsMustBeTrue; }
            set { _allConditionsMustBeTrue = value; }
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
    }
}
