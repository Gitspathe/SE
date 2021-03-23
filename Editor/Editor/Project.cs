using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SE.Core;
using SE.Serialization;
using SE.Serialization.Attributes;

namespace SE.Editor
{
    public static class ProjectManager
    {
        internal static Project LoadProject(string projectDirectory)
        {
            Project proj = new Project(projectDirectory);
            // ... Load the project ...
            // ...
            // ...

            // Set it to the currently loaded project.
            Project.Current = proj;
            return proj;
        }
    }

    public sealed class Project
    {
        public string Name => ProjectConfig.Name;

        public ProjectConfig ProjectConfig { get; }

        public string ProjectDirectory { get; }
        public string AssetsDirectory { get; }
        public string CodeDirectory { get; }
        public string CacheDirectory { get; }
        public string SettingsDirectory { get; }

        /// <summary>True if the project is currently used by the editor.</summary>
        public bool IsCurrent => Current == this;

        public static Project Current { get; internal set; }

        internal Project(string projectDirectory)
        {
            const string ext = FileIO.SEFileExtensions._CONFIG;
            ProjectDirectory = projectDirectory;
            
            // SSF = SE Serialization Format
            ProjectConfig = ProjectConfig.FromConfigFile(ProjectDirectory + "ProjectConfig" + ext);
            AssetsDirectory = Path.Combine(projectDirectory, ProjectConfig.AssetsDirectory);
            CodeDirectory = Path.Combine(projectDirectory, ProjectConfig.CodeDirectory);
            CacheDirectory = Path.Combine(projectDirectory, ProjectConfig.CacheDirectory);
            SettingsDirectory = Path.Combine(projectDirectory, ProjectConfig.SettingsDirectory);
        }
    }

    [SerializeObject]
    public sealed class ProjectConfig
    {
        public string Name { get; set; } = "a";

        public string AssetsDirectory { get; set; } = "a";
        public string CodeDirectory { get; set; } = "a";
        public string CacheDirectory { get; set; } = "a";
        public string SettingsDirectory { get; set; } = "a";

        private static SerializerSettings configSerializationSettings = new SerializerSettings {
            Formatting = Formatting.Text,
            NullValueHandling = NullValueHandling.DefaultValue,
            DefaultValueHandling = DefaultValueHandling.Serialize,
            ConvertBehaviour = ConvertBehaviour.Order,
            TypeHandling = TypeHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 10,
            UseHeader = false
        };

        public ProjectConfig FromConfigFile(string configFile)
        {
            byte[] projConfigBytes = FileIO.ReadFileBytes(configFile);
            return Serializer.Deserialize<ProjectConfig>(projConfigBytes, configSerializationSettings);
        }
    }
}
