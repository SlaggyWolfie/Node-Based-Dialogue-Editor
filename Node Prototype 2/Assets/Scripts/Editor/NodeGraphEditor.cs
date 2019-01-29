using System;
using System.Linq;
using RPG.Base;
using RPG.Nodes;
using RPG.Nodes.Base;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPG.Editor.Nodes
{
    [CustomNodeGraphEditor(typeof(NodeGraph))]
    public class NodeGraphEditor : BaseForCustomEditors<NodeGraphEditor, NodeGraph, CustomNodeGraphEditorAttribute>
    {
        public static VariableInventory GetVariableInventory(ScriptableObject target, string name, string labelIdentifier)
        {
            string graphPath = AssetDatabase.GetAssetPath(target);
            //Debug.Log("Graph's Path: " + graphPath);

            string[] GUIDs = AssetDatabase.FindAssets(string.Format("l:{0}", labelIdentifier));
            foreach (string GUID in GUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(GUID);
                //Debug.Log("Checking path for Holder: " + path);
                if (path != graphPath) continue;
                VariableInventory subAsset = AssetDatabase.LoadAssetAtPath<VariableInventory>(path);
                if (subAsset == null || !AssetDatabase.IsSubAsset(subAsset)) continue;
                return subAsset;
            }

            VariableInventory newSubAsset = ScriptableObject.CreateInstance<VariableInventory>();
            newSubAsset.name = name;
            AssetDatabase.AddObjectToAsset(newSubAsset, target);
            var labels = AssetDatabase.GetLabels(newSubAsset).ToList();
            labels.Add(labelIdentifier);
            AssetDatabase.SetLabels(newSubAsset, labels.ToArray());
            return newSubAsset;

        }

        public Rect Rect { get; set; }

        protected override void Awake()
        {
            SetupVariableInventory();
            Target._missingVariableInventory = SetupVariableInventory;
        }
        public virtual void OnGUI() { }

        protected void OnNodeRemoval(Node node)
        {
            //Debug.Log("On Node Removal");
            node.PortHandler.AttemptInputAction(input =>
            {
                //Debug.Log("On Node Removal Input");
                var inputConnections = input.InputPort.GetConnections();
                for (int i = inputConnections.Count - 1; i >= 0; i--)
                {
                    //Debug.Log(i);
                    Connection connection = inputConnections[i];
                    //if (connection != null)
                    RemoveConnection(connection);
                }
            });

            node.PortHandler.AttemptOutputAction(output =>
            {
                //Debug.Log("On Node Removal Output");
                var outputConnection = output.OutputPort.Connection;
                //if (outputConnection != null)
                RemoveConnection(outputConnection);
            });

            node.PortHandler.AttemptMultipleOutputAction(output =>
            {
                var outputs = output.GetOutputs();
                foreach (OutputPort outputPort in outputs)
                {
                    var outputConnection = outputPort.Connection;
                    //if (outputConnection != null)
                    RemoveConnection(outputConnection);
                }
            });
        }

        protected void OnConnectionRemoval(Connection connection)
        {
            if (connection == null) return;
            foreach (ConnectionModifier connectionModifier in connection.GetModifiers())
                if (connectionModifier != null) RemoveConnectionModifier(connectionModifier);
        }

        public void RemoveNode(Node node)
        {
            //Do not trust Unity.
            OnNodeRemoval(node);
            Target.RemoveNode(node);
            DestroyHelper.Destroy(node);
            //Object.DestroyImmediate(node, true);

            //TODO: Expand Undo Functionality.
            //Undo.DestroyObjectImmediate(node);
        }
        public void RemoveConnection(Connection connection)
        {
            OnConnectionRemoval(connection);
            Target.RemoveConnection(connection);
            DestroyHelper.Destroy(connection);
            //Object.DestroyImmediate(connection, true);
        }
        public void RemoveConnectionModifier(ConnectionModifier connectionModifier)
        {
            connectionModifier.Connection.RemoveModifier(connectionModifier);
            DestroyHelper.Destroy(connectionModifier);
            //Object.DestroyImmediate(connectionModifier, true);
        }

        private void SetupVariableInventory()
        {
            string name = "Local Variable Inventory";
            Target._SetLocalVariableInventory(GetVariableInventory(Target, name, name));
        }

        public virtual string GetNodeMenuName(Type type)
        {
            CreateNodeMenuAttribute attribute;
            return ReflectionUtilities.GetAttribute(type, out attribute) ? attribute.menuName :
                UnityEditor.ObjectNames.NicifyVariableName(type.ToString().Replace('.', '/'));
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CustomNodeGraphEditorAttribute : Attribute, IEditorAttribute
    {
        private readonly Type _inspectedType;
        public CustomNodeGraphEditorAttribute(Type inspectedType) { _inspectedType = inspectedType; }
        public Type GetInspectedType() { return _inspectedType; }
    }
}
