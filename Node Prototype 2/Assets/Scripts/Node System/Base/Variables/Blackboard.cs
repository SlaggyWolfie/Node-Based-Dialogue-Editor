using System;
using UnityEditor;
using UnityEngine;

namespace RPG.Nodes.Base
{
    public class Blackboard
    {
        private static Blackboard _blackboard = null;

        public static Blackboard Instance
        {
            get { return _blackboard ?? new Blackboard(); }
        }

        private VariableInventory _globalVariableInventory = null;
        private VariableInventory _currentLocalVariableInventory = null;

        public VariableInventory GlobalVariableInventory
        {
            get
            {
                return _globalVariableInventory ?? (_globalVariableInventory = GetAGlobalVariableInventory());
            }
        }

        public VariableInventory CurrentLocalVariableInventory
        {
            get { return _currentLocalVariableInventory; }
            set { _currentLocalVariableInventory = value; }
        }

        public Blackboard()
        {
            _globalVariableInventory = GetAGlobalVariableInventory();
        }

        private VariableInventory GetAGlobalVariableInventory()
        {
            //TODO: Test this
            //Search for assets with this label
            string[] GUIDs = AssetDatabase.FindAssets("l:global_variable_inventory");

            //If any assets are found, take the first and load it as a Variable Inventory
            if (GUIDs.Length != 0)
                return AssetDatabase.LoadAssetAtPath<VariableInventory>(AssetDatabase.GUIDToAssetPath(GUIDs[0]));

            //If no assets are found, create one
            VariableInventory globalVariableInventory = ScriptableObject.CreateInstance<VariableInventory>();
            globalVariableInventory.name = "Global Variable Inventory";
            globalVariableInventory.Location = VariableLocation.Global;
            AssetDatabase.SetLabels(globalVariableInventory, new[] { "global_variable_inventory", "variable_inventory" });
            AssetDatabase.CreateAsset(globalVariableInventory, string.Format("Assets/{0}.asset", globalVariableInventory.name));

            return globalVariableInventory;
        }
    }
}
