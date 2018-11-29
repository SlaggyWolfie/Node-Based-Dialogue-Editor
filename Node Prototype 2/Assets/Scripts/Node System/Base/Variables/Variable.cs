using System;
using UnityEngine;

namespace RPG.Nodes.Base
{
    public enum VariableLocation
    {
        None,
        Local,
        Global,
        Scene
    }

    [Serializable]
    public class Variable : BaseValue
    {
        [SerializeField]
        private string _name = null;

        [SerializeField]
        private VariableLocation _variableLocation = VariableLocation.None;
        
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public VariableLocation Location
        {
            get { return _variableLocation; }
            set { _variableLocation = value; }
        }
    }
}
