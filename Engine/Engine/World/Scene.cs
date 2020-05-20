using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SE.AssetManagement;
using SE.Common;
using SE.Components;
using SE.Core;
using SE.Core.Internal;
using SE.Lighting;
using SE.Rendering;
using SE.Core.Extensions;
using SE.Utility;
//using SE.Engine.Physics;
using Vector2 = System.Numerics.Vector2;

namespace SE.World
{

    /// <summary>
    /// Represents a level within the engine. Can be overridden to change loading logic for a specific level.
    /// </summary>
    ///
    /// TODO: Move tile maps out of this class and into TileMap.cs.
    public class Scene : IAssetConsumer
    {
        public TileSpot[][] MapData;
        public Dictionary<string, MarkerGraphicData> MarkerGraphicData;
        public List<Marker> MarkerData;
        internal QuickList<ShadowCaster> MapShadows = new QuickList<ShadowCaster>();

        internal HashSet<GameObject> AttachedGameObjects = new HashSet<GameObject>();
        internal QuickList<GameObject> GameObjectsToRemove = new QuickList<GameObject>();
        internal Dictionary<string, Func<Vector2, GameObject>> TileSet;

        private QuickList<TileSpot.ShadowTemplate> shadowTemplates = new QuickList<TileSpot.ShadowTemplate>();
        private bool runInEditor;
        private SceneScript script;

        public virtual string LevelName { get; set; }
        public virtual string LevelNamespace { get; set; }
        public string FolderPath => Path.Combine(LevelNamespace, "Levels");
        private bool ExecuteScriptLoad => !GameEngine.IsEditor || (GameEngine.IsEditor && runInEditor);

        /// <summary>Amount of tiles in the X and Y axis.</summary>
        public Point TilesCount { get; private set; } = new Point(-1, -1);

        HashSet<IAsset> IAssetConsumer.ReferencedAssets { get; set; }

        internal void Update()
        {
            DestroyPending();

            if (ExecuteScriptLoad) {
                script?.SceneUpdate();
            }
        }

        internal void DestroyPending()
        {
            for (int i = 0; i < GameObjectsToRemove.Count; i++) {
                AttachedGameObjects.Remove(GameObjectsToRemove.Array[i]);
            }
            GameObjectsToRemove.Clear();
        }

        /// <summary>
        /// Loads the level from the file system. Uses the current LevelName of the level.
        /// </summary>
        internal virtual void Load()
        {
            TileSet = AssetManager.GetDictionary<Func<Vector2, GameObject>>(this);
            LoadingScreen.SetProgress("Reading file system...");
            string dataString = FileIO.ReadFile(Path.Combine(LevelNamespace, "Levels", LevelName + ".dzmap"));

            Reflection.SceneInfo sceneInfo = Reflection.GetSceneInfo(LevelNamespace, LevelName);
            script = sceneInfo.SceneScript;
            runInEditor = sceneInfo.RunInEditor;

            if (dataString != null) {
                LoadingScreen.SetProgress("Deserializing data...");
                LevelData data = dataString.Deserialize<LevelData>();
                if (ExecuteScriptLoad) {
                    script?.BeforeSceneLoad();
                }
                LoadData(data);
            }
            GenerateShadows();
            if (ExecuteScriptLoad) {
                script?.AfterSceneLoad();
            }
        }

        private void LoadData(LevelData data)
        {
            int totalTiles = data.TilesCount.X * data.TilesCount.Y;
            int curTilesLoaded = 0;
            CreateLevel(data.TilesCount);
            MapData = new TileSpot[TilesCount.X][];
            for(int x = 0; x < data.MapData.Length; x++) {
                MapData[x] = new TileSpot[TilesCount.Y];
                for(int y = 0; y < data.MapData[x].Length; y++) {
                    MapData[x][y] = new TileSpot(this);
                    MapData[x][y].TileTemplates = data.MapData[x][y];
                    CreateGameObjectsForTile(new Point(x, y));
                    curTilesLoaded++;
                }
                LoadingScreen.SetProgress("Loading", (float)curTilesLoaded / totalTiles);
            }
            MarkerData = data.MarkerData;
            MarkerGraphicData = data.MarkerGraphicData;
        }

