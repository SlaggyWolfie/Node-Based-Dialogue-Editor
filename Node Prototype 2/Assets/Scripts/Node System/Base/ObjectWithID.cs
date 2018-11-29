using NaughtyAttributes;
using UnityEngine;

namespace RPG.Nodes.Base
{
    [System.Serializable]
    public class ObjectWithID
    {
        [ShowNativeProperty]
        private int Hash
        {
            get { return GetHashCode(); }
        }

        [SerializeField]
        [HideInInspector]
        protected int _id = -1;

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }
    }
}