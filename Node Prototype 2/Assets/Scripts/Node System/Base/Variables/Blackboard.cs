namespace RPG.Nodes.Base
{
    public class Blackboard : DataObject
    {
        private NodeGraph _currentNodeGraph = null;

        private VariableRepository _globalVariableRepository = null;
        private VariableRepository _currentLocalVariableRepository = null;

        public VariableRepository GlobalVariableRepository
        {
            get
            {
                return _globalVariableRepository ??
                       (_globalVariableRepository = new VariableRepository() { Location = VariableLocation.Global });
            }
            set { _globalVariableRepository = value; }
        }

        public VariableRepository CurrentLocalVariableRepository
        {
            get { return _currentLocalVariableRepository; }
        }

        public NodeGraph CurrentNodeGraph
        {
            get { return _currentNodeGraph; }
            set
            {
                _currentNodeGraph = value;
                _currentLocalVariableRepository = value.LocalVariableRepository;
                //_currentSceneVariableRepository = value.SceneVariableRepository;
            }
        }
    }
}
