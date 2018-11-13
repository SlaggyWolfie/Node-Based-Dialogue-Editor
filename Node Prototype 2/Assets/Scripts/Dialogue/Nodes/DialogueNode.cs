using UnityEngine;

namespace RPG.Nodes
{
    public sealed class DialogueNode : Node, IInput, ISingleOutput
    {
        private InputPort _inputPort = null;
        private OutputPort _outputPort = null;

        #region Node Stuff
        public InputPort InputPort
        {
            get
            {
                if (_inputPort == null)
                    _inputPort = new InputPort() { Node = this };
                return _inputPort;
            }
            set
            {
                _inputPort = value;
                _inputPort.Node = this;
            }
        }
        public OutputPort OutputPort
        {
            get
            {
                if (_outputPort == null)
                    _outputPort = new OutputPort() { Node = this };
                return _outputPort;
            }
            set
            {
                _outputPort = value;
                _outputPort.Node = this;
            }
        }
        #endregion

        //extra
        private string _text = "default text";
        private string _speaker = "default speaker name";
        private AudioClip _audio = null;
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