using RPG.Dialogue;
using RPG.Nodes.Editor;

[CustomNodeGraphEditor(typeof(DialogueGraph))]
public class DialogueGraphEditor : NodeGraphEditor
{
    ///// <summary> 
    ///// Overriding GetNodeMenuName lets you control if and how nodes are categorized.
    ///// In this example we are sorting out all node types that are not in the XNode.Examples namespace.
    ///// </summary>
    //public override string GetNodeMenuName(System.Type type)
    //{
    //    if (type.Namespace == "XNode.Examples.MathNodes")
    //    {
    //        return base.GetNodeMenuName(type).Replace("X Node/Examples/Math Nodes/", "");
    //    }
    //    else return null;
    //}
}