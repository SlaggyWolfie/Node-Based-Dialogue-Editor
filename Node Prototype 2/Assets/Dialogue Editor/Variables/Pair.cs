using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WolfEditor.Base;

namespace WolfEditor.Variables
{
    [Serializable]
    public abstract class Pair
    {
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

        public override bool ReferenceExists() { return variable != null; }
        public override bool VariableExists() { return reference != null; }
    }

    [Serializable] public sealed class FloatPair : Pair<FloatVariable, FloatReference> { }
    [Serializable] public sealed class IntPair : Pair<IntVariable, IntReference> { }
    [Serializable] public sealed class BoolPair : Pair<BoolVariable, BoolReference> { }
    [Serializable] public sealed class StringPair : Pair<StringVariable, StringReference> { }
}