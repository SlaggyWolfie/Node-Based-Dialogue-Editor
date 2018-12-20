using System.Collections;
using System.Collections.Generic;
using RPG.Dialogue;
using RPG.Nodes;
using UnityEngine;

public class DialogueLogger : MonoBehaviour
{
    [SerializeField] private DialogueGraph _graph = null;

    private void Start()
    {
        _graph.Flow.SetStartNode(_graph.FindStartNode());
    }

    private void Update()
    {
        if (_graph == null) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleNode(_graph.Flow.NextNode());
        }
    }

    private void HandleNode(Node node)
    {
        if (node is DialogueNode)
        {
            DialogueNode dialogueNode = (DialogueNode)node;
            Debug.Log(string.Format("{0} says: {1}", dialogueNode.Speaker, dialogueNode.Text));
        }

    }
}
