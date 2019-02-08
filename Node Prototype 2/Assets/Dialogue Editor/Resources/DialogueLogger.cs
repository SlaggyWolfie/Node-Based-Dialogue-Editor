using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Dialogue;
using RPG.Nodes;
using UnityEngine;

public class DialogueLogger : MonoBehaviour
{
    [SerializeField] private DialogueGraph _graph = null;

    private void Start() { if (_graph != null) _graph.Flow.SetStartNode(_graph.FindStartNode()); }
    private void Update()
    {
        if (_graph == null) return;
        if (Input.GetKeyDown(KeyCode.Space)) HandleNode(_graph.Flow.NextNode());
    }

    private static void HandleNode(Node node)
    {
        DialogueNode dialogueNode = node as DialogueNode;
        if (dialogueNode != null)
        {
            Debug.Log(string.Format("{0} says: {1}", dialogueNode.Speaker, dialogueNode.Text));
            return;
        }

        ChoiceNode choiceNode = node as ChoiceNode;
        if (choiceNode != null)
        {
            string output = "Available Options:";
            for (int i = 0; i < choiceNode.BranchCount; i++)
            {
                Branch branch = choiceNode.GetBranch(i);
                if (branch.IsAvailable)
                    output += string.Format("\nOption {0}: {1}", i + 1, branch.DialogueNode.Text);
            }

            Debug.Log(output);
        }
    }
}
