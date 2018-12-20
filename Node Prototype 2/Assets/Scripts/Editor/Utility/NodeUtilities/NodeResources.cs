using RPG.Editor.Nodes;
using UnityEditor;
using UnityEngine;

namespace RPG.Editor
{
    public static class NodeResources
    {
        // Textures
        private static Texture2D _dotTexture;
        public static Texture2D DotTexture
        {
            get
            {
                return _dotTexture ?? (_dotTexture = Resources.Load<Texture2D>("dot"));
            }
        }
        private static Texture2D _outerDotTexture;
        public static Texture2D OuterDotTexture
        {
            get
            {
                return _outerDotTexture ?? (_outerDotTexture = Resources.Load<Texture2D>("dot_outer"));
            }
        }
        private static Texture2D _nodeBodyTexture;
        public static Texture2D NodeBodyTexture
        {
            get
            {
                return _nodeBodyTexture ?? (_nodeBodyTexture = Resources.Load<Texture2D>("node"));
            }
        }
        private static Texture2D _nodeHighlightTexture;
        public static Texture2D NodeHighlightTexture
        {
            get
            {
                return _nodeHighlightTexture ?? (_nodeHighlightTexture = Resources.Load<Texture2D>("node_highlight"));
            }
        }

        // Styles
        private static StyleHolder _styles;
        public static StyleHolder Styles { get { return _styles ?? (_styles = new StyleHolder()); } }

        public class StyleHolder
        {
            public readonly GUIStyle inputPort, outputPort, nodeHeader, nodeBody, tooltip, nodeHighlight;

            public StyleHolder()
            {
                GUIStyle baseStyle = new GUIStyle("Label") { fixedHeight = 18 };

                inputPort = new GUIStyle(baseStyle)
                {
                    alignment = TextAnchor.UpperLeft,
                    padding = { left = 10 }
                };

                outputPort = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperRight };

                nodeHeader = new GUIStyle
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white }
                };

                nodeBody = new GUIStyle
                {
                    normal = { background = NodeBodyTexture },
                    border = new RectOffset(32, 32, 32, 32),
                    padding = new RectOffset(16, 16, 4, 16)
                };

                nodeHighlight = new GUIStyle
                {
                    normal = { background = NodeHighlightTexture },
                    border = new RectOffset(32, 32, 32, 32)
                };

                //tooltip = new GUIStyle("helpBox");
                //tooltip.alignment = TextAnchor.MiddleCenter;
            }
        }

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
            texture.name = "Grid Texture";
            texture.Apply();
            return texture;
        }

        public static Texture2D GenerateCrossTexture(Color line, int gridSize = NodePreferences.GRID_SIZE)
        {
            Texture2D texture = new Texture2D(gridSize, gridSize);
            Color[] colors = new Color[gridSize * gridSize];
            for (int rows = 0; rows < gridSize; rows++)
            {
                for (int columns = 0; columns < gridSize; columns++)
                {
                    Color color = line;
                    if (columns != gridSize / 2 - 1 && rows != gridSize / 2 - 1) color.a = 0;
                    colors[columns * gridSize + rows] = color;
                }
            }

            texture.SetPixels(colors);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.name = "Grid Cross Texture";
            texture.Apply();
            return texture;
        }
    }
}