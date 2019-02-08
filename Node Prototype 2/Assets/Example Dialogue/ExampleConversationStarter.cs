using System.Collections;
using System.Collections.Generic;
using RPG.Dialogue;
using UnityEngine;

namespace RPG.Example
{
    public class ExampleConversationStarter : MonoBehaviour
    {
        [SerializeField] private DialogueGraph _conversation = null;
        [SerializeField] private ExampleDialogueHandler _handler = null;

        public void StartConversation()
        {
            if (_conversation == null || _handler == null) return;
            _handler.DialogueGraph = _conversation;
            _handler.StartDialogue();
        }
    }
}

