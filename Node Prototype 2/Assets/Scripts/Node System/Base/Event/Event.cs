using RPG.Nodes.Base;

namespace Wolfram.RPG.Events
{
    public class Event : DataObject
    {
        private string _name = null;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}
