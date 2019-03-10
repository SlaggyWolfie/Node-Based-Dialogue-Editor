using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WolfEditor.Utility;
using UnityEditor;
using UnityEngine;
using WolfEditor.Base;
using WolfEditor.Nodes.Base;

namespace WolfEditor.Nodes
{
    [Serializable]
    public class Connection : BaseScriptableObject, ISerializationCallbackReceiver
    {
        public static bool AttemptToConnect(Connection connection, InputPort input, OutputPort output)
        {
            bool success = CanBeConnected(input, output);
            if (success) Connect(connection, input, output);
            //Debug.Log("Connection success: " + success.ToString());
            return success;
        }

        public static bool CanBeConnected(InputPort input, OutputPort output)
        {
            return !output.IsConnected && input.Node != output.Node;
        }

        private static void Connect(Connection connection, InputPort input, OutputPort output)
        {
            output.Connection = connection;
            input.AddConnection(connection);

            connection._startNode = output.Node;
            connection._endNode = input.Node;
        }

        [SerializeField]
        private NodeGraph _graph = null;
        public NodeGraph Graph
        {
            get { return _graph; }
            set { _graph = value; }
        }

        [SerializeField]
        private List<Instruction> _modifiers = new List<Instruction>();

        [SerializeField]
        private OutputPort _start = null;
        public OutputPort Start
        {
            get { return _start; }
            set { _start = value; }
        }

        [SerializeField]
        private InputPort _end = null;
        public InputPort End
        {
            get { return _end; }
            set { _end = value; }
        }

        public void Traverse() { if (_modifiers.Count > 0) _modifiers.ForEach(m => m.Execute()); }

        public void Disconnect()
        {
            DisconnectStart();
            DisconnectEnd();
        }
        public void DisconnectPort(Port port)
        {
            if (port == Start) DisconnectStart();
            else if (port == End) DisconnectEnd();
        }
        public void DisconnectStart()
        {
            if (Start == null) return;
            Start.Connection = null;
            Start = null;
        }
        public void DisconnectEnd()
        {
            if (End == null) return;
            End.RemoveConnection(this);
            End = null;
        }
        public void DisconnectInput() { DisconnectEnd(); }
        public void DisconnectOutput() { DisconnectStart(); }

        public T CreateAndAddModifier<T>() where T : Instruction
        {
            return (T)CreateAndAddModifier(typeof(T));
        }
        public virtual Instruction CreateAndAddModifier(Type type)
        {
            Instruction mod = (Instruction)CreateInstance(type);
            InitConnectionModifier(mod);
            return mod;
        }
        public virtual void AddModifier(Instruction instruction)
        {
            if (instruction == null) return;
            _modifiers.Add(instruction);
            instruction.Connection = this;
        }
        public void RemoveModifier(Instruction mod) { _modifiers.Remove(mod); }
        public void RemoveModifier(int index) { _modifiers.RemoveAt(index); }
        public void ClearModifiers() { _modifiers.Clear(); }
        public int ModifierCount { get { return _modifiers.Count; } }
        public int GetIndex(Instruction mod) { return _modifiers.IndexOf(mod); }
        public Instruction GetModifier(int index) { return _modifiers[index]; }
        public Instruction[] GetModifiers() { return _modifiers.ToArray(); }

        [SerializeField, /*NaughtyAttributes.ReadOnly*//*, HideInInspector*/] private Node _endNode = null;
        [SerializeField, /*NaughtyAttributes.ReadOnly*//*, HideInInspector*/] private Node _startNode = null;
        [SerializeField, /*NaughtyAttributes.ReadOnly*//*, HideInInspector*/] private int _contingencyPlan = -1;

        public void OnDisable()
        {
            OnBeforeSerialize();
        }

#if UNITY_EDITOR
        private void DeserializeAgain()
        {
            EditorApplication.update -= DeserializeAgain;
            OnAfterDeserialize();
        }
#else
        private IEnumerator DeserializeAgain2()
        {
            yield return null;
            OnAfterDeserialize();
        }
#endif

