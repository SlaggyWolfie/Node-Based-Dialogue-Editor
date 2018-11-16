using RPG.Nodes.Base;

namespace RPG.Nodes.Base
{
    public abstract class ConnectionModifier : ObjectWithID
    {
        private Connection _connection = null;

        public Connection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public abstract void Execute();
        public ConnectionModifier ShallowCopy() { return (ConnectionModifier)MemberwiseClone(); }
        public abstract ConnectionModifier DeepCopy();
    }
}
