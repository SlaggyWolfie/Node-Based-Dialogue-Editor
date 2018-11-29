using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Nodes.Editor
{
    public sealed class NodePreferences
    {
        private NodePreferences() { }

        public static readonly Color GRID_BACKGROUND_COLOR = new Color(0.65f, 0.65f, 0.65f, 1);
        public static readonly Color GRID_LINE_COLOR = new Color(0.3f, 0.3f, 0.3f, 1);
        public static readonly Color CROSS_LINE_COLOR = new Color(0.1f, 0.1f, 0.1f);
        public static readonly Color CONNECTION_PORT_COLOR = Color.white;
        public static readonly Color SELECTION_FACE_COLOR = new Color(0, 0, 0, 0.1f);
        public static readonly Color SELECTION_BORDER_COLOR = new Color(1, 1, 1, 0.6f);

        public const float PROPERTY_HEIGHT = 30;
        public const float PROPERTY_MIN_WIDTH = 30;
        public static readonly Vector2 STANDARD_NODE_SIZE = new Vector2(210, 210);
        public static readonly Vector2 STANDARD_PORT_SIZE = new Vector2(16, 16);
        public static readonly Vector2 DUPLICATION_OFFSET = new Vector2(30, 30);

        public const float CONNECTION_WIDTH = 8;
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
        public Texture2D GridTexture { get { return _gridTexture ?? (_gridTexture = NodeResources.GenerateGridTexture(GRID_LINE_COLOR, GRID_BACKGROUND_COLOR)); } }
        public Texture2D CrossTexture { get { return _crossTexture ?? (_crossTexture = NodeResources.GenerateCrossTexture(CROSS_LINE_COLOR)); } }


        public enum ConnectionType { Bezier, Line, Angled }

        [Serializable]
        public sealed class NodeEditorSettings
        {
            [SerializeField]
            private bool _autosave = true;
            public bool ShouldAutoSave
            {
                get { return _autosave; }
                set { _autosave = value; }
            }

            [SerializeField]
            private bool _gridSnap = true;
            public bool ShouldGridSnap
            {
                get { return _gridSnap; }
                set { _gridSnap = value; }
            }

            //private Dictionary<Type, GraphSettings> _graphSettings = null;
        }

        [Serializable]
        public sealed class GraphSettings
        {
            //private ConnectionType _connectionType = ConnectionType.Bezier;

            private Texture2D _gridTexture = null;
            private Texture2D _crossTexture = null;
            public Texture2D GridTexture { get { return _gridTexture ?? (_gridTexture = NodeResources.GenerateGridTexture(GRID_LINE_COLOR, GRID_BACKGROUND_COLOR)); } }
            public Texture2D CrossTexture { get { return _crossTexture ?? (_crossTexture = NodeResources.GenerateCrossTexture(CROSS_LINE_COLOR)); } }
        }

        private bool _shouldAutoSave = true;

        public bool ShouldAutoSave
        {
            get { return _shouldAutoSave; }
            set { _shouldAutoSave = value; }
        }
    }
    
}
