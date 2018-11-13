using System;
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

    public class Condition : ScriptableObjectWithID
    {
        private VariableType _variableType = VariableType.None;
        private ComparisonType _comparison = ComparisonType.None;

        private bool _isOutsideVariable = false;

        private Variable _variable = null;

        private BaseValue _actualCheck = null;

        private Value _localValue = null;
        private Variable _otherVariable = null;

        private BaseValue _ActualCheck
        {
            get
            {
                if (IsOutsideVariable) _actualCheck = OtherVariable;
                else _actualCheck = LocalValue;

                return _actualCheck;
            }
        }

        public VariableType VariableType
        {
            get { return _variableType; }
            set { _variableType = value; }
        }

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

        public bool IsOutsideVariable
        {
            get { return _isOutsideVariable; }
            set
            {
                _isOutsideVariable = value;

                if (value) _actualCheck = OtherVariable;
                else _actualCheck = LocalValue;
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
            return Evaluate(_variableType);
        }

        #region Evaluate
        private bool Evaluate(VariableType variableType)
        {
            bool result = false;
            switch (variableType)
            {
                case VariableType.Boolean:
                    result = EvaluateBoolean(_comparison);
                    break;
                case VariableType.Float:
                    result = EvaluateFloat(_comparison);
                    break;
                case VariableType.String:
                    result = EvaluateString(_comparison);
                    break;
                case VariableType.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        private bool EvaluateBoolean(ComparisonType comparison)
        {
            bool result = false;

            switch (comparison)
            {
                case ComparisonType.IsEqual:
                    result = _variable.BoolValue == _ActualCheck.BoolValue;
                    break;
                case ComparisonType.IsNotEqual:
                    result = _variable.BoolValue != _ActualCheck.BoolValue;
                    break;
                case ComparisonType.None:
                case ComparisonType.GreaterThan:
                case ComparisonType.LesserThan:
                case ComparisonType.GreaterThanOrEqual:
                case ComparisonType.LesserThanOrEqual:
                default:
                    break; //throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null);
            }

            return result;
        }

        private bool EvaluateString(ComparisonType comparison)
        {
            bool result = false;

            switch (comparison)
            {
                case ComparisonType.IsEqual:
                    result = _variable.StringValue == _ActualCheck.StringValue;
                    break;
                case ComparisonType.IsNotEqual:
                    result = _variable.StringValue != _ActualCheck.StringValue;
                    break;
                case ComparisonType.None:
                case ComparisonType.GreaterThan:
                case ComparisonType.LesserThan:
                case ComparisonType.GreaterThanOrEqual:
                case ComparisonType.LesserThanOrEqual:
                default:
                    break; //throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null);
            }

            return result;
        }

        private bool EvaluateFloat(ComparisonType comparison, float epsilon = float.Epsilon)
        {
            bool result = false;
            bool areEqual = NearlyEqual(_variable.FloatValue, _ActualCheck.FloatValue, epsilon);

            switch (comparison)
            {
                case ComparisonType.IsEqual:
                    result = areEqual;
                    break;
                case ComparisonType.IsNotEqual:
                    result = !areEqual;
                    break;
                case ComparisonType.GreaterThan:
                    result = !areEqual && _variable.FloatValue > _ActualCheck.FloatValue;
                    break;
                case ComparisonType.LesserThan:
                    result = !areEqual && _variable.FloatValue < _ActualCheck.FloatValue;
                    break;
                case ComparisonType.GreaterThanOrEqual:
                    result = areEqual || _variable.FloatValue > _ActualCheck.FloatValue;
                    break;
                case ComparisonType.LesserThanOrEqual:
                    result = areEqual || _variable.FloatValue < _ActualCheck.FloatValue;
                    break;
                case ComparisonType.None:
                default:
                    break; //throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null);
            }

            return result;
        }

        private static bool NearlyEqual(float a, float b, float epsilon)
        {
            float absoluteA = Math.Abs(a);
            float absoluteB = Math.Abs(b);
            float difference = Math.Abs(a - b);

            //shortcut, handles infinity
            if (a == b) return true;
            else if (a == 0 || b == 0 || difference < float.MinValue)
            {
                //a or b is zero or both are extremely close to it
                //relative error is less meaningful here
                return difference < epsilon * float.MinValue;
            }
            //shortcut, handles infinity
            else return difference / Math.Min(absoluteA + absoluteB, float.MaxValue) < epsilon;
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