        /// <summary>
        /// Serializes the level's data into a class, and returns the result.
        /// </summary>
        /// <returns>Level data.</returns>
        public virtual LevelData MakeData()
        {
            LevelData data = new LevelData();
            data.TilesCount = new Point(TilesCount.X, TilesCount.Y);
            data.MapData = new List<int>[TilesCount.X][];
            for(int x = 0; x < MapData.Length; x++) {
                data.MapData[x] = new List<int>[TilesCount.Y];
                for(int y = 0; y < MapData[x].Length; y++) {
                    data.MapData[x][y] = MapData[x][y].TileTemplates;
                }
            }
            data.MarkerData = MarkerData;
            data.MarkerGraphicData = MarkerGraphicData;
            return data;
        }

        void SetTilesCount(int x, int y)
        {
            TilesCount = new Point(x, y);
        }

        /// <summary>
        /// Creates and initializes the level, giving it a specified size.
        /// </summary>
        /// <param name="size">Amount of tiles on the X and Y axis.</param>
        public void CreateLevel(Point size)
        {
            MapData = new TileSpot[size.X][];
            for(int x = 0; x < size.X; x++) {
                MapData[x] = new TileSpot[size.Y];
                for(int y = 0; y < size.Y; y++) {
                    MapData[x][y] = new TileSpot(this);
                }
            }
            SetTilesCount(size.X, size.Y);
            SceneManager.WorldSize = new Point(TilesCount.X * SceneManager._TILE_SIZE, TilesCount.Y * SceneManager._TILE_SIZE);
            //NavigationManager.Initialize(new Rectangle(0, 0, SceneManager.WorldSize.X, SceneManager.WorldSize.Y));
            //LightingSystem.Initialize(new Rectangle(0, 0,LevelManager.WorldSize.X, LevelManager.WorldSize.Y), LevelManager._TILE_SIZE);
        }

        /// <summary>
        /// Places a tile at a specific position.
        /// </summary>
        /// <param name="position">Position in X and Y tile coordinates.</param>
        /// <param name="tileID">Tileset tile ID to place.</param>
        public void PlaceTile(Point position, int tileID)
        {
            if (position.X > MapData.Length-1 || position.Y > MapData[0].Length-1 || position.X < 0 || position.Y < 0 || TileExistsAtPoint(position, tileID))
                return;

            MapData[position.X][position.Y].TileTemplates.Add(tileID);
            CreateGameObjectsForTile(position);
        }

        public void GenerateShadows()
        {
            MapShadows.Clear();
            shadowTemplates.Clear();
            for (int x = 0; x < TilesCount.X; x++) {
                for (int y = 0; y < TilesCount.Y; y++) {
                    MapData[x][y].ShadowTemplates.Clear();
                    for (int z = 0; z < MapData[x][y].Gos.Count; z++) {
                        GameObject go = MapData[x][y].Gos[z];
                        for (int s = 0; s < go.Sprites.Length; s++) {
                            SpriteBase spr = go.Sprites[s];
                            if (!(spr is ILit) || ((ILit)spr).ShadowType != ShadowCasterType.Map)
                                continue;

                            Sprite sprite = (Sprite)go.Sprites[s];
                            Rectangle shadowBounds = sprite.Shadow.Bounds;
                            MapData[x][y].ShadowTemplates.Add(new TileSpot.ShadowTemplate(new Point((int)go.Bounds.X, (int)go.Bounds.Y), shadowBounds));
                        }
                    }
                }
            }

            for (int x = 0; x < TilesCount.X; x++) {
                for (int y = 0; y < TilesCount.Y; y++) {
                    for (int z = 0; z < MapData[x][y].Gos.Count; z++) {
                        for (int s = 0; s < MapData[x][y].Gos[z].Sprites.Length; s++) {
                            if(((ILit)MapData[x][y].Gos[z].Sprites[s]).ShadowType == ShadowCasterType.Map)
                                GenerateShadowsIteration(x, y, z, s);
                        }
                    }
                }
            }

            for (int x = 0; x < TilesCount.X; x++) {
                for (int y = 0; y < TilesCount.Y; y++) {
                    for (int i = 0; i < MapData[x][y].ShadowTemplates.Count; i++) {
                        if (!shadowTemplates.Contains(MapData[x][y].ShadowTemplates[i])) {
                            shadowTemplates.Add(MapData[x][y].ShadowTemplates[i]);
                        }
                    }
                }
            }

            for (int i = 0; i < shadowTemplates.Count; i++) {
                if (shadowTemplates.Array[i].Merged)
                    continue;

                ShadowCaster s = new ShadowCaster {
                    Bounds = new Rectangle(0, 0, shadowTemplates.Array[i].MaxX, shadowTemplates.Array[i].MaxY),
                    Position = new Vector2(shadowTemplates.Array[i].MinX, shadowTemplates.Array[i].MinY)
                };
                s.CalculateHull(true);
                MapShadows.Add(s);
            }
        }

