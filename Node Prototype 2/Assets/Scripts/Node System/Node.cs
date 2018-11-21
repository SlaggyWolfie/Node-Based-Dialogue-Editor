using System;
using System.Collections.Generic;
using System.Security.Permissions;
using RPG.Nodes.Base;
using UnityEngine;

namespace RPG.Nodes
{
    [Serializable]
    public abstract class Node : ScriptableObjectWithID
    {
        private Vector2 _position = Vector2.zero;
        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        private NodeGraph _graph = null;
        public NodeGraph Graph
        {
            get { return _graph; }
            set { _graph = value; }
        }

        public void AssignNodesToPorts()
        {
            IInput inputNode = this as IInput;
            if (inputNode != null) inputNode.InputPort.Node = this;

            ISingleOutput sOutputNode = this as ISingleOutput;
            if (sOutputNode != null) sOutputNode.OutputPort.Node = this;

            IMultipleOutput mOutputNode = this as IMultipleOutput;
            if (mOutputNode != null) mOutputNode.AssignNodesToOutputPorts(this);
        }

        public List<Port> GetAllPorts()
        {
            List<Port> ports = new List<Port>();

            IInput inputNode = this as IInput;
            if (inputNode != null) ports.Add(inputNode.InputPort);

            ISingleOutput sOutputNode = this as ISingleOutput;
            if (sOutputNode != null) ports.Add(sOutputNode.OutputPort);

            IMultipleOutput mOutputNode = this as IMultipleOutput;
            if (mOutputNode != null) ports.AddRange((IEnumerable<Port>)mOutputNode.GetOutputs());

            return ports;
        }

        public void ClearConnections()
        {
            IInput inputNode = this as IInput;
            if (inputNode != null) inputNode.InputPort.ClearConnections();

            ISingleOutput sOutputNode = this as ISingleOutput;
            if (sOutputNode != null) sOutputNode.OutputPort.ClearConnections();

            IMultipleOutput mOutputNode = this as IMultipleOutput;
            if (mOutputNode != null) mOutputNode.ClearMultipleConnections();
        }

        public Node NextNode()
        {
            ISingleOutput sOutputNode = this as ISingleOutput;
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
            IInput inputNode = this as IInput;
            if (inputNode != null) inputNode.InputPort.Position += offset;

            ISingleOutput sOutputNode = this as ISingleOutput;
            if (sOutputNode != null) sOutputNode.OutputPort.Position += offset;

            IMultipleOutput mOutputNode = this as IMultipleOutput;
            if (mOutputNode != null) mOutputNode.OffsetMultiplePorts(offset);
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
    public interface IOutput : IPort { }
    public interface ISingleOutput : IOutput { OutputPort OutputPort { get; set; } }
    public interface IMultipleOutput : IOutput
    {
        void ClearMultipleConnections();
        List<OutputPort> GetOutputs();
        Node NextNode();
        void AssignNodesToOutputPorts(Node node);
        void OffsetMultiplePorts(Vector2 offset);
    }
}
