using System;
using System.Collections.Generic;
using System.Security.Permissions;
using RPG.Base;
using RPG.Nodes.Base;
using UnityEngine;

namespace RPG.Nodes
{
    public class PortHandler
    {
        //private Node _node = null;
        public IInput inputNode = null;
        public IOutput outputNode = null;
        public IMultipleOutput multipleOutputNode = null;

        public PortHandler(Node node)
        {
            //_node = node;
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

        //public void InputPortAction(Action action)
        //{
        //    if (inputNode != null) action.Invoke();
        //}

        //public void SingeOutputPortAction(Action action)
        //{
        //    if (outputNode != null) action.Invoke();
        //}

        //public void MultipleOutputPortAction(Action action)
        //{
        //    if (multipleOutputNode != null) action.Invoke();
        //}

        public void InputPortAction(Action<IInput> action)
        {
            if (inputNode != null) action.Invoke(inputNode);
        }

        public void SingeOutputPortAction(Action<IOutput> action)
        {
            if (outputNode != null) action.Invoke(outputNode);
        }

        public void MultipleOutputPortAction(Action<IMultipleOutput> action)
        {
            if (multipleOutputNode != null) action.Invoke(multipleOutputNode);
        }
    }

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

        public void AssignNodesToPorts()
        {
            PortHandler.InputPortAction(input => input.InputPort.Node = this); 
            PortHandler.SingeOutputPortAction(output => output.OutputPort.Node = this);
            PortHandler.MultipleOutputPortAction(output => output.AssignNodesToOutputPorts(this));

            //PortHandler.PortAction<IInput>(input => input.InputPort.Node = this);

            //IInput inputNode = this as IInput;
            //if (inputNode != null) inputNode.InputPort.Node = this;

            //ISingleOutput sOutputNode = this as ISingleOutput;
            //if (sOutputNode != null) sOutputNode.OutputPort.Node = this;

            //IMultipleOutput mOutputNode = this as IMultipleOutput;
            //if (mOutputNode != null) mOutputNode.AssignNodesToOutputPorts(this);
        }

        public List<Port> GetAllPorts()
        {
            List<Port> ports = new List<Port>();
            PortHandler.InputPortAction(input => ports.Add(input.InputPort));
            PortHandler.SingeOutputPortAction(output => ports.Add(output.OutputPort));
            PortHandler.MultipleOutputPortAction(output => ports.AddRange(output.GetOutputs().ToArray()));

            //IInput inputNode = this as IInput;
            //if (inputNode != null) ports.Add(inputNode.InputPort);

            //ISingleOutput sOutputNode = this as ISingleOutput;
            //if (sOutputNode != null) ports.Add(sOutputNode.OutputPort);

            //IMultipleOutput mOutputNode = this as IMultipleOutput;
            //if (mOutputNode != null) ports.AddRange((IEnumerable<Port>)mOutputNode.GetOutputs());

            return ports;
        }

        public void ClearConnections()
        {
            GetAllPorts().ForEach(port => port.ClearConnections());

            //IInput inputNode = this as IInput;
            //if (inputNode != null) inputNode.InputPort.ClearConnections();

            //ISingleOutput sOutputNode = this as ISingleOutput;
            //if (sOutputNode != null) sOutputNode.OutputPort.ClearConnections();

            //IMultipleOutput mOutputNode = this as IMultipleOutput;
            //if (mOutputNode != null) mOutputNode.ClearMultipleConnections();
        }

        public Node NextNode()
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
        public void SetupCallbacks()
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

        public void OffsetPorts(Vector2 offset)
        {
            //GetAllPorts().ForEach(port => port.Position += offset);

            //IInput inputNode = this as IInput;
            //if (inputNode != null) inputNode.InputPort.Position += offset;

            //ISingleOutput sOutputNode = this as ISingleOutput;
            //if (sOutputNode != null) sOutputNode.OutputPort.Position += offset;

            //IMultipleOutput mOutputNode = this as IMultipleOutput;
            //if (mOutputNode != null) mOutputNode.OffsetMultiplePorts(offset);
        }

        public virtual bool InputPortIsInHeader()
        {
            return true;
        }

        public virtual bool OutputPortIsInHeader()
        {
            return true;
        }

        public virtual void OnDestroy()
        {
            GetAllPorts().ForEach(port => port.OnDestroy());
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
        void ClearOutputs();
        List<OutputPort> GetOutputs();
        Node NextNode();
        void AssignNodesToOutputPorts(Node node);
        void OffsetMultiplePorts(Vector2 offset);
    }
}
