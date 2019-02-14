using System;
using RPG.Utility;
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
            set { _variableLocation = value; }
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
        
        private bool _initialBoolValue = false;
        private float _initialFloatValue = 0;
        private string _initialStringValue = string.Empty;

        private void OnEnable()
        {
            _initialBoolValue = BoolValue;
            _initialFloatValue = FloatValue;
            _initialStringValue = StringValue;
            //Debug.Log("Init");
            //Extensions.BehaviourCallbackSender.Instance.onApplicationQuit += OnDisable;
            Extensions.BehaviourCallbackSender.Instance.onApplicationQuit += OnDisable;
        }

        private void OnDisable()
        {
            BoolValue = _initialBoolValue;
            FloatValue = _initialFloatValue;
            StringValue = _initialStringValue;
            //Debug.Log("Deinit");
        }
    }
}
