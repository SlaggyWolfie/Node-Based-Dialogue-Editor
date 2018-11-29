using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Nodes.Base
{
    [Serializable]
    public class VariableRepository
    {
        private const bool PRESCRIPTIVE_OR_DESCRIPTIVE = true;

        private VariableLocation _location = VariableLocation.None;
        [SerializeField]
        private List<Variable> _variables = new List<Variable>();

        public VariableLocation Location
        {
            get { return _location; }
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
            return _variables[index];
        }

        public void RemoveVariable(int index)
        {
            _variables.RemoveAt(index);
        }
        #endregion

        public void AddVariable(Variable variable)
        {
#pragma warning disable 0162
            if (PRESCRIPTIVE_OR_DESCRIPTIVE)
            {
                if (variable.Location == Location)
                    _variables.Add(variable);
                else throw new InvalidOperationException("Wrong variable location!");
            }
            else
            {
                _variables.Add(variable);
                variable.Location = Location;
            }
#pragma warning restore 0162

            //_variables.Add(variable);
        }

        public void RemoveVariable(Variable variable)
        {
            _variables.Remove(variable);
        }
        #endregion
    }
}
