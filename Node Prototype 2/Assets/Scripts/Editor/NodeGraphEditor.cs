using System;
using System.Linq;
using RPG.Base;
using RPG.Nodes;
using RPG.Nodes.Base;
using UnityEditor;
using UnityEngine;

namespace RPG.Editor.Nodes
{
    [CustomNodeGraphEditor(typeof(NodeGraph))]
    public class NodeGraphEditor : BaseForCustomEditors<NodeGraphEditor, NodeGraph, CustomNodeGraphEditorAttribute>
    {
        private Rect _rect;
        public Rect Rect
        {
            get { return _rect; }
            set { _rect = value; }
        }

        public VariableInventory GetVariableInventory(string name, string labelIdentifier)
        {
            string graphPath = AssetDatabase.GetAssetPath(Target);
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
            AssetDatabase.AddObjectToAsset(newSubAsset, Target);
            var labels = AssetDatabase.GetLabels(newSubAsset).ToList();
            labels.Add(labelIdentifier);
            AssetDatabase.SetLabels(newSubAsset, labels.ToArray());
            return newSubAsset;

        }

        public Node CopyNode(Node original)
        {
            Node node = Target.CopyNode(original);
            node.name = original.name;
            AssetDatabase.AddObjectToAsset(node, Target);
            return node;
        }

        public void RemoveNode(Node node)
        {
            OnNodeRemoval(node);
            Target.RemoveNode(node);
            UnityEngine.Object.DestroyImmediate(node, true);

            //TODO: Expand Undo Functionality.
            //Undo.DestroyObjectImmediate(node);
        }
        public void RemoveConnection(Connection connection)
        {
            Target.RemoveConnection(connection);
            UnityEngine.Object.DestroyImmediate(connection, true);
        }

        protected void OnNodeRemoval(Node node)
        {
            node.PortHandler.InputPortAction(input =>
            {
                var inputConnections = input.InputPort.GetConnections();
                for (int i = inputConnections.Count - 1; i >= 0; i--)
                {
                    Connection connection = inputConnections[i];
                    if (connection != null) RemoveConnection(connection);
                }
            });

            node.PortHandler.OutputPortAction(output =>
            {
                var outputConnection = output.OutputPort.Connection;
                if (outputConnection != null) RemoveConnection(outputConnection);
            });

            node.PortHandler.MultipleOutputPortAction(output =>
            {
                var outputs = output.GetOutputs();
                foreach (OutputPort outputPort in outputs)
                {
                    var outputConnection = outputPort.Connection;
                    if (outputConnection != null) RemoveConnection(outputConnection);
                }
            });
        }

        protected override void OnEnable()
        {
            string name = "Local Variable Inventory";
            Target._SetLocalVariableInventory(GetVariableInventory(name, name));
        }

        public virtual void OnGUI() { }

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
