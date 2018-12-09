using System;
using UnityEngine;

namespace RPG.Nodes.Base
{
    public enum VariableLocation
    {
        None
        , Local
        , Global
        //, Scene
    }

    [Serializable]
    public class Variable : BaseValue
    {
        [SerializeField]
        private VariableLocation _variableLocation = VariableLocation.None;
        public VariableLocation Location
        {
            get { return _variableLocation; }
            internal set { _variableLocation = value; }
        }

        [SerializeField]
        [HideInInspector]
        private VariableInventory _variableInventory = null;
        public VariableInventory VariableInventory
        {
            get { return _variableInventory; }
            set
            {
                _variableInventory = value;
                Location = value.Location;
            }
        }
    }
}
