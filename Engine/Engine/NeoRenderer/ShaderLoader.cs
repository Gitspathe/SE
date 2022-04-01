using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace SE.NeoRenderer
{

    // TODO: Eventually this should be more fancy and should properly support subdirectories etc.

    internal static class ShaderLoader
    {
        private static string ShaderDirectory { get; } = Path.Combine(FileIO.DataDirectory, "_MAIN", "Shader");

        private static ShaderProfile profile;
        private static bool initialized;

        internal static void Initialize()
        {
            if (initialized)
                return;

            Assembly mgAssembly = Assembly.GetAssembly(typeof (Game));
            Type shaderType = mgAssembly.GetType("Microsoft.Xna.Framework.Graphics.Shader");
            PropertyInfo profileProperty = shaderType.GetProperty("Profile");
            int value = (int) profileProperty.GetValue(null);
            
            profile = value == 1 ? ShaderProfile.DirectX : ShaderProfile.OpenGL;

            if (!Directory.Exists(ShaderDirectory)) {
                Directory.CreateDirectory(ShaderDirectory);
            }

            initialized = true;
        }

        internal static Effect LoadEffect(string shaderName)
        {
            if (!initialized)
                throw new Exception();

            string append = profile == ShaderProfile.OpenGL ? ".ogl" : ".d3d";
            string cachePathAbsolute = Path.Combine(ShaderDirectory, shaderName + append);
            if (File.Exists(cachePathAbsolute)) {
                return new Effect(GameEngine.Engine.GraphicsDevice, File.ReadAllBytes(cachePathAbsolute));
            }

            throw new Exception();
        }

        // TODO: Quick and dirty bandaid. Should figure out how Effect vs SpriteEFfect will work.
        internal static SpriteEffect LoadSpriteEffect(string shaderName)
        {
            if (!initialized)
                throw new Exception();

            string append = profile == ShaderProfile.OpenGL ? ".ogl" : ".d3d";
            string cachePathAbsolute = Path.Combine(ShaderDirectory, shaderName + append);
            if (File.Exists(cachePathAbsolute)) {
                return new SpriteEffect(GameEngine.Engine.GraphicsDevice, File.ReadAllBytes(cachePathAbsolute));
            }

            throw new Exception();
        }

        private enum ShaderProfile
        {
            DirectX,
            OpenGL
        }
    }
}
