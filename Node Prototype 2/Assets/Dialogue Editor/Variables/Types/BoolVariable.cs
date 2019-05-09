using System;
using System.Collections.Generic;
using UnityEngine;

namespace WolfEditor.Variables
{
    [CreateAssetMenu]
    public class BoolVariable : Variable<bool>
    {
        public static readonly Operation[] permittedOperations = { Operation.Set };
        public override IEnumerable<Operation> GetPermittedOperations() { return permittedOperations; }

        public static readonly Comparison[] permittedComparisons = { Comparison.Equal, Comparison.NotEqual };
        public override IEnumerable<Comparison> GetPermittedComparisons() { return permittedComparisons; }

        public override void Execute(Operation operation, bool other)
        {
            switch (operation)
            {
                case Operation.Set: CurrentValue = other; break;
                default: throw new ArgumentOutOfRangeException("operation", operation, "Invalid Variable Operation.");
            }
        }

        public override bool Compare(Comparison comparison, bool other)
        {
            switch (comparison)
            {
                case Comparison.Equal: return CurrentValue == other;
                case Comparison.NotEqual: return CurrentValue != other;
                default: throw new ArgumentOutOfRangeException("comparison", comparison, "Invalid Variable Comparison.");
            }
        }
    }
}