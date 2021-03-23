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

        public static string ProjectTemplatesDirectory { get; private set; }

        /// <summary>
        /// Initializes all the templates located in the ProjectTemplates folder.
        /// </summary>
        internal static void InitializeTemplates()
        {
            ProjectTemplatesDirectory = Path.Combine(ProjectTemplatesDirectory, "ProjectTemplates");

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
