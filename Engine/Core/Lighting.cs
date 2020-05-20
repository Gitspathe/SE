using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Penumbra;
using SE.Components;
using SE.Core.Exceptions;
using SE.Core.Extensions;
using SE.Lighting;
using SE.Utility;
using Light = SE.Lighting.Light;
using Vector2 = System.Numerics.Vector2;
using MonoGameVector2 = Microsoft.Xna.Framework.Vector2;

namespace SE.Core
{
    public static class Lighting
    {
        public static PenumbraComponent Penumbra;

        private static QuickList<Light> pointLights = new QuickList<Light>(128);
        private static QuickList<ShadowCaster> shadowCasterList = new QuickList<ShadowCaster>(128);
        private static ObservableCollection<Hull> penumbraHulls;
        private static ObservableCollection<Penumbra.Light> penumbraLights;

        private static bool enabled;
        public static bool Enabled
        {
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

            Vector2 camPosition = camera.Position;
            Rectangle viewRect = new Rectangle(0, 0, camera.ViewBounds.Width, camera.ViewBounds.Height);
            shadowCasterList.Clear();
            penumbraHulls.Clear();

            for (int i = 0; i < pointLights.Count; i++) {
                Light l = pointLights.Array[i];
                Vector2 unscaledPos = l.Position + l.Offset;
                MonoGameVector2 pos = new MonoGameVector2(unscaledPos.X - camPosition.X, unscaledPos.Y - camPosition.Y) * camera.Zoom;
                l.PenumbraLight.Scale = Vector2Extensions.ToMonoGameVector2(l.Size) * camera.Zoom;
                l.PenumbraLight.Radius = 10 * camera.Zoom;
                l.PenumbraLight.Position = pos;
                l.PenumbraLight.Rotation = l.Rotation;
            }

            SpatialPartitionManager.GetFromRegion(shadowCasterList, camera.ViewBounds);
            shadowCasterList.AddRange(SceneManager.CurrentScene.MapShadows);

            for (int i = 0; i < shadowCasterList.Count; i++) {
                ShadowCaster s = shadowCasterList.Array[i];
                if (!s.Enabled || !viewRect.Intersects(s.Bounds))
                    continue;

                Hull h = s.Hull;
                h.Position = (s.Position - camera.Position).ToMonoGameVector2() * camera.Zoom;

                h.Scale = Vector2Extensions.ToMonoGameVector2(s.Scale) * camera.Zoom;
                h.Rotation = s.Rotation;
                penumbraHulls.Add(h);
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
            penumbraHulls = Penumbra.Hulls;
            penumbraLights = Penumbra.Lights;
        }

        public static void Reset()
        {
            if (Screen.IsFullHeadless)
                throw new HeadlessNotSupportedException("Cannot reset lighting in fully headless display mode.");

            Hull[] copyHulls = new Hull[penumbraHulls.Count];
            Penumbra.Light[] copyLights = new Penumbra.Light[penumbraLights.Count];
            penumbraHulls.CopyTo(copyHulls, 0);
            penumbraLights.CopyTo(copyLights, 0);

            Penumbra?.Dispose();
            Penumbra = new PenumbraComponent(GameEngine.Engine);
            Penumbra.Initialize();
            Penumbra.AmbientColor = Color.Black;
            penumbraHulls = Penumbra.Hulls;
            penumbraLights = Penumbra.Lights;
            penumbraHulls.AddRange(copyHulls);
            penumbraLights.AddRange(copyLights);
        }

        public static void AddLight(Light pointLight)
        {
            if (pointLight.AddedToLighting || Screen.IsFullHeadless)
                return;

            pointLights.Add(pointLight);
            penumbraLights.Add(pointLight.PenumbraLight);
            pointLight.AddedToLighting = true;
        }

        public static void RemoveLight(Light pointLight)
        {
            if(Screen.IsFullHeadless)
                return;

            pointLights.Remove(pointLight);
            Penumbra.Lights.Remove(pointLight.PenumbraLight);
            penumbraLights.Remove(pointLight.PenumbraLight);
            pointLight.AddedToLighting = false;
        }
    }
}
