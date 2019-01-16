using NaughtyAttributes;
using UnityEngine;

namespace RPG.Base
{
    [System.Serializable]
    public class BaseScriptableObject : ScriptableObject
    {
        [ShowNativeProperty]
        private int HashCode { get { return GetHashCode(); } }

        [SerializeField, HideInInspector]
        protected int _id = -1;

        [ShowNativeProperty]
        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }
    }
}