        public void OnBeforeSerialize()
        {
            if (_endNode == null && End != null && End.Node != null) _endNode = End.Node;
            if (_startNode == null && Start != null && Start.Node != null) _startNode = Start.Node;

            if (_startNode == null)
            {
                //Debug.Log("Null shit node");
                return;
            }

            if (Start == null)
            {
                //Debug.Log("Null output port?");
                return;
            }

            IMultipleOutput mOutput = _startNode.PortHandler.multipleOutputNode;
            if (mOutput != null)
            {
                //OutputPort[] outputs = mOutput.GetOutputs().ToArray();
                //_contingencyPlan = Array.IndexOf(outputs, Start);
                //List<OutputPort> outputs = mOutput.GetOutputs().ToList();
                //_contingencyPlan = outputs.IndexOf(Start);

                List<int> outputs = ElementsOf(mOutput.GetOutputs(), outputPort => outputPort.ID).ToList();
                _contingencyPlan = outputs.IndexOf(Start.ID);

                //if (_contingencyPlan != -1)
                //Debug.Log(mOutput + " " + _contingencyPlan);
                //else Debug.LogError("WHY!?");
            }
        }

        private static IEnumerable<TOutput> ElementsOf<TInput, TOutput>(IEnumerable<TInput> source,
            Func<TInput, TOutput> getter)
        {
            TInput[] sourceArray = source as TInput[] ?? source.ToArray();
            TOutput[] collection = new TOutput[sourceArray.Length];

            for (int i = 0; i < sourceArray.Length; i++)
                collection[i] = getter(sourceArray[i]);

            return collection;
        }

        public void OnAfterDeserialize()
        {
            //End = _inputNode.PortHandler.inputNode.InputPort;
            //Start = _outputNode.PortHandler.outputNode.OutputPort;
            FixInput();
            FixOutput();
            //FixNodeInput(End);
            //FixNodeOutput(Start, _contingencyPlan);

            //_contingencyPlan = -1;
        }

        private void FixInput()
        {
            Node node = _endNode;
            if (node == null)
            {
                //Debug.LogWarning("Null node on attempt to fix input port.");
                return;
            }
            if (node.PortHandler.inputNode != null)
                End = node.PortHandler.inputNode.InputPort;
            //End.Node = node;
        }

        private void FixOutput()
        {
            Node node = _startNode;
            if (node == null)
            {
                //Debug.LogWarning("Null node on attempt to fix output port.");
                return;
            }
            if (node.PortHandler.outputNode != null)
            {
                Start = node.PortHandler.outputNode.OutputPort;
                //Start.Node = node;
            }
            if (node.PortHandler.multipleOutputNode != null)
            {
                var outputs = node.PortHandler.multipleOutputNode.GetOutputs();
                OutputPort[] outputsArray = outputs.ToArray();
                if (outputsArray.Length == 0)
                {
                    //Debug.Log("Very weird");
#if UNITY_EDITOR
                    EditorApplication.update += DeserializeAgain;
#else
                    DeserializeAgain2().RunCoroutine();
#endif
                }

                if (_contingencyPlan < 0 || _contingencyPlan >= outputsArray.Length) return;
                Start = outputsArray[_contingencyPlan];
                //Start.Node = node;
            }
        }

        public static void FixNodeInput(InputPort input)
        {
            Node node = input.Node;
            if (node == null)
            {
                Debug.LogWarning("Null node on attempt to fix input port.");
                return;
            }
            node.PortHandler.inputNode.InputPort = input;
        }
        public static void FixNodeOutput(OutputPort output, int contingencyPlan)
        {
            Node node = output.Node;
            if (node == null)
            {
                Debug.LogWarning("Null node on attempt to fix output port.");
                return;
            }

            if (node.PortHandler.outputNode != null)
            {
                node.PortHandler.outputNode.OutputPort = output;
            }
            else if (node.PortHandler.multipleOutputNode != null)
            {
                IMultipleOutput mOutput = node.PortHandler.multipleOutputNode;
                OutputPort[] outputs = mOutput.GetOutputs().ToArray();
                if (contingencyPlan >= 0 && contingencyPlan < outputs.Length)
                    mOutput.ReplacePort(outputs[contingencyPlan], output);
            }
        }

        public void InitConnectionModifier(Instruction instruction)
        {
            instruction.ID = Graph.connectionModifierCounter.Get();
            AddModifier(instruction);
        }

        private void OnDestroy()
        {
            //Debug.Log("YOLO!");
            //Do not trust Unity.
            Disconnect();
            foreach (Instruction connectionModifier in _modifiers)
                DestroyHelper.Destroy(connectionModifier);
            //DestroyImmediate(connectionModifier, true);
            _modifiers.Clear();
            Graph.RemoveConnection(this);
        }

        public void ClearPorts()
        {
            Start = null;
            End = null;
        }
    }
}
