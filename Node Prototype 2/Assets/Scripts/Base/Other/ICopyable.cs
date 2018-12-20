using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RPG.Nodes.Base;

namespace RPG.Other
{
    public interface ICopyable
    {
        object ShallowCopy();
        object DeepCopy();
    }

    public interface ICopyable<out T>
    {
        T ShallowCopy();
        T DeepCopy();
    }

    public static class ICopyableExtension
    {
        private static object MemberwiseClone(object obj)
        {
            const BindingFlags binding = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            MethodInfo cloneMethodInfo = typeof(object).GetMethod("MemberwiseClone", binding);
            Func<object> cloneMethod = (Func<object>)Delegate.CreateDelegate(typeof(Func<object>), obj, cloneMethodInfo);
            return cloneMethod();
        }

        public static object DefaultShallowCopy(this ICopyable copyInterface)
        {
            return MemberwiseClone(copyInterface);
        }

        public static T DefaultShallowCopy<T>(this ICopyable<T> copyInterface)
        {
            return (T)MemberwiseClone(copyInterface);
        }
    }
}