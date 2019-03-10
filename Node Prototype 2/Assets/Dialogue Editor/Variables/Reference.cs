using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WolfEditor.Variables
{
    [System.Serializable]
    public abstract class Reference { }

    [System.Serializable]
    public abstract class Reference<TVariable, TType> : Reference
        where TVariable : Variable<TType>
    {
        [SerializeField] private bool _useConstant = true;
        [SerializeField] private TType _constantValue = default(TType);
        [SerializeField] private TVariable _variable = null;

        protected Reference() { }
        protected Reference(TType value)
        {
            _useConstant = true;
            _constantValue = value;
        }
        protected Reference(TVariable variable)
        {
            _useConstant = false;
            _variable = variable;
        }

        public TType Value
        {
            get { return _useConstant ? _constantValue : _variable.CurrentValue; }
        }

        public static implicit operator TType(Reference<TVariable, TType> reference)
        {
            return reference.Value;
        }
    }

    [System.Serializable]
    public sealed class FloatReference : Reference<FloatVariable, float>
    {
        public FloatReference() { }
        public FloatReference(float value) : base(value) { }
        public FloatReference(FloatVariable value) : base(value) { }
    }
    [System.Serializable]
    public sealed class IntReference : Reference<IntVariable, int>
    {
        public IntReference() { }
        public IntReference(int value) : base(value) { }
        public IntReference(IntVariable value) : base(value) { }
    }
    [System.Serializable]
    public sealed class StringReference : Reference<StringVariable, string>
    {
        public StringReference() { }
        public StringReference(string value) : base(value) { }
        public StringReference(StringVariable value) : base(value) { }
    }
    [System.Serializable]
    public sealed class BoolReference : Reference<BoolVariable, bool>
    {
        public BoolReference() { }
        public BoolReference(bool value) : base(value) { }
        public BoolReference(BoolVariable value) : base(value) { }
    }
}
