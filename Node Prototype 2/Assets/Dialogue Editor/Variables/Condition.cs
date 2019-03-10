using System;
using UnityEngine;
using WolfEditor.Base;
using WolfEditor.Variables;
using Comparison = WolfEditor.Variables.Variable.Comparison;

namespace WolfEditor.Variables
{
    [Serializable]
    public class Condition : BaseObject
    {
        public enum Type { Bool, Float, Int, String }

        [SerializeField] private Type _type = Type.Bool;
        [SerializeField] private Comparison _comparison = Comparison.IsEqual;

        [SerializeField/*, HideInInspector*/] private FloatPair _floatPair = null;
        [SerializeField/*, HideInInspector*/] private IntPair _intPair = null;
        [SerializeField/*, HideInInspector*/] private StringPair _stringPair = null;
        [SerializeField/*, HideInInspector*/] private BoolPair _boolPair = null;

        public Pair ActivePair
        {
            get
            {
                switch (_type)
                {
                    case Type.Bool: return _boolPair;
                    case Type.Float: return _floatPair;
                    case Type.Int: return _intPair;
                    case Type.String: return _stringPair;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        public Type VariableType { get { return _type; } }

        public bool Evaluate()
        {
            Pair activePair = ActivePair;
            if (activePair == null || !activePair.VariableExists() || !activePair.ReferenceExists()) return false;

            switch (_type)
            {
                case Type.Bool: return _boolPair.variable.Compare(_comparison, _boolPair.reference);
                case Type.Float: return _floatPair.variable.Compare(_comparison, _floatPair.reference);
                case Type.Int: return _intPair.variable.Compare(_comparison, _intPair.reference);
                case Type.String: return _stringPair.variable.Compare(_comparison, _stringPair.reference);
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
