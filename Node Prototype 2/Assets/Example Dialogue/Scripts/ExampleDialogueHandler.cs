using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text.RegularExpressions;
using WolfEditor.Base;
using UnityEngine;
using UnityEngine.UI;
using WolfEditor.Dialogue;
using WolfEditor.Nodes;
using WolfEditor.Variables;

namespace WolfEditor.Example
{
    public class ExampleDialogueHandler : MonoBehaviour
    {
        public DialogueGraph DialogueGraph
        {
            get { return _dialogueGraph; }
            set { _dialogueGraph = value; }
        }

        private DialogueNode _previousDialogueNode = null;
        
        [Header("Introspection. Do not edit!")]
        [SerializeField] private DialogueGraph _dialogueGraph = null;
        [SerializeField] private bool _waiting = true;
        [SerializeField] private bool _paused = false;
        [SerializeField] private bool _started = false;
        [SerializeField] private Node _current = null;
        [SerializeField] private ChoiceNode _currentChoiceNode = null;

        [Header("Display")]
        [SerializeField] private GameObject _normalSubtitlesObject = null;
        [SerializeField] private Text _normalSubtitleText = null;
        [SerializeField] private GameObject _choicesObject = null;
        [SerializeField] private Transform _choiceContent = null;
        [SerializeField] private Text _choiceSubtitleText = null;

        [Header("Resources")]
        [SerializeField] private AudioSource _audioSource = null;
        [SerializeField] private GameObject _choiceButtonPrefab = null;

        public static Action OnDialogueStart;
        public static Action OnDialogueEnd;

        private void Start()
        {
            Debug.Assert(_normalSubtitlesObject != null);
            Debug.Assert(_normalSubtitleText != null);
            Debug.Assert(_choicesObject != null);
            Debug.Assert(_choiceContent != null);
            Debug.Assert(_choiceSubtitleText != null);

            Debug.Assert(_audioSource != null);
            Debug.Assert(_choiceButtonPrefab != null);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P)) _paused = !_paused;
            if (_paused) return;

