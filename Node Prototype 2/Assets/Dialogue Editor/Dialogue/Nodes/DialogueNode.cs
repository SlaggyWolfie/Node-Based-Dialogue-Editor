using System;
using WolfEditor.Nodes.Base;
using UnityEngine;
using WolfEditor.Nodes;
using WolfEditor.Other;

namespace WolfEditor.Dialogue
{
    [Serializable]
    //[CreateNodeMenu(preferredName = "Dialogue Node", menuName = "RPG Framework/Dialogue Graph")]
    public sealed class DialogueNode : Node, IInput, IOutput
    {
        [SerializeField] private InputPort _inputPort = null;
        [SerializeField] private OutputPort _outputPort = null;

        #region Node Stuff
        public InputPort InputPort
        {
            get { return this.DefaultGetInputPort(ref _inputPort); }
            set { this.ReplaceInputPort(ref _inputPort, value); }
        }
        public OutputPort OutputPort
        {
            get { return this.DefaultGetOutputPort(ref _outputPort); }
            set { this.ReplaceOutputPort(ref _outputPort, value); }
        }
        #endregion

        [SerializeField] private string _speaker = string.Empty;
        [SerializeField]
        [ResizableTextArea]
        private string _text = string.Empty;
        [SerializeField] private AudioClip _audio = null;
        [SerializeField] private float _duration = 0;

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public string Speaker
        {
            get { return _speaker; }
            set { _speaker = value; }
        }

        public AudioClip Audio
        {
            get { return _audio; }
            set { _audio = value; }
        }

        public float Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }
    }
}