using System;
using UnityEngine;

namespace RPG.Nodes.Base
{
    public enum VariableType
    {
        None = 0,
        Boolean = 1,
        Float = 2,
        String = 3
    }

    [Serializable]
    public abstract class BaseValue : BaseScriptableObject
    {
        private static Type[] _typeRegister = null;
        private static Type[] TypeRegister
        {
            get
            {
                if (_typeRegister != null) return _typeRegister;

                //Lazy Initialization
                _typeRegister = new Type[4];
                _typeRegister[0] = null;
                _typeRegister[1] = typeof(bool);
                _typeRegister[2] = typeof(float);
                _typeRegister[3] = typeof(string);
                return _typeRegister;
            }
        }
        private static Type GetRealType(VariableType fakeType)
        {
            return TypeRegister[(int)fakeType];
        }
        private static VariableType GetFakeType(Type realType)
        {
            return (VariableType)Array.IndexOf(TypeRegister, realType);
        }

        [SerializeField]
        private VariableType _variableTypeEnum = VariableType.None;
        private Type _variableTypeReal = null;

        [SerializeField]
        [HideInInspector]
        private object _value = null;

        public Type RealType
        {
            get { return _variableTypeReal ?? (_variableTypeReal = GetRealType(EnumType)); }
            set
            {
                _variableTypeReal = value;
                _variableTypeEnum = GetFakeType(value);
            }
        }
        public VariableType EnumType
        {
            get { return _variableTypeEnum; }
            set
            {
                _variableTypeEnum = value;
                _variableTypeReal = GetRealType(value);
            }
        }

        public object UncastValue
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

        public bool IsBoolean { get { return EnumType == VariableType.Boolean; } }
        public bool IsFloat { get { return EnumType == VariableType.Float; } }
        public bool IsString { get { return EnumType == VariableType.String; } }
        public bool HasType { get { return EnumType != VariableType.None; } }
    }

    public class Value : BaseValue { }
}
