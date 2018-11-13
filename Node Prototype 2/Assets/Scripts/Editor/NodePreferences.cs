using UnityEngine;

namespace RPG.Nodes
{
    public sealed class NodePreferences
    {
        public static readonly Color GRID_BACKGROUND_COLOR = new Color(0.65f, 0.65f, 0.65f, 1);
        public static readonly Color GRID_LINE_COLOR = new Color(0.2f, 0.2f, 0.2f, 1);
        public static readonly Color CROSS_LINE_COLOR = new Color(0.9f, 0.9f, 0.9f);
        public static readonly Color CONNECTION_PORT_COLOR = Color.white;

        public const float MIN_ZOOM = 1;
        public const float MAX_ZOOM = 5;
        public const int GRID_SIZE = 64;

        private static NodePreferences _settings = null;
        public static NodePreferences Settings
        {
            get { return _settings ?? (_settings = new NodePreferences()); }
        }

        private Texture2D _gridTexture = null;
        private Texture2D _crossTexture = null;
        public Texture2D GridTexture { get { return _gridTexture ?? (_gridTexture = NodeUtilities.GenerateGridTexture(GRID_LINE_COLOR, GRID_BACKGROUND_COLOR)); } }
        public Texture2D CrossTexture { get { return _crossTexture ?? (_crossTexture = NodeUtilities.GenerateCrossTexture(CROSS_LINE_COLOR)); } }


        public sealed class NodeGraphSettings
        {
            private Texture2D _gridTexture = null;
            private Texture2D _crossTexture = null;
            public Texture2D GridTexture { get { return _gridTexture ?? (_gridTexture = NodeUtilities.GenerateGridTexture(GRID_LINE_COLOR, GRID_BACKGROUND_COLOR)); } }
            public Texture2D CrossTexture { get { return _crossTexture ?? (_crossTexture = NodeUtilities.GenerateCrossTexture(CROSS_LINE_COLOR)); } }
        }

        private NodePreferences() { }
        private bool _shouldAutoSave = true;
        public bool ShouldAutoSave
        {
            get { return _shouldAutoSave; }
            set { _shouldAutoSave = value; }
        }
    }
}
