using System;
using RPG.Base;
using UnityEngine;

namespace RPG.Nodes.Base
{
    public enum ComparisonType
    {
        None,
        IsEqual,
        IsNotEqual,
        GreaterThan,
        LesserThan,
        GreaterThanOrEqual,
        LesserThanOrEqual
    };

    [Serializable]
    public class Condition : BaseObject
    {
        [SerializeField]
        private ComparisonType _comparison = ComparisonType.None;

        [SerializeField]
        private bool _usingBuiltInValue = false;

        [SerializeField]
        private Variable _variable = null;

        //[SerializeField]
        private BaseValue _actualValue = null;

        [SerializeField]
        private Value _localValue = null;
        [SerializeField]
        private Variable _otherVariable = null;

        //public VariableType VariableType { get { return Variable != null ? Variable.EnumType : VariableType.None; } }
        public ComparisonType ComparisonType
        {
            get { return _comparison; }
            set { _comparison = value; }
        }

        public Variable Variable
        {
            get { return _variable; }
            set { _variable = value; }
        }

        public bool UsingBuiltInValue
        {
            get { return _usingBuiltInValue; }
            set
            {
                _usingBuiltInValue = value;
                if (value) _actualValue = OtherVariable;
                else _actualValue = LocalValue;
            }
        }

        public BaseValue ActualValue
        {
            get
            {
                if (UsingBuiltInValue) _actualValue = OtherVariable;
                else _actualValue = LocalValue;
                return _actualValue;
            }
        }
        public Value LocalValue
        {
            get { return _localValue; }
            set { _localValue = value; }
        }
        public Variable OtherVariable
        {
            get { return _otherVariable; }
            set { _otherVariable = value; }
        }

        public bool Evaluate()
        {
            return Variable != null && Evaluate(Variable.EnumType);
        }

        #region Evaluate
        private bool Evaluate(VariableType variableType)
        {
            switch (variableType)
            {
                case VariableType.Boolean: return EvaluateBoolean(_comparison);
                case VariableType.Float: return EvaluateFloat(_comparison);
                case VariableType.String: return EvaluateString(_comparison);
            }

            return false;
        }

        private bool EvaluateBoolean(ComparisonType comparison)
        {
            switch (comparison)
            {
                case ComparisonType.IsEqual:return Variable.BoolValue == ActualValue.BoolValue;
                case ComparisonType.IsNotEqual:return Variable.BoolValue != ActualValue.BoolValue;
            }

            return false;
        }

        private bool EvaluateString(ComparisonType comparison)
        {
            switch (comparison)
            {
                case ComparisonType.IsEqual:return _variable.StringValue == ActualValue.StringValue;
                case ComparisonType.IsNotEqual:return _variable.StringValue != ActualValue.StringValue;
            }

            return false;
        }

        private bool EvaluateFloat(ComparisonType comparison, float epsilon = float.Epsilon)
        {
            bool areEqual = NearlyEqual(_variable.FloatValue, ActualValue.FloatValue, epsilon);

            switch (comparison)
            {
                case ComparisonType.IsEqual:
                    return areEqual;
                case ComparisonType.IsNotEqual:
                    return !areEqual;
                case ComparisonType.GreaterThan:
                    return !areEqual && _variable.FloatValue > ActualValue.FloatValue;
                case ComparisonType.LesserThan:
                    return !areEqual && _variable.FloatValue < ActualValue.FloatValue;
                case ComparisonType.GreaterThanOrEqual:
                    return areEqual || _variable.FloatValue > ActualValue.FloatValue;
                case ComparisonType.LesserThanOrEqual:
                    return areEqual || _variable.FloatValue < ActualValue.FloatValue;
            }

            return false;
        }

        public static bool NearlyEqual(float a, float b, float epsilon)
        {
            float absoluteA = Mathf.Abs(a);
            float absoluteB = Mathf.Abs(b);
            float difference = Mathf.Abs(a - b);

            //shortcut, handles infinity
            if (a == b) return true;

            if (a == 0 || b == 0 || difference < float.MinValue)
            {
                //a or b is zero or both are extremely close to it
                //relative error is less meaningful here
                return difference < epsilon * float.MinValue;
            }
            //shortcut, handles infinity

            return difference / Math.Min(absoluteA + absoluteB, float.MaxValue) < epsilon;
        }

        //private bool EvaluateFloat(ComparisonType comparison)
        //{
        //    bool result = false;

        //    switch (comparison)
        //    {
        //        case ComparisonType.IsEqual:
        //            result = Math.Abs(_variable.FloatValue - _ActualCheckingVariable.FloatValue) < float.Epsilon;
        //            break;
        //        case ComparisonType.IsNotEqual:
        //            result = Math.Abs(_variable.FloatValue - _ActualCheckingVariable.FloatValue) > float.Epsilon;
        //            break;
        //        case ComparisonType.GreaterThan:
        //            result = _variable.FloatValue > _ActualCheckingVariable.FloatValue;
        //            break;
        //        case ComparisonType.LesserThan:
        //            result = _variable.FloatValue < _ActualCheckingVariable.FloatValue;
        //            break;
        //        case ComparisonType.GreaterThanOrEqual:
        //            result = _variable.FloatValue >= _ActualCheckingVariable.FloatValue;
        //            break;
        //        case ComparisonType.LesserThanOrEqual:
        //            result = _variable.FloatValue <= _ActualCheckingVariable.FloatValue;
        //            break;
        //        case ComparisonType.None:
        //        default:
        //            break; //throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null);
        //    }

        //    return result;
        //}
        #endregion
    }
}
