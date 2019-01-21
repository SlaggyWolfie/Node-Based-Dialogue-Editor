using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using RPG.Base;
using RPG.Nodes.Base;
using RPG.Other;
using UnityEngine;

namespace RPG.Nodes
{
    [Serializable]
    public abstract class Node : BaseScriptableObject
    {
        private PortHandler _portHandler = null;
        public PortHandler PortHandler { get { return _portHandler ?? (_portHandler = new PortHandler(this)); } }

        [SerializeField, HideInInspector]
        private Vector2 _position = Vector2.zero;
        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        [SerializeField, HideInInspector]
        private NodeGraph _graph = null;
        public NodeGraph Graph
        {
            get { return _graph; }
            set { _graph = value; }
        }

        public void Init()
        {
            PortSetup();
            CallbackSetup();
        }

        private void PortSetup()
        {
            GetAllPorts().ForEach(port =>
            {
                port.Node = this;
                port.ID = Graph.portCounter.Get();
            });
            //PortHandler.InputPortAction(input => input.InputPort.Node = this);
            //PortHandler.OutputPortAction(output => output.OutputPort.Node = this);
            //PortHandler.MultipleOutputPortAction(output => output.AssignNodesToOutputPorts(this));
        }

        public List<Port> GetAllPorts()
        {
            List<Port> ports = new List<Port>();
            PortHandler.InputPortAction(input => ports.Add(input.InputPort));
            PortHandler.OutputPortAction(output => ports.Add(output.OutputPort));
            PortHandler.MultipleOutputPortAction(output => ports.AddRange(output.GetOutputs().ToArray()));
            return ports;
        }

        public void ClearConnections()
        {
            GetAllPorts().ForEach(p => p.ClearConnections());
        }

        public void KillPorts()
        {
            PortHandler.InputPortAction(input => input.InputPort = null);
            PortHandler.OutputPortAction(output => output.OutputPort = null);
            PortHandler.MultipleOutputPortAction(output => output.KillOutputPorts());
        }

        public void Disconnect() { GetAllPorts().ForEach(port => port.Disconnect()); }

        public virtual Node NextNode()
        {
            IOutput sOutputNode = this as IOutput;
            if (sOutputNode != null) return CheckNextNodePort(sOutputNode.OutputPort);

            IMultipleOutput mOutputNode = this as IMultipleOutput;
            if (mOutputNode != null) return mOutputNode.NextNode();

            return null;
        }

        protected static Node CheckNextNodePort(OutputPort output)
        {
            if (output != null &&
                output.Connection != null &&
                output.Connection.End != null)
                return output.Connection.End.Node;

            return null;
        }

        //Callbacks
        private void CallbackSetup()
        {
            onEnter = null;
            onExit = null;
            onUpdate = null;

            onEnter += OnEnter;
            onExit += OnExit;
            onUpdate += Update;
        }

        public Action onUpdate;
        public Action onEnter;
        public Action onExit;

        public virtual void Update() { }
        public virtual void OnEnter() { }
        public virtual void OnExit() { }

        //public void OffsetPorts(Vector2 offset)
        //{
        //    //GetAllPorts().ForEach(port => port.Position += offset);

        //    //IInput inputNode = this as IInput;
        //    //if (inputNode != null) inputNode.InputPort.Position += offset;

        //    //ISingleOutput sOutputNode = this as ISingleOutput;
        //    //if (sOutputNode != null) sOutputNode.OutputPort.Position += offset;

        //    //IMultipleOutput mOutputNode = this as IMultipleOutput;
        //    //if (mOutputNode != null) mOutputNode.OffsetMultiplePorts(offset);
        //}

        public virtual bool InputPortIsInHeader() { return true; }
        public virtual bool OutputPortIsInHeader() { return true; }

        public virtual void OnDestroy()
        {
            //Debug.Log("You only live once.");
            GetAllPorts().ForEach(port => port.OnDestroy());

            //Do not trust Unity.

            //PortHandler.InputPortAction(input =>
            //{
            //    var inputConnections = input.InputPort.GetConnections();
            //    foreach (var connection in inputConnections)
            //    {
            //        DestroyImmediate(connection, true);
            //    }
            //});

            //PortHandler.OutputPortAction(output =>
            //{
            //    DestroyImmediate(output.OutputPort.Connection, true);
            //});

            //PortHandler.MultipleOutputPortAction(output =>
            //{
            //    var outputs = output.GetOutputs();
            //    foreach (OutputPort outputPort in outputs)
            //        DestroyImmediate(outputPort.Connection, true);
            //});
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CreateNodeMenuAttribute : Attribute
    {
        public string menuName;
        public CreateNodeMenuAttribute(string menuName) { this.menuName = menuName; }
    }

    public interface IPort { }
    public interface IInput : IPort { InputPort InputPort { get; set; } }
    public interface IOutput : IPort { OutputPort OutputPort { get; set; } }
    public interface IMultipleOutput : IPort
    {
        IEnumerable<OutputPort> GetOutputs();
        void KillOutputPorts();
        Node NextNode();
    }

    public static class IPortExtensions
    {
        public static InputPort DefaultGetInputPort(this IInput inputNode, ref InputPort inputPort)
        {
            return inputPort ?? (inputPort = new InputPort() { Node = inputNode as Node });
        }
        public static void ReplaceInputPort(this IInput inputNode, ref InputPort oldPort, InputPort value)
        {
            if (oldPort != null)
            {
                oldPort.GetConnections().ForEach(c =>
                {
                    if (c != null) c.End = value;
                });
                oldPort.Node = null;
                oldPort = null;
            }

            oldPort = value;
            if (oldPort != null) oldPort.Node = inputNode as Node;
        }
        public static OutputPort DefaultGetOutputPort(this IOutput outputNode, ref OutputPort outputPort)
        {
            return outputPort ?? (outputPort = new OutputPort() { Node = outputNode as Node });
        }
        public static void ReplaceOutputPort(this IOutput outputNode, ref OutputPort outputPort, OutputPort value)
        {
            if (outputPort != null)
            {
                if (outputPort.Connection != null)
                    outputPort.Connection.Start = value;
                outputPort.Node = null;
                outputPort = null;
            }
            outputPort = value;
            if (outputPort != null) outputPort.Node = outputNode as Node;
        }
    }

    public class PortHandler
    {
        //private Node _node = null;
        public IInput inputNode = null;
        public IOutput outputNode = null;
        public IMultipleOutput multipleOutputNode = null;

        public PortHandler(Node node)
        {
            inputNode = node as IInput;
            outputNode = node as IOutput;
            multipleOutputNode = node as IMultipleOutput;
        }

        public void PortAction<T>(Action<T> action)
            where T : IPort
        {
            if (typeof(T) == typeof(IInput) && inputNode != null) action.Invoke((T)inputNode);
            if (typeof(T) == typeof(IOutput) && outputNode != null) action.Invoke((T)outputNode);
            if (typeof(T) == typeof(IMultipleOutput) && multipleOutputNode != null) action.Invoke((T)multipleOutputNode);
        }

        public void InputPortAction(Action<IInput> action) { if (inputNode != null) action.Invoke(inputNode); }
        public void OutputPortAction(Action<IOutput> action) { if (outputNode != null) action.Invoke(outputNode); }
        public void MultipleOutputPortAction(Action<IMultipleOutput> action)
        {
            if (multipleOutputNode != null) action.Invoke(multipleOutputNode);
        }
    }
}
