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
        public static string DefaultProjectsDirectory { get; } = Path.Combine(EditorApp.BaseDirectory, "Projects");

        internal static Project LoadProject(string projectDirectory)
        {
            Project proj = new Project(projectDirectory);
            // ........................
            // ... Load the project ...
            // ........................

            // Set it to the currently loaded project.
            Project.Current = proj;
            return proj;
        }

        internal static Project CreateNewProject(string containingDirectory, ProjectConfig config, ProjectTemplate template)
        {
            string projDir = Path.Combine(containingDirectory, config.Name);
            char sep = Path.DirectorySeparatorChar;
            
            // The directory MUST not exist. Don't want to overwrite system files or some crazy shit.
            if(Directory.Exists(projDir))
                throw new IOException("Directory already exists!");

            // Create directories, and save the ProjectConfig.
            Directory.CreateDirectory(projDir);
            config.SaveToDirectory(projDir);

            return new Project(projDir);
        }
    }

    public sealed class Project
    {
        public string Name => ProjectConfig.Name;

        public ProjectConfig ProjectConfig { get; }

        public string ProjectBaseDirectory { get; }
        public string BuildDirectory { get; }
        public string AssetsDirectory { get; }
        public string CodeDirectory { get; }
        public string CacheDirectory { get; }
        public string SettingsDirectory { get; }

        /// <summary>True if the project is currently used by the editor.</summary>
        public bool IsCurrent => Current == this;

        public static Project Current { get; internal set; }

        internal Project(string projectDirectory)
        {
            char sep = Path.DirectorySeparatorChar;
            ProjectBaseDirectory = projectDirectory;

            string s = projectDirectory + sep + ProjectConfig.ProjectConfigFileName;

            ProjectConfig = ProjectConfig.FromConfigFile(projectDirectory + sep + ProjectConfig.ProjectConfigFileName);
            BuildDirectory = Path.Combine(projectDirectory, ProjectConfig.BuildDirectory);
            AssetsDirectory = Path.Combine(projectDirectory, ProjectConfig.AssetsDirectory);
            CodeDirectory = Path.Combine(projectDirectory, ProjectConfig.CodeDirectory);
            CacheDirectory = Path.Combine(projectDirectory, ProjectConfig.CacheDirectory);
            SettingsDirectory = Path.Combine(projectDirectory, ProjectConfig.SettingsDirectory);
        }
    }

    [SerializeObject]
    public sealed class ProjectConfig
    {
        public string Name { get; set; }

        public string BuildDirectory { get; set; } = "Build";
        public string AssetsDirectory { get; set; } = "Assets";
        public string CodeDirectory { get; set; } = "Code";
        public string CacheDirectory { get; set; } = "Cache";
        public string SettingsDirectory { get; set; } = "Settings";

        public static string ProjectConfigFileName { get; } = "ProjectConfig" + FileIO.SEFileExtensions._CONFIG;

        private static SerializerSettings configSerializationSettings = new SerializerSettings {
            Formatting = Formatting.Text,
            NullValueHandling = NullValueHandling.DefaultValue,
            DefaultValueHandling = DefaultValueHandling.Serialize,
            ConvertBehaviour = ConvertBehaviour.Configuration,
            TypeHandling = TypeHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 10,
            UseHeader = false
        };

        public static ProjectConfig Default { get; } = new ProjectConfig();

        public void SaveToDirectory(string projectDirectory)
        {
            byte[] data = Serializer.Serialize(this, configSerializationSettings).ToArray();
            FileIO.SaveFile(data, projectDirectory + Path.DirectorySeparatorChar + ProjectConfigFileName);
        }

        public static ProjectConfig FromConfigFile(string configFile)
        {
            byte[] data = FileIO.ReadFileBytes(configFile);
            return Serializer.Deserialize<ProjectConfig>(data, configSerializationSettings);
        }
    }
}
