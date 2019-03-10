using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WolfEditor.Base;

namespace WolfEditor.Nodes
{
    [Serializable]
    public abstract class Node : BaseScriptableObject, ISerializationCallbackReceiver
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
        }

        public void OnEnable()
        {
            CallbackSetup();
            GetAllPorts().ForEach(p => p.OnEnable());
        }

        public void OnDisable()
        {
            CallbackKill();
            GetAllPorts().ForEach(p => p.OnDisable());
        }

        private void PortSetup()
        {
            GetAllPorts().ForEach(InitPort);
            //PortHandler.InputPortAction(input => input.InputPort.Node = this);
            //PortHandler.OutputPortAction(output => output.OutputPort.Node = this);
            //PortHandler.MultipleOutputPortAction(output => output.AssignNodesToOutputPorts(this));
        }

        public void InitPort(Port port)
        {
            port.Node = this;
            if (port.ID != -1) return;
            port.ID = Graph.portCounter.Get();
        }

        public List<Port> GetAllPorts()
        {
            List<Port> ports = new List<Port>();
            PortHandler.AttemptInputAction(input => ports.Add(input.InputPort));
            PortHandler.AttemptOutputAction(output => ports.Add(output.OutputPort));
            PortHandler.AttemptMultipleOutputAction(output => ports.AddRange(output.GetOutputs().ToArray()));
            return ports;
        }

        public void ClearConnections()
        {
            GetAllPorts().ForEach(p => p.ClearConnections());
        }

        public void ResetPorts()
        {
            _portHandler = new PortHandler(this);
            PortHandler.AttemptInputAction(input => input.InputPort = new InputPort());
            PortHandler.AttemptOutputAction(output => output.OutputPort = new OutputPort());
            PortHandler.AttemptMultipleOutputAction(output => output.ResetOutputPorts());
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

        //protected static Node CheckNextNodePort(OutputPort output)
        //{
        //    if (output != null &&
        //        output.Connection != null &&
        //        output.Connection.End != null)
        //        return output.Connection.End.Node;

        //    return null;
        //}

        protected static Node CheckNextNodePort(OutputPort output)
        {
            if (output != null &&
                output.Connection &&
                output.Connection.End != null)
                return output.Connection.End.Node;

            return null;
        }

        //Callbacks
        private void CallbackSetup()
        {
            CallbackKill();

            onEnter += OnEnter;
            onExit += OnExit;
            onUpdate += Update;

            onExit += PortConnectionTraversal;
        }

        private void CallbackKill()
        {
            onEnter = null;
            onExit = null;
            onUpdate = null;
        }

        public Action onUpdate;
        public Action onEnter;
        public Action onExit;

        public virtual void Update() { }
        public virtual void OnEnter() { }
        public virtual void OnExit() { }

        private void PortConnectionTraversal()
        {
            PortHandler.AttemptOutputAction(output => output.OutputPort.OnExit());
            PortHandler.AttemptMultipleOutputAction(output => output.GetExitPort().OnExit());
        }

        public virtual bool InputPortIsInHeader() { return true; }
        public virtual bool OutputPortIsInHeader() { return true; }

        public virtual void OnDestroy()
        {
            GetAllPorts().ForEach(port => port.OnDestroy());
        }

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() { PortSetup(); }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CreateNodeMenuAttribute : Attribute
    {
        public string menuName;
        public CreateNodeMenuAttribute(string menuName) { this.menuName = menuName; }
    }

    public interface IPort { }
    public interface IInput : IPort { InputPort InputPort { get; set; } }
    public interface IBaseOutput : IPort { }
    public interface IOutput : IBaseOutput { OutputPort OutputPort { get; set; } }
    public interface IMultipleOutput : IBaseOutput
    {
        IEnumerable<OutputPort> GetOutputs();
        //OutputPort[] GetOutputs();
        void ReplacePort(OutputPort oldPort, OutputPort newPort);
        void ResetOutputPorts();
        Node NextNode();
        OutputPort GetExitPort();
    }

    public static class IPortExtensions
    {
        public static InputPort DefaultGetInputPort(this IInput inputNode, ref InputPort inputPort)
        {
            return inputPort ?? (inputPort = new InputPort() { Node = inputNode as Node });
        }
        public static void ReplaceInputPort(this IInput inputNode, ref InputPort oldPort, InputPort value)
        {
            //if (oldPort != null)
            //{
            //    //oldPort.GetConnections().ForEach(c =>
            //    //{
            //    //    if (c != null) c.End = value;
            //    //});
            //    oldPort.Node = null;
            //    oldPort = null;
            //}

            oldPort = value;
            //if (oldPort != null) oldPort.Node = inputNode as Node;
        }
        public static OutputPort DefaultGetOutputPort(this IBaseOutput outputNode, ref OutputPort outputPort)
        {
            return outputPort ?? (outputPort = new OutputPort() { Node = outputNode as Node });
        }
        public static void ReplaceOutputPort(this IBaseOutput outputNode, ref OutputPort outputPort, OutputPort value)
        {
            //if (outputPort != null)
            //{
            //    //if (outputPort.Connection != null)
            //    //    outputPort.Connection.Start = value;
            //    outputPort.Node = null;
            //    outputPort = null;
            //}
            outputPort = value;
            //if (outputPort != null) outputPort.Node = outputNode as Node;
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

        public void AttemptInputAction(Action<IInput> action) { if (inputNode != null) action.Invoke(inputNode); }
        public void AttemptOutputAction(Action<IOutput> action) { if (outputNode != null) action.Invoke(outputNode); }
        public void AttemptMultipleOutputAction(Action<IMultipleOutput> action)
        {
            if (multipleOutputNode != null) action.Invoke(multipleOutputNode);
        }
    }
}
