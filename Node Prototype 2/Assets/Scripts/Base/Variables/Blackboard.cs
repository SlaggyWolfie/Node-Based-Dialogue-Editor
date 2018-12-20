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
            get { return _blackboard ?? (_blackboard = new Blackboard()); }
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
            string label = "Global Variable Inventory";
            
            //Search for assets with this label
            string[] GUIDs = AssetDatabase.FindAssets("l:" + label);
            //string[] GUIDs = AssetDatabase.FindAssets("t:VariableInventory");

            //If any assets are found, take the first and load it as a Variable Inventory
            if (GUIDs.Length != 0)
                return AssetDatabase.LoadAssetAtPath<VariableInventory>(AssetDatabase.GUIDToAssetPath(GUIDs[0]));

            //If no assets are found, create one
            Debug.Log("Creating a new Global Variable Inventory.");
            VariableInventory globalVariableInventory = ScriptableObject.CreateInstance<VariableInventory>();
            globalVariableInventory.name = "Global Variable Inventory";
            globalVariableInventory.Location = VariableLocation.Global;
            //AssetDatabase.SetLabels(globalVariableInventory, new[] { label });
            AssetDatabase.CreateAsset(globalVariableInventory, string.Format("Assets/{0}.asset", globalVariableInventory.name));
            AssetDatabase.SetLabels(globalVariableInventory, new[] { label });

            return globalVariableInventory;
        }
    }
}
