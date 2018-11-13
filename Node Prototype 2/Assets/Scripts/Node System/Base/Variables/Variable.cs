namespace RPG.Nodes.Base
{
    public enum VariableLocation
    {
        None,
        Local,
        Global,
        Scene
    }

    public class Variable : BaseValue
    {
        private string _name = null;

        private VariableLocation _variableLocation = VariableLocation.None;
        
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public VariableLocation Location
        {
            get { return _variableLocation; }
            set { _variableLocation = value; }
        }
    }
}