            //if (Input.GetKeyDown(KeyCode.N) && _current is DialogueNode)
            if (Input.GetKeyDown(KeyCode.Space) && _current is DialogueNode)
            {
                if (_audioSource != null) _audioSource.Stop();
                StopAllCoroutines();
                HandleNextNode();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_audioSource != null) _audioSource.Stop();
                StopAllCoroutines();
                HandleEndNode();
            }

            if (_currentChoiceNode != null) CheckChoiceKeyInput();

            if (!_waiting) HandleNextNode();
        }

        public void StartDialogue()
        {
            if (_dialogueGraph == null || _started) return;
            _dialogueGraph.Flow.SetStartNode(_dialogueGraph.FindStartNode());
            _waiting = false;
            _started = true;

            if (OnDialogueStart != null) OnDialogueStart();
        }

        private void HandleNextNode()
        {
            if (_dialogueGraph == null) return;
            //Node current = _dialogueGraph.Flow.CurrentNode;
            _current = _dialogueGraph.Flow.NextNode();

            //Ugly
            DialogueNode dialogueNode = _current as DialogueNode;
            if (dialogueNode != null)
            {
                HandleDialogueNode(dialogueNode);
                return;
            }

            ConditionNode conditionNode = _current as ConditionNode;
            if (conditionNode != null)
            {
                _waiting = false;
                return;
            }

            ChoiceNode choiceNode = _current as ChoiceNode;
            if (choiceNode != null)
            {
                HandleChoiceNode(choiceNode);
                return;
            }

            EndNode endNode = _current as EndNode;
            if (endNode != null)
            {
                HandleEndNode(endNode);
                return;
            }

            //StartNode startNode = current as StartNode;
            //if (startNode != null)
            //{
            //    throw new InvalidOperationException();
            //}
        }

        private void HandleEndNode(EndNode endNode = null)
        {
            DestroyBranchObjects();
            _choicesObject.SetActive(false);
            _choiceSubtitleText.gameObject.SetActive(false);
            _normalSubtitlesObject.SetActive(false);
            _waiting = true;
            _started = false;
            _paused = false;

            if (OnDialogueEnd != null) OnDialogueEnd();
        }

        private IEnumerator Wait(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            _waiting = false;
        }

        private void HandleDialogueNode(DialogueNode dialogueNode)
        {
            if (dialogueNode == null) return;

            _waiting = true;

            //_normalSubtitleSpeaker.text = dialogueNode.Speaker;

            string dialogue = dialogueNode.Text;
            if (dialogue.StartsWith("["))
            {
                string[] groups = dialogue.Split('[', ']', '\n', '\t');
                if (groups.Length >= 4)
                {
                    for (int i = 2; i < groups.Length; i++)
                    {
                        if (groups[i].Length <= 2) continue;
                        dialogue = groups[i];
                        break;
                    }
                    //dialogue = groups[3];
                }
            }

            _normalSubtitleText.text = string.Format("{0}: {1}", dialogueNode.Speaker, dialogue);

            _normalSubtitlesObject.SetActive(true);
            _choicesObject.SetActive(false);
            _choiceSubtitleText.gameObject.SetActive(false);

            HandleAudio(dialogueNode.Audio);
            StopAllCoroutines();

            float duration = dialogueNode.Audio ? dialogueNode.Audio.length + 0.1f : dialogueNode.Duration;
            if (duration < 0.1f) duration = 2;
            StartCoroutine(Wait(duration));

            _previousDialogueNode = dialogueNode;
        }

        private void HandleChoiceNode(ChoiceNode choiceNode)
        {
            if (choiceNode == null) return;

            _currentChoiceNode = choiceNode;

            _waiting = true;

            _normalSubtitlesObject.SetActive(false);
            _choicesObject.SetActive(true);
            _choiceSubtitleText.gameObject.SetActive(true);

            var branches = choiceNode.GetAvailableBranches();

            for (int i = 0; i < branches.Count; i++)
            {
                Branch branch = branches[i];
                GameObject choiceButton = Instantiate(_choiceButtonPrefab, _choiceContent);

                //Assign text to the button that can be clicked in the format of
                // "1. Option to pick"
                Text text = choiceButton.GetComponentInChildren<Text>();
                if (text != null)
                {
                    //Regex Magic - stolen from StackOverflow
                    //string choiceText = Regex.Match(_previousDialogueNode.Text, @"\[([^]]*)\]").Groups[1].Value;

                    string choiceText = branch.DialogueNode.Text;
                    if (choiceText.StartsWith("["))
                    {
                        string[] groups = choiceText.Split('[', ']', '\n', '\t');
                        if (groups.Length >= 4) choiceText = groups[1];
                    }
                    text.text = string.Format("{0}. {1}", i + 1, choiceText);
                }

                //Assign click command
                Button button = choiceButton.GetComponentInChildren<Button>();
                if (button == null) continue;

                int capturedIndex = i;
                button.onClick.AddListener(() => { PickBranch(choiceNode, capturedIndex); });
            }

            if (_previousDialogueNode != null) _choiceSubtitleText.text = _previousDialogueNode.Text;
        }

        //private static readonly KeyCode[] _numberedKeys =
        //{
        //    KeyCode.Alpha1,
        //    KeyCode.Alpha2,
        //    KeyCode.Alpha3,
        //    KeyCode.Alpha4,
        //    KeyCode.Alpha5,
        //    KeyCode.Alpha6,
        //    KeyCode.Alpha7,
        //    KeyCode.Alpha8,
        //    KeyCode.Alpha9
        //};

        //private static readonly string[] _numberedStringKeys =
        //{
        //    "1","2","3","4","5","6","7","8","9"
        //};

        private void CheckChoiceKeyInput()
        {
            //Debug.Log(Input.inputString);

            int keyParse;
            if (int.TryParse(Input.inputString, out keyParse)) PickBranch(keyParse - 1);

            //Probably too slow. RIP
            //int keyInput = Array.IndexOf(_numberedStringKeys, Input.inputString);

            //Event.current is always null!? RIP
            //int keyInput = Array.IndexOf(_numberedKeys, current.keyCode);

            //if (keyInput != -1) PickBranch(keyInput);
        }

        private void PickBranch(int index)
        {
            if (index < 0 || index >= _currentChoiceNode.AvailableBranchCount) return;
            PickBranch(_currentChoiceNode, index);
            _currentChoiceNode = null;
        }

        private void PickBranch(ChoiceNode choiceNode, int index)
        {
            choiceNode.PickBranch(choiceNode.GetAvailableBranches()[index]);
            DestroyBranchObjects();
            _waiting = false;
        }

        private void DestroyBranchObjects()
        {
            foreach (Transform child in _choiceContent.GetComponentsInChildren<Transform>())
            {
                if (child == _choiceContent) continue;
                Destroy(child.gameObject);
            }
        }

        private void HandleAudio(AudioClip clip)
        {
            if (clip == null || _audioSource == null) return;
            _audioSource.PlayOneShot(clip);
        }
    }
}
