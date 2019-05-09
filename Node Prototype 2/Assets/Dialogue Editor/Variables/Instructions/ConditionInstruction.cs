using System;
using System.Collections.Generic;
using UnityEngine;
using WolfEditor.Base;
using WolfEditor.Nodes.Base;
using WolfEditor.Utility;

namespace WolfEditor.Variables
{
    [Serializable]
    public sealed class ConditionInstruction : Instruction, ICondition//, ICopyable<EventModifier>
    {
        [SerializeField] private List<Condition> _conditions = new List<Condition>();
        [SerializeField] private bool _logicalAND = true;

        public int ConditionCount { get { return _conditions.Count; } }
        public Condition GetCondition(int index) { return _conditions[index]; }
        public void RemoveCondition(int index) { _conditions.RemoveAt(index); }
        public bool RemoveCondition(Condition condition) { return _conditions.Remove(condition); }
        public void AddCondition(Condition condition) { if (condition != null) _conditions.Add(condition); }

        public bool Evaluate()
        {
            //I realize that both values would align were I to assign
            //result a value in the 'if-else' scopes, so I just assign
            //it a value here.
            bool result = _logicalAND;

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

        public override void Execute() { throw new NotImplementedException(); }
    }
}