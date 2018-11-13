using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RPG.Nodes
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
            GUI.DrawTextureWithTexCoords(rect, crossTexture, new Rect(tileOffset + new Vector2(0.5f, 0.5f), tileAmount));
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

        public static void DrawPort(Rect rect, Texture backgroundTexture, Texture foregroundTexture, Texture foregroundTexture2,
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

        public static void DrawConnection(Vector2 start, Vector2 end, Color color)
        {
            Vector2 startTangent = start;
            startTangent.x = start.x < end.x ?
                Mathf.LerpUnclamped(start.x, end.x, 0.7f) :
                Mathf.LerpUnclamped(start.x, end.x, -0.7f);

            Vector2 endTangent = end;
            endTangent.x = end.x < start.x ?
                Mathf.LerpUnclamped(end.x, start.x, 0.7f) :
                Mathf.LerpUnclamped(end.x, start.x, -0.7f);

            Handles.DrawBezier(start, end, startTangent, endTangent, color, null, 2);
            DrawArrow((start + end) / 2, (endTangent - startTangent).normalized, 6, 4);
        }

        public static void DrawArrow(Vector2 position, Vector2 directionNormalized, float width, float height)
        {
            //direction = direction.normalized;
            Vector2 heightDirection = directionNormalized * height;
            Vector2 top = position + heightDirection * 2 / 3;
            Vector2 bottomMiddle = top - heightDirection;
            Vector2 perpendicularDirection = Vector3.Cross(directionNormalized, Vector3.forward);
            Vector2 bottomDifference = perpendicularDirection * width / 2;
            Vector2 left = bottomMiddle - bottomDifference;
            Vector2 right = bottomMiddle + bottomDifference;

            Handles.DrawAAConvexPolygon(top, left, right);
        }
    }

    public static class ExtensionClass
    {
        public static Vector3 ToVector3(this Vector2 vector2)
        {
            return new Vector3(vector2.x, vector2.y);
        }

        public static Vector2 ToVector2(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.y);
        }
    }
}
