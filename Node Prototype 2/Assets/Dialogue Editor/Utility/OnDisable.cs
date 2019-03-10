using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace WolfEditor.Utility
{
    public interface IOnDisable
    {
        void OnDisable();
    }
    public static class OnDisable
    {
        //[RuntimeInitializeOnLoadMethod]
        //public static void InitializeCallback()
        //{
            
        //}

        //private static List<IOnDisable> GetOnDisable()
        //{
        //    if (_OnDisables != null) return _OnDisables;
        //    _OnDisables = GetOnDisable();
        //    return _OnDisables;
        //}
        //private static List<IOnDisable> _OnDisables = null;

        //public List<IOnDisable> FindOnDisables()
        //{
        //    return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
        //        .Where(x => typeof(IOnDisable).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToList();
        //}
    }
}
