using System;
using RPG.Other;
using RPG.Nodes;
using UnityEngine;

namespace RPG.Dialogue
{
    [Serializable]
    public sealed class DialogueNode : Node, IInput, IOutput
    {
        [SerializeField]
        private InputPort _inputPort = null;
        [SerializeField]
        private OutputPort _outputPort = null;

        #region Node Stuff
        public InputPort InputPort
        {
            get { return _inputPort ?? (_inputPort = new InputPort() {Node = this}); }
            set
            {
                _inputPort = value;
                _inputPort.Node = this;
            }
        }
        public OutputPort OutputPort
        {
            get { return _outputPort ?? (_outputPort = new OutputPort() {Node = this}); }
            set
            {
                _outputPort = value;
                _outputPort.Node = this;
            }
        }
        #endregion
        
        [SerializeField]
        private string _speaker = "Speaker...";
        [SerializeField]
        [ResizableTextArea]
        private string _text = "Text...";
        [SerializeField]
        private AudioClip _audio = null;
        [SerializeField]
        private float _duration = 0;

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