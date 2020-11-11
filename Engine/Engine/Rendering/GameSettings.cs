using System;

namespace SE.Rendering
{
    public static class GameSettings
    {
        //Default & Display settings
        public static int g_screenwidth = 1920;
        public static int g_screenheight = 1080;
        public static bool g_vsync = false;
        public static int g_fixedfps = 0;
        public static int u_showdisplayinfo = 3;
        public static bool p_physics = false;
        public static Renderer.RenderModes g_rendermode = Renderer.RenderModes.Deferred;

        //Editor
        public static bool e_enableeditor = true;
        public static bool e_drawoutlines = true;
        public static bool e_drawboundingbox = true;

        //UI
        public static bool ui_enabled = true;

        //Renderer

        //debug
        public static bool d_drawlines = true;

        //Default Material
        public static bool d_defaultmaterial = false;
        public static float m_defaultroughness = 0.5f;

        //Settings
        public static float g_farplane = 500;
        public static bool g_cpusort = true;
        public static bool g_cpuculling = true;
        public static bool g_batchbymaterial = false; //Note this must be activated before the application is started.

        //Profiler
        public static bool d_profiler = false;

        //Environment mapping
        public static bool g_environmentmapping = true;
        public static bool g_envmapupdateeveryframe = false;
        public static int g_envmapresolution = 1024;

        //Shadow Settings
        public static int g_shadowforcefiltering = 0; //1 = PCF, 2 3 better PCF  4 = Poisson, 5 = VSM;
        public static bool g_shadowforcescreenspace = false;

        //Deferred Decals
        public static bool g_drawdecals = true;

        //Forward pass
        public static bool g_forwardenable = true;

        //Temporal AntiAliasing
        public static bool g_taa = true;
        public static int g_taa_jittermode = 2;
        public static bool g_taa_tonemapped = true;

        //Screen Space Ambient Occlusion

        public static bool g_ssao_blur = true;

        //private static float _g_TemporalAntiAliasingThreshold = 0.9f;
        public static int g_UseDepthStencilLightCulling = 1; //None, Depth, Depth+Stencil
        public static bool g_BloomEnable = true;

        public static float g_BloomRadius1 = 1.0f;
        public static float g_BloomRadius2 = 1.0f;
        public static float g_BloomRadius3 = 2.0f;
        public static float g_BloomRadius4 = 3.0f;
        public static float g_BloomRadius5 = 4.0f;

        public static float g_BloomStrength1 = 0.5f;
        public static float g_BloomStrength2 = 1;
        public static float g_BloomStrength3 = 1;
        public static float g_BloomStrength4 = 1.0f;
        public static float g_BloomStrength5 = 1.0f;


        public static float ShadowBias = 0.005f;
        public static int sdf_threads = 4;
        public static bool sdf_cpu = false;
        public static bool sdf_draw = true;
        public static bool sdf_drawdistance = false;
        public static bool sdf_debug = false;
        public static bool sdf_subsurface = true;
        public static bool sdf_drawvolume = false;
        public static bool sdf_regenerate;
        public static bool d_drawnothing = false;
        public static bool e_saveBoundingBoxes = true;
        public static bool d_hotreloadshaders = true;

        public static void ApplySettings()
        {
            ApplySSAO();

            g_taa = true;
            g_environmentmapping = true;


            d_defaultmaterial = false;

        }

        public static void ApplySSAO()
        {

        }
    }

    public static class GameStats
    {
        public static int MeshDraws = 0;
        public static int MaterialDraws = 0;
        public static int LightsDrawn = 0;

        public static int shadowMaps = 0;
        public static int activeShadowMaps = 0;
        public static int EmissiveMeshDraws = 0;

        public static long d_profileRenderChanges;
        public static long d_profileDrawShadows;
        public static long d_profileDrawCubeMap;
        public static long d_profileUpdateViewProjection;
        public static long d_profileSetupGBuffer;
        public static long d_profileDrawGBuffer;
        public static long d_profileDrawHolograms;
        public static long d_profileDrawScreenSpaceEffect;
        public static long d_profileDrawScreenSpaceDirectionalShadow;
        public static long d_profileDrawBilateralBlur;
        public static long d_profileDrawLights;
        public static long d_profileDrawEnvironmentMap;
        public static long d_profileDrawEmissive;
        public static long d_profileDrawSSR;
        public static long d_profileCompose;
        public static long d_profileCombineTemporalAntialiasing;
        public static long d_profileDrawFinalRender;
        public static long d_profileTotalRender;
        public static bool UIIsHovered;
        public static bool e_EnableSelection = false;
        //public static EditorLogic.GizmoModes e_gizmoMode = EditorLogic.GizmoModes.Translation;
        public static bool e_LocalTransformation = false;
        public static float sdf_load = 0;
    }
}
