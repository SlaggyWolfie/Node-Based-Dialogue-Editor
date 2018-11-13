using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPG.Nodes
{
    public static class NodeUtilities
    {
        public static Texture2D GenerateGridTexture(Color line, Color background, int gridSize = NodePreferences.GRID_SIZE)
        {
            Texture2D texture = new Texture2D(gridSize, gridSize);
            Color[] colorPixels = new Color[gridSize * gridSize];
            for (int column = 0; column < gridSize; column++)
            {
                for (int row = 0; row < gridSize; row++)
                {
                    Color color = background;

                    //Nice fading between the grid and background
                    int startFadePosition = gridSize / 4;
                    if (column % startFadePosition == 0 || row % startFadePosition == 0) color = Color.Lerp(line, background, 0.65f);
                    if (column == gridSize - 1 || row == gridSize - 1) color = Color.Lerp(line, background, 0.35f);

                    colorPixels[column * gridSize + row] = color;
                }
            }

            texture.SetPixels(colorPixels);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;
            texture.name = "Grid";
            texture.Apply();
            return texture;
        }
        public static Texture2D GenerateCrossTexture(Color line, int gridSize = NodePreferences.GRID_SIZE)
        {
            Texture2D texture = new Texture2D(gridSize, gridSize);
            Color[] colors = new Color[gridSize * gridSize];
            for (int columns = 0; columns < gridSize; columns++)
            {
                for (int rows = 0; rows < gridSize; rows++)
                {
                    Color color = line;

                    //Absolute color
                    if (columns != gridSize - 1 && rows != gridSize - 1) color.a = 0;

                    colors[columns * gridSize + rows] = color;
                }
            }

            texture.SetPixels(colors);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.name = "Cross";
            texture.Apply();
            return texture;
        }

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

    }
}
