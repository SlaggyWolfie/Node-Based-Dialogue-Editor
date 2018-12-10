using System;
using System.Collections.Generic;
using RPG.Base;
using UnityEngine;

namespace RPG.Nodes.Base
{
    [Serializable]
    public sealed class VariableInventory : BaseScriptableObject
    {
        //private const bool PRESCRIPTIVE_OR_DESCRIPTIVE = false;

        [SerializeField]
        [HideInInspector]
        private VariableLocation _location = VariableLocation.None;
        [SerializeField]
        private List<Variable> _variables = new List<Variable>();

        public VariableLocation Location
        {
            get { return _location; }
            /*internal*/
            set { _location = value; }
        }

        #region List Wrapping Interface
        #region Standard Index Stuff
        public int VariableCount
        {
            get { return _variables.Count; }
        }

        public Variable GetVariable(int index)
        {
            if (index < 0 || index >= VariableCount) return null;
            return _variables[index];
        }

        public void RemoveVariable(int index)
        {
            _variables.RemoveAt(index);
        }
        #endregion

        public void AddVariable(Variable variable)
        {
            _variables.Add(variable);
            variable.VariableInventory = this;
        }

        public void RemoveVariable(Variable variable)
        {
            if (variable != null) _variables.Remove(variable);
        }
        #endregion
    }
}
