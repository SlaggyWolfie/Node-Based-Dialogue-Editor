using System;
using UnityEngine;

namespace RPG.Nodes.Base
{
    public enum VariableType
    {
        None,
        Boolean,
        Float,
        String
    };

    [Serializable]
    public abstract class BaseValue : ScriptableObjectWithID
    {
        private Type _variableTypeReal = null;

        [SerializeField]
        private VariableType _variableTypeEnum = VariableType.None;

        [SerializeField]
        private object _value = null;

        public Type RealType
        {
            get { return _variableTypeReal; }
            set { _variableTypeReal = value; }
        }

        public VariableType EnumType
        {
            get { return _variableTypeEnum; }
            set { _variableTypeEnum = value; }
        }

        public object _Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public bool BoolValue
        {
            get { return (bool)_value; }
            set { _value = value; }
        }

        public float FloatValue
        {
            get { return (float)_value; }
            set { _value = value; }
        }

        public string StringValue
        {
            get { return (string)_value; }
            set { _value = value; }
        }
    }

    public class Value : BaseValue { }
}
