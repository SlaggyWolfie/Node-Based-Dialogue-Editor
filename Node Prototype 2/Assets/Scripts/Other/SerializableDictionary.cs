using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RPG.Other
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        private int _count = 0;
        private TKey[] _keys;
        private TValue[] _values;

        public void Cache()
        {
            _count = Count;
            _keys = Keys.ToArray();
            _values = Values.ToArray();
        }

        public void OnBeforeSerialize()
        {
            Cache();
            Clear();
        }

        public void OnAfterDeserialize()
        {
            for (int i = 0; i < _count; i++) Add(_keys[i], _values[i]);
            //_keys = new TKey[0];
            //_values = new TValue[0];
        }
    }
}
