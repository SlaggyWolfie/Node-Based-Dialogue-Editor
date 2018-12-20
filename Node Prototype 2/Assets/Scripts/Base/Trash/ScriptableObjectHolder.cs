using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RPG.Base
{
    public class ScriptableObjectHolder : BaseScriptableObject { }

    public class SubAssetHolder<T> where T : BaseScriptableObject
    {
        private T _subAsset = null;
        public T SubAsset { get { return _subAsset; } }

        private BaseScriptableObject _parent = null;
        private string _name = String.Empty;
        private string _label = String.Empty;

        public SubAssetHolder(string name, string label, BaseScriptableObject parent, Func<T> subAssetConstructor)
        {
            _name = name;
            _label = label;
            _parent = parent;
            _subAsset = GetScriptableObjectChild(subAssetConstructor);
        }

        public SubAssetHolder(string name, string label, BaseScriptableObject parent)
        {
            _name = name;
            _label = label;
            _parent = parent;
            _subAsset = GetScriptableObjectChild();
        }

        private T GetScriptableObjectChild()
        {
            if (_subAsset != null) return _subAsset;

            string graphPath = AssetDatabase.GetAssetPath(_parent);
            //Debug.Log("Graph's Path: " + graphPath);

            string[] GUIDs = AssetDatabase.FindAssets(string.Format("l:{0}", _label));
            foreach (string GUID in GUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(GUID);
                //Debug.Log("Checking path for Holder: " + path);
                if (path != graphPath) continue;
                T subAsset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (subAsset == null || !AssetDatabase.IsSubAsset(subAsset)) continue;
                _subAsset = subAsset;
                //Debug.Log("Found holder");
                return subAsset;
            }

            T newSubAsset = ScriptableObject.CreateInstance<T>();
            newSubAsset.name = _name;
            AssetDatabase.AddObjectToAsset(newSubAsset, _parent);
            var labels = AssetDatabase.GetLabels(newSubAsset).ToList();
            labels.Add(_label);
            AssetDatabase.SetLabels(newSubAsset, labels.ToArray());
            //AssetDatabase.SetLabels(connectionHolder, new[] { label });
            _subAsset = newSubAsset;
            return newSubAsset;
        }

        private T GetScriptableObjectChild(Func<T> subAssetConstructor)
        {
            if (_subAsset != null) return _subAsset;

            string graphPath = AssetDatabase.GetAssetPath(_parent);
            //Debug.Log("Graph's Path: " + graphPath);

            string[] GUIDs = AssetDatabase.FindAssets(string.Format("l:{0}", _label));
            foreach (string GUID in GUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(GUID);
                //Debug.Log("Checking path for Holder: " + path);
                if (path != graphPath) continue;
                T subAsset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (subAsset == null || !AssetDatabase.IsSubAsset(subAsset)) continue;
                _subAsset = subAsset;
                //Debug.Log("Found holder");
                return subAsset;
            }

            //T newSubAsset = ScriptableObject.CreateInstance<T>();
            T newSubAsset = subAssetConstructor();
            newSubAsset.name = _name;
            AssetDatabase.AddObjectToAsset(newSubAsset, _parent);
            var labels = AssetDatabase.GetLabels(newSubAsset).ToList();
            labels.Add(_label);
            AssetDatabase.SetLabels(newSubAsset, labels.ToArray());
            //AssetDatabase.SetLabels(connectionHolder, new[] { label });
            _subAsset = newSubAsset;
            return newSubAsset;
        }

        //private SubAssetHolder<ScriptableObjectHolder> _nodeHolder = null;
        //private SubAssetHolder<ScriptableObjectHolder> NodeHolder
        //{
        //    get
        //    {
        //        if (_nodeHolder != null) return _nodeHolder;
        //        _nodeHolder = new SubAssetHolder<ScriptableObjectHolder>("Nodes", "Node Holder", Target);
        //        return _nodeHolder;
        //    }
        //}

        //private SubAssetHolder<ScriptableObjectHolder> _connectionsHolder = null;
        //private SubAssetHolder<ScriptableObjectHolder> ConnectionsHolder
        //{
        //    get
        //    {
        //        if (_connectionsHolder != null) return _connectionsHolder;
        //        _connectionsHolder = new SubAssetHolder<ScriptableObjectHolder>("Connections", "Connection Holder", Target);
        //        return _connectionsHolder;
        //    }
        //}

        //private ScriptableObjectHolder _nodeHolder = null;
        //private ScriptableObjectHolder NodeHolder
        //{
        //    get
        //    {
        //        if (_nodeHolder != null) return _nodeHolder;

        //        string label = "Node Holder";
        //        string graphPath = AssetDatabase.GetAssetPath(Target);
        //        //Debug.Log("Graph's Path: " + graphPath);

        //        string[] GUIDs = AssetDatabase.FindAssets(string.Format("l:{0}", label));
        //        foreach (string GUID in GUIDs)
        //        {
        //            string path = AssetDatabase.GUIDToAssetPath(GUID);
        //            //Debug.Log("Checking path for Holder: " + path);
        //            if (path != graphPath) continue;

        //            ScriptableObjectHolder holder = AssetDatabase.LoadAssetAtPath<ScriptableObjectHolder>(path);

        //            if (holder == null || !AssetDatabase.IsSubAsset(holder)) continue;
        //            _nodeHolder = holder;
        //            //Debug.Log("Found holder");
        //            return _nodeHolder;
        //        }

        //        ScriptableObjectHolder nodeHolder = ScriptableObject.CreateInstance<ScriptableObjectHolder>();
        //        nodeHolder.name = "Nodes";
        //        AssetDatabase.AddObjectToAsset(nodeHolder, Target);
        //        var labels = AssetDatabase.GetLabels(nodeHolder).ToList();
        //        labels.Add(label);
        //        AssetDatabase.SetLabels(nodeHolder, labels.ToArray());
        //        //AssetDatabase.SetLabels(connectionHolder, new[] { label });
        //        _nodeHolder = nodeHolder;
        //        return nodeHolder;
        //    }
        //}

        //private ScriptableObjectHolder _connectionHolder = null;
        //private ScriptableObjectHolder ConnectionHolder
        //{
        //    get
        //    {
        //        if (_connectionHolder != null) return _connectionHolder;

        //        string label = "Connection Holder";
        //        string graphPath = AssetDatabase.GetAssetPath(Target);
        //        //Debug.Log("Graph's Path: " + graphPath);

        //        string[] GUIDs = AssetDatabase.FindAssets(string.Format("l:{0}", label));
        //        if (GUIDs.Length != 0)
        //        {
        //            foreach (string GUID in GUIDs)
        //            {
        //                string path = AssetDatabase.GUIDToAssetPath(GUID);
        //                //Debug.Log("Checking path for Holder: " + path);
        //                if (path != graphPath) continue;

        //                ScriptableObjectHolder holder = AssetDatabase.LoadAssetAtPath<ScriptableObjectHolder>(path);

        //                if (holder == null || !AssetDatabase.IsSubAsset(holder)) continue;
        //                _connectionHolder = holder;
        //                //Debug.Log("Found holder");
        //                return holder;
        //            }
        //        }

        //        ScriptableObjectHolder connectionHolder = ScriptableObject.CreateInstance<ScriptableObjectHolder>();
        //        connectionHolder.name = "Connections";
        //        AssetDatabase.AddObjectToAsset(connectionHolder, Target);
        //        var labels = AssetDatabase.GetLabels(connectionHolder).ToList();
        //        labels.Add(label);
        //        AssetDatabase.SetLabels(connectionHolder, labels.ToArray());
        //        //AssetDatabase.SetLabels(connectionHolder, new[] { label });
        //        _connectionHolder = connectionHolder;
        //        return connectionHolder;
        //    }
        //}
    }
}
