using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using MonoGame.Framework.Content.Pipeline.Builder;
using System;
using System.Collections.Generic;
using System.IO;

namespace EffectsCompiler
{
    class Program
    {
        private static string shaderDir { get; set; }
        private static string compiledDir { get; set; }

        private static Dictionary<string, byte[]> data = new Dictionary<string, byte[]>();

        static void Main(string[] args)
        {
            if (args.Length < 2) {
                Console.WriteLine("Usage: [input directory] [output directory]");
                return;
            }

            shaderDir = args[0];
            compiledDir = args[1];

            Console.WriteLine("Initializing.");
            if (!Directory.Exists(shaderDir)) {
                Directory.CreateDirectory(shaderDir);
            }
            if (!Directory.Exists(compiledDir)) {
                Directory.CreateDirectory(compiledDir);
            }

            Console.WriteLine("Clearing existing.");
            DirectoryInfo shaderDirInfo = new DirectoryInfo(shaderDir);
            DirectoryInfo compiledDirInfo = new DirectoryInfo(compiledDir);

            foreach (FileInfo fi in shaderDirInfo.GetFiles()) {
                if (fi.Extension != ".fx")
                    continue;

                try {
                    string name = fi.Name.Remove(fi.Name.Length - 3, 3);
                    data.Add(name + ".ogl", Compile(fi.FullName, Platform.OpenGL));
                    data.Add(name + ".d3d", Compile(fi.FullName, Platform.DirectX));
                } catch (Exception e) {
                    Console.Write(e.ToString());
                }
            }

            foreach (KeyValuePair<string, byte[]> val in data) {
                string name = val.Key;
                string path = compiledDir + Path.DirectorySeparatorChar + name;
                if (File.Exists(path)) {
                    File.Delete(path);
                }
                File.WriteAllBytes(path, val.Value);
            }

            foreach (FileInfo file in compiledDirInfo.GetFiles()) {
                if (!data.ContainsKey(file.Name)) {
                    File.Delete(file.FullName);
                }
            }
        }

        private static byte[] Compile(string filePath, Platform platform)
        {
            // TODO: Use mgfxc directly instead of having a 200mb dependency.
            EffectImporter importer = new EffectImporter();
            EffectContent content = importer.Import(filePath, null);
            EffectProcessor processor = new EffectProcessor();
            PipelineManager pm;
            pm = new PipelineManager(string.Empty, string.Empty, string.Empty);

            switch (platform) {
                case Platform.DirectX:
                    pm.Platform = TargetPlatform.Windows;
                    break;
                case Platform.OpenGL:
                    pm.Platform = TargetPlatform.DesktopGL;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            PipelineProcessorContext ppc = new PipelineProcessorContext(pm, new PipelineBuildEvent());
            CompiledEffectContent cecontent = processor.Process(content, ppc);
            ContentCompiler compiler = new ContentCompiler();
            return cecontent.GetEffectCode();
        }

        private enum Platform
        {
            DirectX,
            OpenGL
        }
    }
}
