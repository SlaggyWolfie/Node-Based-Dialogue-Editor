using System;
using UnityEngine;

namespace RPG.Nodes.Base
{
    [Serializable]
    public class Blackboard : BaseScriptableObject
    {
        [SerializeField]
        private NodeGraph _currentNodeGraph = null;
        
        [SerializeField]
        private VariableInventory _globalVariableInventory = null;
        [SerializeField]
        private VariableInventory _currentLocalVariableInventory = null;

        public VariableInventory GlobalVariableInventory
        {
            get
            {
                return _globalVariableInventory ??
                       (_globalVariableInventory = new VariableInventory() { Location = VariableLocation.Global });
            }
            set { _globalVariableInventory = value; }
        }

        public VariableInventory CurrentLocalVariableInventory
        {
            get { return _currentLocalVariableInventory; }
        }

        public NodeGraph CurrentNodeGraph
        {
            get { return _currentNodeGraph; }
            set
            {
                _currentNodeGraph = value;
                _currentLocalVariableInventory = value.LocalVariableInventory;
                //_currentSceneVariableRepository = value.SceneVariableRepository;
            }
        }
    }
}
