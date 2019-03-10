using System;
using RPG.Nodes.Base;
using RPG.Utility;
using UnityEngine;

namespace RPG.Dialogue
{
    [Serializable]
    public sealed class EditVariableModifier : ConnectionModifier//, ICopyable<EventModifier>
    {
        public enum EditOperation { Set, Add, Subtract, Multiply, Divide }

        [SerializeField]
        private Variable _variable = null;
        public Variable Variable
        {
            get { return _variable; }
            set { _variable = value; }
        }

        [SerializeField]
        private EditOperation _operation = EditOperation.Set;
        public EditOperation Operation
        {
            get { return _operation; }
            set { _operation = value; }
        }

        [SerializeField]
        private bool _usingBuiltInValue = true;
        public bool UsingBuiltInValue
        {
            get { return _usingBuiltInValue; }
            set { _usingBuiltInValue = value; }
        }

        private BaseValue _actualValue = null;
        public BaseValue ActualValue
        {
            get
            {
                if (_actualValue != null) return _actualValue;

                if (_usingBuiltInValue) _actualValue = LocalValue;
                else _actualValue = OtherVariable;
                return _actualValue;
            }
        }

        [SerializeField]
        private Value _localValue = null;
        public Value LocalValue
        {
            get { return _localValue; }
            set { _localValue = value; }
        }

        [SerializeField]
        private Variable _otherVariable = null;
        public Variable OtherVariable
        {
            get { return _otherVariable; }
            set { _otherVariable = value; }
        }

        public override void Execute()
        {
            if (Variable == null) return;
            switch (Variable.EnumType)
            {
                case VariableType.Float:
                    switch (Operation)
                    {
                        case EditOperation.Set: Variable.FloatValue = ActualValue.FloatValue; break;
                        case EditOperation.Add: Variable.FloatValue += ActualValue.FloatValue; break;
                        case EditOperation.Subtract: Variable.FloatValue -= ActualValue.FloatValue; break;
                        case EditOperation.Multiply: Variable.FloatValue *= ActualValue.FloatValue; break;
                        case EditOperation.Divide:
                            if (Utilities.NearlyEqual(ActualValue.FloatValue, 0, float.Epsilon)) break;
                            Variable.FloatValue /= ActualValue.FloatValue;
                            break;
                        default: throw new ArgumentOutOfRangeException();
                    }

                    break;

                case VariableType.Boolean: Variable.BoolValue = ActualValue.BoolValue; break;
                case VariableType.String: Variable.StringValue = ActualValue.StringValue; break;
                default: Variable.Value = ActualValue.Value; break;
            }
        }

        //public override void ApplyDataFromCopy(ConnectionModifier original)
        //{
        //    EditVariableModifier evm = original as EditVariableModifier;
        //    if (evm == null) return;

        //    _variable = evm._variable;
        //    _localValue = evm._localValue;
        //    _otherVariable = evm._otherVariable;
        //    _operation = evm._operation;
        //    _usingBuiltInValue = evm._usingBuiltInValue;
        //}

        //public override ConnectionModifier Copy() { return (EditVariableModifier)MemberwiseClone(); }
    }
}
