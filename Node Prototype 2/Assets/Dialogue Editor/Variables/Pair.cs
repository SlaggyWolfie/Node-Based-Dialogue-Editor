using System;

namespace WolfEditor.Variables
{
    [Serializable]
    public abstract class Pair
    {
        public abstract Variable GetGenericVariable();
        public abstract Reference GetGenericReference();

        public abstract bool VariableExists();
        public abstract bool ReferenceExists();
    }

    [Serializable]
    public abstract class Pair<TVariable, TReference> : Pair
        where TVariable : Variable
        where TReference : Reference
    {
        public TVariable variable;
        public TReference reference;

        public sealed override Variable GetGenericVariable() { return variable; }
        public sealed override Reference GetGenericReference() { return reference; }

        public sealed override bool ReferenceExists() { return variable != null; }
        public sealed override bool VariableExists() { return reference != null; }
    }

    [Serializable] public sealed class FloatPair : Pair<FloatVariable, FloatReference> { }
    [Serializable] public sealed class IntPair : Pair<IntVariable, IntReference> { }
    [Serializable] public sealed class BoolPair : Pair<BoolVariable, BoolReference> { }
    [Serializable] public sealed class StringPair : Pair<StringVariable, StringReference> { }
}