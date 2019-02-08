using System;
using RPG.Nodes.Base;
using UnityEditor;
using UnityEngine;

namespace RPG.Utility
{
    public interface ICondition
    {
        int ValueCount { get; }
        Value GetValue(int index);
        void AddValue(Value value);
        bool RemoveValue(Value value);
        void RemoveValue(int index);

        int ConditionCount { get; }
        Condition GetCondition(int index);
        void AddCondition(Condition condition);
        bool RemoveCondition(Condition condition);
        void RemoveCondition(int index);
    }

    public static class Utilities
    {
        public static void BeginZoom(Rect rect, float zoom, float topPadding)
        {
            GUI.EndClip();
            GUIUtility.ScaleAroundPivot(Vector2.one / zoom, rect.size * 0.5f);
            Rect viewport = new Rect(rect.size * (1 - zoom) * 0.5f, rect.size * zoom);
            viewport.y += topPadding * zoom;
            GUI.BeginClip(viewport);
        }

        public static void EndZoom(Rect rect, float zoom, float topPadding)
        {
            GUIUtility.ScaleAroundPivot(Vector2.one * zoom, rect.size * 0.5f);
            Vector2 reverseViewport = rect.size * (zoom - 1) * 0.5f;
            Vector3 offset = new Vector3(reverseViewport.x, reverseViewport.y + topPadding * (1 - zoom), 0);
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
        }

        //Left to Right Direction
        public static Vector2 GetPerpendicular(Vector2 direction)
        {
            return new Vector2(-direction.y, direction.x);
        }

        public static bool PointOverlapBezier(Vector2 point, Vector2 start, Vector2 end, float width)
        {
            throw new NotImplementedException();
            return true;
        }

        public static bool NearlyEqual(float a, float b, float epsilon)
        {
            float absoluteA = Mathf.Abs(a);
            float absoluteB = Mathf.Abs(b);
            float difference = Mathf.Abs(a - b);

            //shortcut, handles infinity
            if (a == b) return true;

            if (a == 0 || b == 0 || difference < float.MinValue)
            {
                //a or b is zero or both are extremely close to it
                //relative error is less meaningful here
                return difference < epsilon * float.MinValue;
            }
            //shortcut, handles infinity

            return difference / Math.Min(absoluteA + absoluteB, float.MaxValue) < epsilon;
        }
    }
}
