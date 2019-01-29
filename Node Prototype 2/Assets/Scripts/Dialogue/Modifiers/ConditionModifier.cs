using System;
using System.Collections.Generic;
using RPG.Nodes;
using RPG.Nodes.Base;
using RPG.Utility;
using UnityEngine;

namespace RPG.Dialogue
{
    //Copied and adapted from ConditionNode
    [Serializable]
    public sealed class ConditionModifier : ConnectionModifier, ICondition
    {
        [SerializeField] private List<Value> _values = new List<Value>();
        [SerializeField] private List<Condition> _conditions = new List<Condition>();
        [SerializeField] private bool _isAnd = true;

        public bool IsAnd
        {
            get { return _isAnd; }
            set { _isAnd = value; }
        }

        public int ConditionCount { get { return _conditions.Count; } }
        public Condition GetCondition(int index) { return _conditions[index]; }
        public void RemoveCondition(int index) { _conditions.RemoveAt(index); }
        public bool RemoveCondition(Condition condition) { return _conditions.Remove(condition); }
        public void AddCondition(Condition condition) { if (condition != null) _conditions.Add(condition); }

        public int ValueCount { get { return _values.Count; } }
        public Value GetValue(int index) { return _values[index]; }
        public void AddValue(Value value) { if (value != null) _values.Add(value); }
        public void RemoveValue(int index) { _values.RemoveAt(index); }
        public bool RemoveValue(Value value) { return _values.Remove(value); }

        public bool Evaluate()
        {
            //I realize that both values would align were I to assign
            //result a value in the 'if-else' scopes, so I just assign
            //it a value here.
            bool result = _isAnd;

            //Already have the value on the stack and not on the heap
            //so why not?
            //Also must scope if-else cause nested unscoped scopes.
            if (result)
            {
                foreach (Condition condition in _conditions)
                    if (!condition.Evaluate()) return false;
            }
            else
            {
                foreach (Condition condition in _conditions)
                    if (condition.Evaluate()) return true;
            }

            return result;
        }

        public override void Execute() { /*Does nothing, here cause Connection Modifier*/ }
    }
}
