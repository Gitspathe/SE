using Microsoft.Xna.Framework;
using Penumbra;
using SE.Components;
using SE.Core.Exceptions;
using SE.Core.Extensions;
using SE.Lighting;
using SE.Utility;
using Light = SE.Lighting.Light;
using MGVector2 = Microsoft.Xna.Framework.Vector2;
using Vector2 = System.Numerics.Vector2;

namespace SE.Core
{
    public static class Lighting
    {
        public static PenumbraComponent Penumbra;

        private static QuickList<Light> pointLights = new QuickList<Light>(128);
        private static QuickList<ShadowCaster> shadowCasterList = new QuickList<ShadowCaster>(128);

        private static object lightManagerLock = new object();

        private static float cleanUpTimer = 10.0f;

        private static bool enabled;
        public static bool Enabled {
            get => enabled;
            set {
                if (value && Screen.IsFullHeadless)
                    throw new HeadlessNotSupportedException("Cannot enable lighting in fully headless display mode.");

                enabled = value;
                Penumbra.Enabled = enabled;
            }
        }

        public static Color Ambient {
            get {
                if (Screen.IsFullHeadless)
                    throw new HeadlessNotSupportedException("Cannot retrieve lighting value in fully headless display mode.");

                return Penumbra.AmbientColor;
            }
            set {
                if (Screen.IsFullHeadless)
                    throw new HeadlessNotSupportedException("Cannot set lighting value in fully headless display mode.");

                Penumbra.AmbientColor = value;
            }
        }

        public static bool Debug {
            get {
                if (Screen.IsFullHeadless)
                    throw new HeadlessNotSupportedException("Cannot retrieve lighting value in fully headless display mode.");

                return Penumbra.Debug;
            }
            set {
                if (Screen.IsFullHeadless)
                    throw new HeadlessNotSupportedException("Cannot set lighting value in fully headless display mode.");

                Penumbra.Debug = value;
            }
        }

        public static void Update(Camera2D camera)
        {
            if (!enabled)
                return;
            if (Screen.IsFullHeadless)
                throw new HeadlessNotSupportedException("Cannot update lighting in fully headless display mode.");

            // Clear old Penumbra vao data to fix a stupid memory leak.
            cleanUpTimer -= Time.DeltaTime;
            if (cleanUpTimer <= 0.0f) {
                Penumbra.ClearLightVaos();
                cleanUpTimer = 10.0f;
            }

            Penumbra.Transform = camera.ViewMatrix;

            Rectangle viewRect = new Rectangle(0, 0, camera.ViewBounds.Width, camera.ViewBounds.Height);
            shadowCasterList.Clear();
            Penumbra.Hulls.Clear();

            for (int i = 0; i < pointLights.Count; i++) {
                Light l = pointLights.Array[i];
                Vector2 unscaledPos = l.Position + l.Offset;
                MGVector2 pos = new MGVector2(unscaledPos.X, unscaledPos.Y);
                l.PenumbraLight.Scale = l.Size.ToMonoGameVector2();
                l.PenumbraLight.Radius = 10;
                l.PenumbraLight.Position = pos;
                l.PenumbraLight.Rotation = l.Rotation;
            }

            SpatialPartitionManager<ShadowCaster>.GetFromRegion(shadowCasterList, camera.ViewBounds);
            shadowCasterList.AddRange(SceneManager.CurrentScene.MapShadows);

            for (int i = 0; i < shadowCasterList.Count; i++) {
                ShadowCaster s = shadowCasterList.Array[i];
                if (!s.Enabled || !viewRect.Intersects(s.Bounds))
                    continue;

                Hull h = s.Hull;

                // Clamp position and scale to int. Avoids graphics issues.
                h.Position = new MGVector2((int)s.Position.X, (int)s.Position.Y);
                h.Scale = new MGVector2((int)s.Scale.X, (int)s.Scale.Y);
                h.Rotation = s.Rotation;
                Penumbra.Hulls.Add(h);
            }
        }

        internal static void Initialize()
        {
            if (Screen.IsFullHeadless)
                throw new HeadlessNotSupportedException("Cannot initialize lighting in fully headless display mode.");

            Penumbra = new PenumbraComponent(GameEngine.Engine);
            Penumbra.Initialize();
            Penumbra.AmbientColor = Color.Black;
            Enabled = true;
        }

        public static void Reset()
        {
            if (Screen.IsFullHeadless)
                throw new HeadlessNotSupportedException("Cannot reset lighting in fully headless display mode.");

            lock (lightManagerLock) {
                Hull[] copyHulls = new Hull[Penumbra.Hulls.Count];
                Penumbra.Light[] copyLights = new Penumbra.Light[Penumbra.Lights.Count];
                Penumbra.Hulls.CopyTo(copyHulls, 0);
                Penumbra.Lights.CopyTo(copyLights, 0);
                Penumbra.Hulls.Clear();
                Penumbra.Lights.Clear();

                Penumbra?.Dispose();
                Penumbra = new PenumbraComponent(GameEngine.Engine);
                Penumbra.Initialize();
                Penumbra.AmbientColor = Color.Black;
                Penumbra.Hulls.AddRange(copyHulls);
                Penumbra.Lights.AddRange(copyLights);
            }
        }

        public static void AddLight(Light pointLight)
        {
            if (pointLight.AddedToLighting || Screen.IsFullHeadless)
                return;

            lock (lightManagerLock) {
                pointLights.Add(pointLight);
                Penumbra.Lights.Add(pointLight.PenumbraLight);
            }
            pointLight.AddedToLighting = true;
        }

        public static void RemoveLight(Light pointLight)
        {
            if (Screen.IsFullHeadless)
                return;

            lock (lightManagerLock) {
                pointLights.Remove(pointLight);
                Penumbra.Lights.Remove(pointLight.PenumbraLight);
            }
            pointLight.AddedToLighting = false;
        }
    }
}
