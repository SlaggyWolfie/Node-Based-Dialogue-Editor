using System;
using WolfEditor.Utility;
using UnityEngine;
using WolfEditor.Base;
using WolfEditor.Nodes.Base;

namespace WolfEditor.Variables
{
    [Serializable]
    public sealed class EditVariableModifier : Instruction//, ICopyable<EventModifier>
    {
        public enum Type { Bool, Float, Int, String }

        [SerializeField] private Type _type = Type.Bool;
        [SerializeField] private Variable.Operation _operation = Variable.Operation.Set;
        [SerializeField/*, HideInInspector*/] private FloatPair _floatPair = null;
        [SerializeField/*, HideInInspector*/] private IntPair _intPair = null;
        [SerializeField/*, HideInInspector*/] private StringPair _stringPair = null;
        [SerializeField/*, HideInInspector*/] private BoolPair _boolPair = null;

        private Pair ActivePair
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

        public override void Execute()
        {
            Pair activePair = ActivePair;
            if (activePair == null || !activePair.VariableExists() || !activePair.ReferenceExists()) return;

            switch (_type)
            {
                case Type.Bool: _boolPair.variable.Execute(_operation, _boolPair.reference); break;
                case Type.Float: _floatPair.variable.Execute(_operation, _floatPair.reference); break;
                case Type.Int: _intPair.variable.Execute(_operation, _intPair.reference); break;
                case Type.String: _stringPair.variable.Execute(_operation, _stringPair.reference); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

    }

}
