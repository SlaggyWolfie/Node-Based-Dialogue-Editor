using System;
using RPG.Base;
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

        //[SerializeField]
        //[HideInInspector]
        //private object _value = null;
        [SerializeField] private bool _boolValue = false;
        [SerializeField] private float _floatValue = 0;
        [SerializeField] private string _stringValue = string.Empty;

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

        //public object UncastValue
        //{
        //    get { return _value; }
        //    set { _value = value; }
        //}
        public object Value
        {
            get
            {
                switch (EnumType)
                {
                    case VariableType.Boolean: return BoolValue;
                    case VariableType.Float: return FloatValue;
                    default:
                    case VariableType.String: return StringValue;
                }
            }
            set
            {
                switch (EnumType)
                {
                    case VariableType.Boolean:
                        BoolValue = (bool)value;
                        break;
                    case VariableType.Float:
                        FloatValue = (float)value;
                        break;
                    case VariableType.String:
                        StringValue = value.ToString();
                        break;
                }
            }
        }
        public bool BoolValue
        {
            get { return _boolValue; }
            set { _boolValue = value; }
        }
        public float FloatValue
        {
            get { return _floatValue; }
            set { _floatValue = value; }
        }
        public string StringValue
        {
            get { return _stringValue; }
            set { _stringValue = value; }
        }

        public bool IsBoolean { get { return EnumType == VariableType.Boolean; } }
        public bool IsFloat { get { return EnumType == VariableType.Float; } }
        public bool IsString { get { return EnumType == VariableType.String; } }
        public bool HasType { get { return EnumType != VariableType.None; } }
    }
}
