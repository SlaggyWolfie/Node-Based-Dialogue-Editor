using System;
using System.Collections;
using System.Collections.Generic;
using WolfEditor.Base;
using UnityEngine;

namespace WolfEditor.Variables
{
    public abstract class Variable : BaseScriptableObject
    {
#if UNITY_EDITOR
#pragma warning disable 0414
        [SerializeField] private string _developerNote = string.Empty;
#pragma warning restore 0414
#endif

        public enum Operation { Set, Add, Subtract, Multiply, Divide }
        public abstract IEnumerable<Operation> GetPermittedOperations();

        public enum Comparison { Equal, NotEqual, GreaterThan, LesserThan, GreaterThanOrEqual, LesserThanOrEqual };
        public abstract IEnumerable<Comparison> GetPermittedComparisons();
    }

    public abstract class Variable<T> : Variable
    {
        [Space]
        [SerializeField] private T _initialValue = default(T);
        [Space]
        [SerializeField, Utility.ReadOnly] private bool _initialized = false;
        [SerializeField, Utility.ReadOnly] private T _currentValue = default(T);

        public T CurrentValue
        {
            get
            {
                if (_initialized) return _currentValue;

                _currentValue = _initialValue;
                _initialized = true;

                return _currentValue;
            }
            set
            {
                _currentValue = value;
                _initialized = true;
            }
        }

        public abstract void Execute(Operation operation, T other);
        public abstract bool Compare(Comparison comparison, T other);

        public static implicit operator T(Variable<T> variable)
        {
            return variable._currentValue;
        }
    }
}
