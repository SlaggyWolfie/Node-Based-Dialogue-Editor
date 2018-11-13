namespace RPG.Nodes.Base
{
    public class IdentifiableObject
    {
        protected int _id = -1;

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }
    }
}