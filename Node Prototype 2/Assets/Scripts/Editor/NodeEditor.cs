using System;
using UnityEditor;
using UnityEngine;

namespace RPG.Nodes.Editor
{
    [CustomNodeEditor(typeof(Node))]
    public abstract class NodeEditor : BaseEditor<NodeEditor, Node, CustomNodeEditorAttribute>
    {
        public static Action<Node> onUpdateNode;

        private enum RenamingState { Idle, StandBy, Renaming }
        private RenamingState _renaming = RenamingState.Idle;

        private Rect _rectangle;
        public Rect Rectangle
        {
            get { return _rectangle; }
            set { _rectangle = value; }
        }

        protected virtual void OnValidate() { }

        public virtual void OnHeaderGUI()
        {
            string title = Target.name;
            if (_renaming != RenamingState.Idle && Selection.Contains(Target)) RenamingGUI();
            else GUILayout.Label(title, GUILayout.Height(NodePreferences.HEADER_HEIGHT));
        }

        public virtual void OnBodyGUI()
        {
            DrawDefaultNodeEditorBody();
        }

        protected void RenamingGUI()
        {
            int controlID = EditorGUIUtility.GetControlID(FocusType.Keyboard) + 1;
            if (_renaming == RenamingState.StandBy)
            {
                EditorGUIUtility.keyboardControl = controlID;
                EditorGUIUtility.editingTextField = true;
                _renaming = RenamingState.Renaming;
            }

            Target.name = EditorGUILayout.TextField(Target.name, GUILayout.Height(NodePreferences.HEADER_HEIGHT));

            if (EditorGUIUtility.editingTextField) return;
            Rename(Target.name);
            _renaming = RenamingState.Idle;
        }

        protected void DrawDefaultNodeEditorBody()
        {
            SerializedObject.Update();

            bool enterChildren = true;
            for (SerializedProperty i = SerializedObject.GetIterator();
                i.NextVisible(enterChildren);
                enterChildren = false)
                EditorGUILayout.PropertyField(i, true);

            SerializedObject.ApplyModifiedProperties();
        }

        public void Rename(string newName)
        {
            Target.name = newName;
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Target));
        }

        public void InitiateRename()
        {
            _renaming = RenamingState.StandBy;
        }

        public abstract Rect GetPortRect(Port port);

        public static Rect FindPortRect(Port port)
        {
            return GetEditor(port.Node).GetPortRect(port);
        }

        public static void _UpdateNode(Node node)
        {
            GetEditor(node).OnValidate();
            if (onUpdateNode != null) onUpdateNode(node);
            node.onUpdate();
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CustomNodeEditorAttribute : Attribute, IEditorAttribute
    {
        private readonly Type _inspectedType;
        public CustomNodeEditorAttribute(Type inspectedType) { _inspectedType = inspectedType; }
        public Type GetInspectedType() { return _inspectedType; }
    }
}
