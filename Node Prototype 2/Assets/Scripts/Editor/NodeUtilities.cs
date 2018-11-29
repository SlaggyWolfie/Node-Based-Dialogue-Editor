using System;
using UnityEditor;
using UnityEngine;

namespace RPG.Nodes.Editor
{
    public static class NodeUtilities
    {
        public static void AutoSaveAssets()
        {
            if (NodePreferences.Settings.ShouldAutoSave) AssetDatabase.SaveAssets();
        }

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
    }
}
