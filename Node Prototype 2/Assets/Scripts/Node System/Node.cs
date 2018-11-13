using System;
using System.Collections.Generic;
using System.Security.Permissions;
using RPG.Nodes.Base;
using UnityEngine;

namespace RPG.Nodes
{
    public abstract class Node : DataObject
    {
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

            //IMultipleOutput mOutputNode = this as IMultipleOutput;
            //if (mOutputNode != null)
            //{
            //    for (int i = 0; i < mOutputNode.OutputCount; i++)
            //        ports.Add(mOutputNode.GetOutput(i));
            //}

            //PortAction<IInput>(n => ports.Add(n.InputPort));
            //PortAction<ISingleOutput>(n => ports.Add(n.OutputPort));
            //PortAction<ISpecialOutput>(n => ports.AddRange((IEnumerable<Port>)n.GetOutputs()));
            //PortAction<IMultipleOutput>(n => 
            //{
            //    for (int i = 0; i < n.OutputCount; i++)
            //        ports.Add(n.GetOutput(i));
            //});

            return ports;
        }

        //private static void PortAction<TPort>(Node node, Action<TPort> action)
        //    where TPort : IPort
        //{
        //    if (node is TPort) { }
        //    TPort portNode = (TPort)node;
        //    TPort portNode2 = node as TPort;
        //    if (portNode != null) action.Invoke(portNode);
        //}

        //private void PortAction<TPort>(Action<TPort> action)
        //    where TPort : IPort
        //{
        //    PortAction(this, action);
        //}

        public void ClearConnections()
        { 
            IInput inputNode = this as IInput;
            if (inputNode != null) inputNode.InputPort.ClearConnections();

            ISingleOutput sOutputNode = this as ISingleOutput;
            if (sOutputNode != null) sOutputNode.OutputPort.ClearConnections();

            IMultipleOutput mOutputNode = this as IMultipleOutput;
            if (mOutputNode != null) mOutputNode.ClearConnections();

            //IMultipleOutput mOutputNode = this as IMultipleOutput;
            //if (mOutputNode != null)
            //{
            //    for (int i = mOutputNode.OutputCount - 1; i >= 0; i--)
            //        Destroy(mOutputNode.GetOutput(i));

            //    mOutputNode.ClearOutput();
            //}
        }

        public Node NextNode()
        {
            ISingleOutput sOutputNode = this as ISingleOutput;
            if (sOutputNode != null) return CheckNextNodePort(sOutputNode.OutputPort);
            
            IMultipleOutput mOutputNode = this as IMultipleOutput;
            if (mOutputNode != null) return mOutputNode.NextNode();
            
            return null;
        }

        private Node CheckNextNodePort(OutputPort output)
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
    }

    public interface IPort { }
    public interface IInput : IPort { InputPort InputPort { get; set; } }
    public interface IOutput : IPort { }
    public interface ISingleOutput : IOutput { OutputPort OutputPort { get; set; } }
    public interface IMultipleOutput : IOutput
    {
        void ClearConnections();
        List<OutputPort> GetOutputs();
        Node NextNode();
        void AssignNodesToOutputPorts(Node node);
    }

    //public interface IMultipleOutput : IOutput
    //{
    //    void AddOutput(OutputPort output);
    //    bool RemoveOutput(OutputPort output);
    //    bool RemoveOutput(int index);
    //    void InsertOutput(int atIndex, OutputPort output);
    //    void ClearOutput();
    //    int OutputCount { get; }
    //    OutputPort GetOutput(int index);
    //}
}
