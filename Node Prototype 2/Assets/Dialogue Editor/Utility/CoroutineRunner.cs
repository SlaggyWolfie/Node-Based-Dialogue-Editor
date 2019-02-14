using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RPG.Utility
{
    public static partial class Extensions
    {
        public class CoroutineHandler : MonoBehaviour { }
        private static CoroutineHandler _coroutineHandler;
        public static Coroutine RunCoroutine(this IEnumerator ienum)
        {
            if (_coroutineHandler != null) return _coroutineHandler.StartCoroutine(ienum);

            _coroutineHandler = new GameObject("[CoroutineHandler]").AddComponent<CoroutineHandler>();

            _coroutineHandler.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy |
                                        HideFlags.HideInInspector | HideFlags.NotEditable | HideFlags.DontSaveInBuild;

            _coroutineHandler.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy |
                                                   HideFlags.HideInInspector | HideFlags.NotEditable |
                                                   HideFlags.DontSaveInBuild;

            return _coroutineHandler.StartCoroutine(ienum);
        }
    }
}
