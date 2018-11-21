﻿namespace RPG.Nodes.Base
{
    [System.Serializable]
    public class ObjectWithID
    {
        protected int _id = -1;

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }
    }
}