        public void GenerateShadowsIteration(int x, int y, int z, int s)
        {
            GameObject go = MapData[x][y].Gos[z];
            SpriteBase sprite = go.Sprites[s];
            if(!(sprite is Sprite))
                return;

            Rectangle shadowBounds = ((Sprite)sprite).Shadow.Bounds;
            TileSpot.ShadowTemplate myShadow = MapData[x][y].GetShadowTemplate(shadowBounds);
            bool canBeSouth = shadowBounds.Height == SceneManager._TILE_SIZE && y + 1 < TilesCount.Y;
            bool canBeEast = shadowBounds.Width == SceneManager._TILE_SIZE && x + 1 < TilesCount.X;
            if (canBeSouth) {
                int plus = 1;
                while (canBeSouth && MapData[x][y + plus].GetShadowTemplate(shadowBounds) != null) {
                    TileSpot.ShadowTemplate sTemplate = MapData[x][y + plus].GetShadowTemplate(shadowBounds);
                    if (sTemplate != null && myShadow.MinX == sTemplate.MinX) {
                        myShadow.MaxY += SceneManager._TILE_SIZE;
                        sTemplate.Merged = true;
                    }
                    plus++;
                    canBeSouth = shadowBounds.Height == SceneManager._TILE_SIZE && y + plus < TilesCount.Y;
                }
            }
            if (canBeEast) {
                int plus = 1;
                while (canBeEast && MapData[x + plus][y].GetShadowTemplate(shadowBounds) != null) {
                    TileSpot.ShadowTemplate sTemplate = MapData[x + plus][y].GetShadowTemplate(shadowBounds);
                    if (sTemplate != null && myShadow.MinY == sTemplate.MinY) {
                        myShadow.MaxX += SceneManager._TILE_SIZE;
                        sTemplate.Merged = true;
                    }
                    plus++;
                    canBeEast = shadowBounds.Width == SceneManager._TILE_SIZE && x + plus < TilesCount.X;
                }
            }
        }

        /// <summary>
        /// Removes all tiles from a specific position.
        /// </summary>
        /// <param name="position">Position in X and Y tile coordinates.</param>
        public void RemoveTilesAtPoint(Point position)
        {
            if (position.X > MapData.Length-1 || position.Y > MapData[0].Length-1 || position.X < 0 || position.Y < 0)
                return;

            MapData[position.X][position.Y].RemoveAndDestroyTiles();
        }

        /// <summary>
        /// Checks if a certain tile exists at a specific position.
        /// </summary>
        /// <param name="position">Position in X and Y tile coordinates.</param>
        /// <param name="id">Tileset tile ID to check.</param>
        /// <returns>True if the specified tile exists at the given position.</returns>
        public bool TileExistsAtPoint(Point position, int id)
        {
            if(MapData[position.X][position.Y] != null && MapData[position.X][position.Y].TileTemplateExists(id))
                return true;

            return false;
        }

        /// <summary>
        /// Gets a list of tileset tile IDs that are present at a specified position.
        /// </summary>
        /// <param name="position">Position in X and Y tile coordinates.</param>
        /// <returns>List of integer tile IDs.</returns>
        public List<int> GetTileTemplatesAtPoint(Point position)
        {
            if (MapData[position.X / SceneManager._TILE_SIZE][position.Y / SceneManager._TILE_SIZE] != null)
                return MapData[position.X / SceneManager._TILE_SIZE][position.Y / SceneManager._TILE_SIZE].TileTemplates;

            return null;
        }

        /// <summary>
        /// Gets the tile spot at at a specified position.
        /// </summary>
        /// <param name="position">Position in X and Y tile coordinates.</param>
        /// <returns>Tile spot.</returns>
        public TileSpot GetTileSpotAtPoint(Point position)
        {
            if (MapData[position.X / SceneManager._TILE_SIZE][position.Y / SceneManager._TILE_SIZE] != null)
                return MapData[position.X / SceneManager._TILE_SIZE][position.Y / SceneManager._TILE_SIZE];

            return null;
        }

