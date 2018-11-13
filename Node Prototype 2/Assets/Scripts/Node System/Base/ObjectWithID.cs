namespace RPG.Nodes.Base
{
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