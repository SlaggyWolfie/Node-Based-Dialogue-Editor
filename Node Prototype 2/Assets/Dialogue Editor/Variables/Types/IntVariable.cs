using System;
using System.Collections.Generic;
using UnityEngine;
using WolfEditor.Base;

namespace WolfEditor.Variables
{
    [CreateAssetMenu]
    public class IntVariable : Variable<int>
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

        public override void Execute(Operation operation, int other)
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

        public override bool Compare(Comparison comparison, int other)
        {
            switch (comparison)
            {
                case Comparison.Equal: return CurrentValue == other;
                case Comparison.NotEqual: return CurrentValue != other;
                case Comparison.GreaterThan: return CurrentValue > other;
                case Comparison.LesserThan: return CurrentValue < other;
                case Comparison.GreaterThanOrEqual: return CurrentValue >= other;
                case Comparison.LesserThanOrEqual: return CurrentValue <= other;
                default: throw new ArgumentOutOfRangeException("comparison", comparison, "Invalid Variable Comparison.");
            }
        }
    }
}