using System.Collections;
using System.Collections.Generic;
using WolfEditor.Nodes;
using WolfEditor.Nodes.Base;
using UnityEngine;
using WolfEditor.Variables;

public class TestNode : Node, IInput, IOutput
{
    [SerializeField]
    private InputPort _inputPort = null;
    [SerializeField]
    private OutputPort _outputPort = null;

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

    public int @int;
    public float @float;
    public bool @bool;
    public Variable @var;
    public string @string;
}
