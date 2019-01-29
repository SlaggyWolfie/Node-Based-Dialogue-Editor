using System;
using System.Collections.Generic;
using System.Linq;
using RPG.Base;
using RPG.Nodes.Base;
using UnityEngine;

namespace RPG.Nodes
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
        }

        [SerializeField]
        private NodeGraph _graph = null;
        public NodeGraph Graph
        {
            get { return _graph; }
            set { _graph = value; }
        }

        [SerializeField]
        private List<ConnectionModifier> _modifiers = new List<ConnectionModifier>();

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

        public T CreateAndAddModifier<T>() where T : ConnectionModifier
        {
            return (T)CreateAndAddModifier(typeof(T));
        }
        public virtual ConnectionModifier CreateAndAddModifier(Type type)
        {
            ConnectionModifier mod = (ConnectionModifier)CreateInstance(type);
            InitConnectionModifier(mod);
            return mod;
        }
        public virtual void AddModifier(ConnectionModifier connectionModifier)
        {
            if (connectionModifier == null) return;
            _modifiers.Add(connectionModifier);
            connectionModifier.Connection = this;
        }
        public void RemoveModifier(ConnectionModifier mod) { _modifiers.Remove(mod); }
        public void RemoveModifier(int index) { _modifiers.RemoveAt(index); }
        public void ClearModifiers() { _modifiers.Clear(); }
        public int ModifierCount { get { return _modifiers.Count; } }
        public int GetIndex(ConnectionModifier mod) { return _modifiers.IndexOf(mod); }
        public ConnectionModifier GetModifier(int index) { return _modifiers[index]; }
        public ConnectionModifier[] GetModifiers() { return _modifiers.ToArray(); }

        [SerializeField, HideInInspector] private Node _endNode = null;
        [SerializeField, HideInInspector] private Node _startNode = null;
        [SerializeField, HideInInspector] private int _contingencyPlan = -1;

        public void OnBeforeSerialize()
        {
            if (End != null) _endNode = End.Node;
            if (Start != null) _startNode = Start.Node;

            if (_startNode == null)
            {
                Debug.Log("Null shit node");
                return;
            }

            IMultipleOutput mOutput = _startNode.PortHandler.multipleOutputNode;
            if (mOutput != null)
            {
                OutputPort[] outputs = mOutput.GetOutputs().ToArray();
                _contingencyPlan = Array.IndexOf(outputs, Start);
                //List<OutputPort> outputs = mOutput.GetOutputs().ToList();
                //_contingencyPlan = outputs.IndexOf(Start);
                if (_contingencyPlan != -1)
                    Debug.Log(mOutput + " " + _contingencyPlan);
                else Debug.LogError("WHY!?");
            }
        }
        public void OnAfterDeserialize()
        {
            //End = _inputNode.PortHandler.inputNode.InputPort;
            //Start = _outputNode.PortHandler.outputNode.OutputPort;
            //Debug.Log("Test");

            FixInput();
            FixOutput();
            //FixNodeInput(End);
            //FixNodeOutput(Start, _contingencyPlan);

            _contingencyPlan = -1;
        }

        private void FixInput()
        {
            Node node = _endNode;
            if (node == null)
            {
                Debug.LogWarning("Null node on attempt to fix input port.");
                return;
            }
            End = node.PortHandler.inputNode.InputPort;
            //End.Node = node;
        }

        private void FixOutput()
        {
            Node node = _startNode;
            if (node == null)
            {
                Debug.LogWarning("Null node on attempt to fix output port.");
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

        public void InitConnectionModifier(ConnectionModifier connectionModifier)
        {
            connectionModifier.ID = Graph.connectionModifierCounter.Get();
            AddModifier(connectionModifier);
        }

        private void OnDestroy()
        {
            //Debug.Log("YOLO!");
            //Do not trust Unity.
            Disconnect();
            foreach (ConnectionModifier connectionModifier in _modifiers)
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
