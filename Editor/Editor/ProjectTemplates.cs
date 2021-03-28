using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SE.Utility;

namespace SE.Editor
{
    public static class ProjectTemplateManager
    {
        private static QuickList<ProjectTemplate> projectTemplates;

        /// <summary>Directory in which the templates are stored.</summary>
        public static string ProjectTemplatesDirectory { get;} = Path.Combine(EditorApp.BaseDirectory, "ProjectTemplates");
        
        /// <summary>Directory which contains SE binaries to be included in builds.</summary>
        public static string BuildLibrariesDirectory { get; } = Path.Combine(EditorApp.BaseDirectory, "BuildLibraries");

        public static ProjectTemplate Default { get; } = new ProjectTemplate(Path.Combine(ProjectTemplatesDirectory, "Default"));

        /// <summary>
        /// Initializes all the templates located in the ProjectTemplates folder.
        /// </summary>
        internal static void InitializeTemplates()
        {
            projectTemplates = new QuickList<ProjectTemplate>();
            foreach (string dirString in Directory.GetDirectories(ProjectTemplatesDirectory)) {
                try {
                    projectTemplates.Add(new ProjectTemplate(dirString));
                } catch (Exception) {
                    // TODO: Print error and continue;
                }
            }
        }

        /// <summary>
        /// Creates a folder for a new project, based on an existing template.
        /// </summary>
        internal static Project CreateProjectFromTemplate(string projectDirectory, ProjectTemplate template)
        {
            // TODO: Create, THEN load the project.
            return null;
        }
    }

    public class ProjectTemplate
    {
        public string BaseDirectory { get; }

        public ProjectTemplate(string baseDirectory)
        {
            BaseDirectory = baseDirectory;
        }
    }
}
