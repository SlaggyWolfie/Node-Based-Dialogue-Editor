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
            Debug.Log("Connection success: " + success.ToString());
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
        public virtual void AddModifier(ConnectionModifier node)
        {
            _modifiers.Add(node);
            node.Connection = this;
        }

        public void RemoveModifier(ConnectionModifier mod)
        {
            _modifiers.Remove(mod);
        }

        public void RemoveModifier(int index)
        {
            _modifiers.RemoveAt(index);
        }
        public void ClearModifiers() { _modifiers.Clear(); }
        public int ModifierCount { get { return _modifiers.Count; } }
        public ConnectionModifier GetModifier(int index)
        {
            if (index < 0 || index >= ModifierCount) return null;
            return _modifiers[index];
        }
        public int GetIndex(ConnectionModifier mod) { return _modifiers.IndexOf(mod); }
        public ConnectionModifier[] GetModifiers() { return _modifiers.ToArray(); }

        [SerializeField, HideInInspector]
        private bool _wasPreviouslySerialized = false;
        public void OnBeforeSerialize()
        {
            //Debug.Log("Oloy");
            _wasPreviouslySerialized = true;
            //Debug.Log("Ser");
        }
        public void OnAfterDeserialize()
        {
            //Debug.Log("Yolo");
            if (!_wasPreviouslySerialized) return;
            //Debug.Log("Yolo__2");
            FixOutput(Start);
            FixInput(End);
            _wasPreviouslySerialized = false;
        }

        private void FixInput(InputPort input)
        {
            Node node = input.Node;
            if (node == null) return;
            node.PortHandler.InputPortAction(inputNode => inputNode.InputPort = input);
        }
        private void FixOutput(OutputPort output)
        {
            Node node = output.Node;
            if (node == null) return;
            if (node is IOutput) node.PortHandler.OutputPortAction(outputNode => outputNode.OutputPort = output);
            else if (node is IMultipleOutput)
            {
                var outputs = node.PortHandler.multipleOutputNode.GetOutputs().ToArray();
                for (var i = 0; i < outputs.Length; i++)
                {
                    OutputPort testedOutput = outputs[i];
                    if (testedOutput.Connection != output.Connection) continue;
                    outputs[i] = output;
                    break;
                }
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
    }
}
