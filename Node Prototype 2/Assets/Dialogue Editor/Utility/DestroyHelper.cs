using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WolfEditor
{
    public sealed class DestroyHelper
    {
        private DestroyHelper()
        {
#if UNITY_EDITOR
            EditorApplication.update += Update;
#endif
        }

        private static DestroyHelper _instance = null;
        public static DestroyHelper Instance
        {
            get { return _instance ?? (_instance = new DestroyHelper()); }
        }
        public static bool Destroy(ScriptableObject o) { return Instance.SetUpDestruction(o); }

        private HashSet<ScriptableObject> _toDestroy = new HashSet<ScriptableObject>();
        private HashSet<ScriptableObject> _calledDestroyOn = new HashSet<ScriptableObject>();

        public bool SetUpDestruction(ScriptableObject o) { return o != null && _toDestroy.Add(o); }

        public void Update()
        {
            _calledDestroyOn.RemoveWhere(o => o == null && !o);

            var needToDestroy = _toDestroy.Except(_calledDestroyOn);
            foreach (ScriptableObject scriptableObject in needToDestroy)
            {
                Object.DestroyImmediate(scriptableObject, true);
                _calledDestroyOn.Add(scriptableObject);
            }
        }
    }
}