        /// <summary>
        /// Creates the GameObjects for a tile at a specified position.
        /// </summary>
        /// <param name="position">Position in X and Y tile coordinates.</param>
        public void CreateGameObjectsForTile(Point position)
        {
            Point actualPos = new Point(position.X * SceneManager._TILE_SIZE, position.Y * SceneManager._TILE_SIZE);
            TileSpot tileSpot = GetTileSpotAtPoint(actualPos);
            tileSpot.CreateGameObjects(actualPos);
        }

        /// <summary>
        /// Destroys and removes all tiles present in the level.
        /// </summary>
        public void RemoveAndDestroyAllTiles()
        {
            for(int x = 0; x < MapData.Length; x++) {
                for(int y = 0; y < MapData[x].Length; y++) {
                    MapData[x][y].RemoveAndDestroyTiles();
                }
            }
        }

        /// <summary>
        /// Unloads the level from memory.
        /// </summary>
        public void Unload()
        {
            if (ExecuteScriptLoad) {
                script?.BeforeSceneUnload();
            }

            ((IAssetConsumer) this).DereferenceAssets();
            RemoveAndDestroyAllTiles();
            foreach (GameObject go in AttachedGameObjects) {
                go.Destroy();
            }
            AttachedGameObjects.Clear();

            if (ExecuteScriptLoad) {
                script?.AfterSceneUnload();
            }
            GC.Collect();
        }

        /// <summary>
        /// Represents a single tile within the level's grid. Can hold multiple tile GameObjects.
        /// </summary>
        [Serializable]
        public class TileSpot
        {

            /// <summary>Contains the tileset tile IDs contained within the tile spot.</summary>
            public List<int> TileTemplates = new List<int>();

            /// <summary>Contains the instantiated tile GameObjects belonging to the tile spot.</summary>
            [JsonIgnore] public List<GameObject> Gos = new List<GameObject>();
            [JsonIgnore] public List<ShadowTemplate> ShadowTemplates = new List<ShadowTemplate>();

            [JsonIgnore] private Scene scene;

            /// <summary>
            /// Checks if the tile spot contains a specified tileset tile ID.
            /// </summary>
            /// <param name="id">Tileset tile ID to check.</param>
            /// <returns>True if a tile template of the tileset tile ID is present.</returns>
            public bool TileTemplateExists(int id)
            {
                for (int i = 0; i < TileTemplates.Count; i++) {
                    if (TileTemplates[i] == id) {
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Creates GameObjects for the tile spot.
            /// </summary>
            /// <param name="position">Tile spot position</param>
            public void CreateGameObjects(Point position)
            {
                DestroyGameObjects();
                for (int i = 0; i < TileTemplates.Count; i++) {
                    Func<Vector2, GameObject> f = scene.TileSet[TileTemplates[i].ToString()];
                    Gos.Add(f.Invoke(new Vector2(position.X, position.Y)));
                }
            }

            /// <summary>
            /// Destroy's this tile spot's GameObjects.
            /// </summary>
            public void DestroyGameObjects()
            {
                for (int i = 0; i < Gos.Count; i++) {
                    Gos[i].Destroy();
                }
                Gos.Clear();
            }

            public ShadowTemplate GetShadowTemplate(Rectangle reqBounds)
            {
                for (int i = 0; i < ShadowTemplates.Count; i++) {
                    if (ShadowTemplates[i].ReqBounds == reqBounds) {
                        return ShadowTemplates[i];
                    }
                }
                return null;
            }

            /// <summary>
            /// Destroy's the tile spot's GameObjects, and clears it's tile templates from memory.
            /// </summary>
            public void RemoveAndDestroyTiles()
            {
                DestroyGameObjects();
                TileTemplates.Clear();
            }

            public TileSpot(Scene scene)
            {
                this.scene = scene;
            }

            public class ShadowTemplate
            {
                public Rectangle ReqBounds;
                public int MinX;
                public int MaxX;
                public int MinY;
                public int MaxY;
                public bool Merged;

                public ShadowTemplate(Point pos, Rectangle bounds)
                {
                    MinX = pos.X;
                    MinY = pos.Y;
                    MaxX = bounds.Width;
                    MaxY = bounds.Height;
                    ReqBounds = bounds;
                }
            }

        }

        /// <summary>
        /// Contains level data that may be serialized and saved to the disk, or transmitted over a network.
        /// </summary>
        [Serializable]
        public class LevelData
        {
            public Point TilesCount;
            public List<int>[][] MapData;
            public Dictionary<string, MarkerGraphicData> MarkerGraphicData;
            public List<Marker> MarkerData;
        }

    }
}
