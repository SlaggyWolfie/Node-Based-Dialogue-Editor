using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WolfEditor.Utility
{
    public static partial class Extensions
    {
        public class BehaviourCallbackSender : MonoBehaviour
        {
            private static BehaviourCallbackSender _behaviourCallbackSender;
            public static BehaviourCallbackSender Instance
            {
                get
                {
                    if (_behaviourCallbackSender != null) return _behaviourCallbackSender;

                    GameObject go = new GameObject("[BehaviourCallbackSender]") { hideFlags = HideFlags.HideAndDontSave };

                    _behaviourCallbackSender = go.AddComponent<BehaviourCallbackSender>();
                    _behaviourCallbackSender.hideFlags = HideFlags.HideAndDontSave;

                    return _behaviourCallbackSender;
                }
            }

            public Action onApplicationQuit;
            private void OnApplicationQuit() { if (onApplicationQuit != null) onApplicationQuit(); }
        }
    }
}
