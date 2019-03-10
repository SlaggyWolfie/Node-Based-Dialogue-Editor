using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WolfEditor.Utility;

namespace WolfEditor.Editor.Nodes
{
    public class NodeRendering
    {
        public static void DrawGrid(Rect rect, float zoom, Vector2 panOffset)
        {
            rect.position = Vector2.zero;

            Vector2 relativeCenter = rect.size / 2;
            Texture2D gridTexture = NodePreferences.Settings.GridTexture;
            Texture2D crossTexture = NodePreferences.Settings.CrossTexture;

            //Offset from origin in tile units
            float xOffset = -(relativeCenter.x * zoom + panOffset.x) / gridTexture.width;
            float yOffset = ((relativeCenter.y - rect.size.y) * zoom + panOffset.y) / gridTexture.height;

            Vector2 tileOffset = new Vector2(xOffset, yOffset);

            //Amount of tiles
            //Textures are generated 64 by 64 and re-tile themselves
            float tileAmountX = Mathf.Round(rect.size.x * zoom) / gridTexture.width;
            float tileAmountY = Mathf.Round(rect.size.y * zoom) / gridTexture.height;

            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

            //Draw tiled background
            GUI.DrawTextureWithTexCoords(rect, gridTexture, new Rect(tileOffset, tileAmount));
            GUI.DrawTextureWithTexCoords(rect, crossTexture,
                new Rect(tileOffset + new Vector2(0.5f, 0.5f), tileAmount));
        }

        public static void DrawPort(Rect rect, Texture backgroundTexture, Texture foregroundTexture,
            Color backgroundColor, Color foregroundColor, bool drawForeground = false)
        {
            Color oldColor = GUI.color;

            GUI.color = backgroundColor;
            GUI.DrawTexture(rect, backgroundTexture);

            if (!drawForeground)
            {
                GUI.color = foregroundColor;
                GUI.DrawTexture(rect, foregroundTexture);
            }

            GUI.color = oldColor;
        }

        public static void DrawPort(Rect rect, Texture backgroundTexture, Texture foregroundTexture,
            Texture foregroundTexture2,
            Color backgroundColor, Color foregroundColor, Color foregroundColor2, bool drawForeground = false)
        {
            Color oldColor = GUI.color;

            GUI.color = backgroundColor;
            GUI.DrawTexture(rect, backgroundTexture);

            if (!drawForeground)
            {
                GUI.color = foregroundColor2;
                GUI.DrawTexture(rect, foregroundTexture2);
            }
            else
            {
                GUI.color = foregroundColor;
                GUI.DrawTexture(rect, foregroundTexture);
            }

            GUI.color = oldColor;
        }

        public static void DrawConnection(Vector2 start, Vector2 end, Color color, float width, bool drawArrows = true)
        {
            //DrawConnectionBezier(start, end, color, width);
            DrawConnectionStraight(start, end, color, width, drawArrows);
        }

        public static void DrawConnectionStraight(Vector2 start, Vector2 end, Color color, float width,
            bool drawArrows = true)
        {
            Handles.DrawAAPolyLine(width, start, end);
            Vector2 direction = end - start;
            Vector2 directionNormalized = direction.normalized;
            if (!drawArrows) return;
            DrawArrow(start + direction / 4, directionNormalized, width * 2);
            DrawArrow(start + direction * 3 / 4, directionNormalized, width * 2);
        }

        public static void DrawConnectionBezier(Vector2 start, Vector2 end, Color color, float width)
        {
            bool startIsLeftOfEnd = start.x < end.x;

            Vector2 startTangent = start;
            startTangent.x = startIsLeftOfEnd
                ? Mathf.LerpUnclamped(start.x, end.x, 0.7f)
                : Mathf.LerpUnclamped(start.x, end.x, -0.7f);

            Vector2 endTangent = end;
            endTangent.x = startIsLeftOfEnd
                ? Mathf.LerpUnclamped(end.x, start.x, 0.7f)
                : Mathf.LerpUnclamped(end.x, start.x, -0.7f);

            Handles.DrawBezier(start, end, startTangent, endTangent, color, null, width);

            //Draw arrow at the center
            DrawArrow((start + end) / 2, (endTangent - startTangent).normalized, 16);
            //Vector2 center = CubicBezierCurvePoint(0.5f, start, end, startTangent, endTangent);
            //DrawArrow(center, (endTangent - startTangent).normalized, 16);
        }

        public static void DrawArrow(Vector2 position, Vector2 directionNormalized, float length)
        {
            //direction = direction.normalized;
            float halfWidth = length / Mathf.Tan(Mathf.Deg2Rad * 60);
            Vector2 heightDirection = directionNormalized * length;
            Vector2 top = position + heightDirection * 2 / 3;
            Vector2 bottomMiddle = top - heightDirection;
            Vector2 perpendicularDirection = Utilities.GetPerpendicular(directionNormalized);
            //Vector2 perpendicularDirection = Vector3.Cross(directionNormalized, Vector3.forward);
            Vector2 bottomDifference = perpendicularDirection * halfWidth;
            Vector2 left = bottomMiddle - bottomDifference;
            Vector2 right = bottomMiddle + bottomDifference;

            Handles.DrawAAConvexPolygon(top, left, right);
        }

        public static Vector2 CubicBezierCurvePoint(float time, Vector2 start, Vector2 end, float straightness)
        {
            float curve = 1 - straightness;

            Vector2 startTangent = start;
            startTangent.x = start.x < end.x
                ? Mathf.LerpUnclamped(start.x, end.x, curve)
                : Mathf.LerpUnclamped(start.x, end.x, -curve);

            Vector2 endTangent = end;
            endTangent.x = end.x > start.x
                ? Mathf.LerpUnclamped(end.x, start.x, curve)
                : Mathf.LerpUnclamped(end.x, start.x, -curve);

            return CubicBezierCurvePoint(time, start, end, startTangent, endTangent);
        }

        public static Vector2 CubicBezierCurvePoint(float time, Vector2 start, Vector2 end, Vector2 control1, Vector2 control2)
        {
            Vector2 startTangent = control1;
            Vector2 endTangent = control2;

            float inverseTime = 1 - time;
            Vector2 point = Mathf.Pow(inverseTime, 3) * start +
                            Mathf.Pow(inverseTime, 2) * time * startTangent +
                            Mathf.Pow(time, 2) * inverseTime * endTangent +
                            Mathf.Pow(time, 3) * end;

            return point;
        }

        public static Vector2[] CubicBezierCurve(int pointAmount, Vector2 start, Vector2 end, float straightness)
        {
            float curve = 1 - straightness;

            Vector2 startTangent = start;
            startTangent.x = start.x < end.x
                ? Mathf.LerpUnclamped(start.x, end.x, curve)
                : Mathf.LerpUnclamped(start.x, end.x, -curve);

            Vector2 endTangent = end;
            endTangent.x = end.x > start.x
                ? Mathf.LerpUnclamped(end.x, start.x, curve)
                : Mathf.LerpUnclamped(end.x, start.x, -curve);

            List<Vector2> points = new List<Vector2>();
            float jump = 1.0f / pointAmount;
            for (float time = 0; time <= 1; time += jump)
            {
                float inverseTime = 1 - time;
                Vector2 point = Mathf.Pow(inverseTime, 3) * start +
                                Mathf.Pow(inverseTime, 2) * time * startTangent +
                                Mathf.Pow(time, 2) * inverseTime * endTangent +
                                Mathf.Pow(time, 3) * end;
                points.Add(point);
            }

            return points.ToArray();
        }
    }
}
