using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace WolfEditor.Utility.Editor
{
    public static class EditorUtilities
    {
        //(Old) Stolen from https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBaseEditor/EditorHelper.cs
        //Stolen from https://github.com/lordofduct/spacepuppy-unity-framework-3.0/blob/master/SpacepuppyUnityFrameworkEditor/EditorHelper.cs
        #region Target Object of Property

        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "")
                        .Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            return obj;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }

            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }

            return enm.Current;
        }

        public static void SetTargetObjectOfProperty(SerializedProperty prop, object value)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal))
                        .Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            if (ReferenceEquals(obj, null)) return;

            try
            {
                var element = elements.Last();

                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "")
                        .Replace("]", ""));

                    var tp = obj.GetType();
                    var field = tp.GetField(elementName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var arr = field.GetValue(obj) as IList;

                    if (arr != null) arr[index] = value;
                }
                else
                {
                    var tp = obj.GetType();
                    var field = tp.GetField(element,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null) field.SetValue(obj, value);
                }

            }
            catch
            {
                // ignored
            }
        }

        #endregion
    }

    public class SerializedList<T>
    {
        private readonly string _arrayAccessorPath;
        private readonly SerializedObject _serializedObject = null;

        public readonly SerializedProperty sizeProperty = null;

        public int Size
        {
            get { return sizeProperty.intValue; }
            set { sizeProperty.intValue = value; }
        }

        public SerializedList(SerializedObject serializedObject, string sizePath, string arrayAccessorPath)
        {
            _serializedObject = serializedObject;
            _arrayAccessorPath = arrayAccessorPath;

            sizeProperty = serializedObject.FindProperty(sizePath);
        }

        public T GetObject(int index)
        {
            return (T)EditorUtilities.GetTargetObjectOfProperty(GetProperty(index));
        }
        public SerializedProperty GetProperty(int index)
        {
            return _serializedObject.FindProperty(string.Format(_arrayAccessorPath, index));
        }
        public void SetAt(int index, T obj)
        {
            EditorUtilities.SetTargetObjectOfProperty(GetProperty(index), obj);
        }

        public bool IsNotMinIndex(int index) { return index > 0; }
        public bool IsNotMaxIndex(int index) { return index < Size; }
        public void Swap(int lhs, int rhs)
        {
            T obj = GetObject(lhs);
            SetAt(lhs, GetObject(rhs));
            SetAt(rhs, obj);
        }

        public void Add()
        {
            Size++;
            SetAt(Size - 1, default(T));
        }
        public void Remove(int index)
        {
            for (int i = index; i < Size - 1; i++) SetAt(i, GetObject(i + 1));
            Size--;
        }
    }
}
