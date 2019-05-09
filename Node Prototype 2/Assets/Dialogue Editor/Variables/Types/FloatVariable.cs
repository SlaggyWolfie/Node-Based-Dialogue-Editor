using System;
using System.Collections.Generic;
using UnityEngine;

namespace WolfEditor.Variables
{
    [CreateAssetMenu]
    public class FloatVariable : Variable<float>
    {
        public static readonly Operation[] permittedOperations =
        {
            Operation.Set,
            Operation.Add, Operation.Subtract,
            Operation.Multiply, Operation.Divide
        };
        public override IEnumerable<Operation> GetPermittedOperations() { return permittedOperations; }

        public static readonly Comparison[] permittedComparisons =
        {
            Comparison.Equal, Comparison.NotEqual,
            Comparison.GreaterThan, Comparison.LesserThan,
            Comparison.GreaterThanOrEqual, Comparison.LesserThanOrEqual
        };
        public override IEnumerable<Comparison> GetPermittedComparisons() { return permittedComparisons; }

        public override void Execute(Operation operation, float other)
        {
            switch (operation)
            {
                case Operation.Set: CurrentValue = other; break;
                case Operation.Add: CurrentValue += other; break;
                case Operation.Subtract: CurrentValue -= other; break;
                case Operation.Multiply: CurrentValue *= other; break;
                case Operation.Divide: CurrentValue /= other; break;
                default: throw new ArgumentOutOfRangeException("operation", operation, "Invalid Variable Operation.");
            }
        }

        public override bool Compare(Comparison comparison, float other)
        {
            switch (comparison)
            {
                case Comparison.Equal: return IsEqualWithError(CurrentValue, other, float.Epsilon);
                case Comparison.NotEqual: return IsNotEqualWithError(CurrentValue, other, float.Epsilon);
                case Comparison.GreaterThan:return IsNotEqualWithError(CurrentValue, other, float.Epsilon) && this > other;
                case Comparison.LesserThan: return IsNotEqualWithError(CurrentValue, other, float.Epsilon) && this < other;
                case Comparison.GreaterThanOrEqual: return IsEqualWithError(CurrentValue, other, float.Epsilon) && this > other;
                case Comparison.LesserThanOrEqual: return IsEqualWithError(CurrentValue, other, float.Epsilon) && this < other;
                default: throw new ArgumentOutOfRangeException("comparison", comparison, "Invalid Variable Comparison.");
            }
        }

        private static bool IsEqualWithError(float lhs, float rhs, float error)
        {
            return Math.Abs(lhs - rhs) < error;
        }

        private static bool IsNotEqualWithError(float lhs, float rhs, float error)
        {
            return Math.Abs(lhs - rhs) > error;
        }
    }
}