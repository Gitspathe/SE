using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Numerics;
using ImGuiNET;
using SE.Core;
using Console = System.Console;

namespace SE.Editor.GUI.Windows
{
    public class IntroWindow : GUIObject
    {
        private bool fileDialogueOpened = true;

        private Vector2 screenSize;
        private Vector2 sidePanelSize;
        private Vector2 mainPanelSize;

        private OpenedMenu curMenu;
        private Submenu curSubmenu;

        private string projectName = "New Project";
        private string projectDirectory = ProjectManager.DefaultProjectsDirectory;

        public override void OnPaint()
        {
            screenSize = new Vector2(Screen.SizeX, Screen.SizeY);
            sidePanelSize = new Vector2(screenSize.X * 0.333f, screenSize.Y);
            mainPanelSize = new Vector2(screenSize.X * 0.66667f, screenSize.Y);

            GUI.SetNextWindowPos(new Vector2(0.0f, 0.0f));
            GUI.SetNextWindowSize(screenSize);
            GUI.Begin("Hub", GUIWindowFlags.NoScrollbar
                             | GUIWindowFlags.NoCollapse
                             | GUIWindowFlags.NoResize
                             | GUIWindowFlags.NoMove
                             | GUIWindowFlags.NoScrollWithMouse
                             | GUIWindowFlags.NoTitleBar);

            switch (curMenu) {
                case OpenedMenu.Main:
                    DrawSidePanel();
                    DrawMainPanel();
                    break;
                case OpenedMenu.CreateProject:
                    DrawCreateProjectMenu();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GUI.End();
        }

        private void DrawCreateProjectMenu()
        {
            const float paddingH = 0.15f;
            const float paddingY = 0.04f;
            const float openButtonWidth = 80;
            string title = "CREATE PROJECT";
            Vector2 titleSize = GUI.CalcTextSize(title);

            Vector2 innerMenuPos = new Vector2((screenSize.X * paddingH), (screenSize.Y * paddingY));
            Vector2 innerMenuSize = new Vector2(screenSize.X - (screenSize.X * paddingH * 2), screenSize.Y - (screenSize.Y * paddingY * 2));

            GUI.SetNextWindowPos(innerMenuPos);
            GUI.BeginChild("##createProjectInnerPanel", 
                innerMenuSize, 
                true, 
                GUIWindowFlags.None);

            if (GUI.Button("< Back", new Vector2(70, 25))) {
                curMenu = OpenedMenu.Main;
            }

            GUI.SameLine((GUI.GetWindowWidth()/2) - (80 + (titleSize.X)));
            GUI.Text(title);
           
            GUI.InputText("##projName", ref projectName, "Project name:");

            GUI.BeginChild("##projDirectoryOpen", new Vector2(innerMenuSize.X - openButtonWidth, ImGui.GetFrameHeight()));
            GUI.InputText("##projDirectory", ref projectDirectory, "Containing directory:");
            GUI.EndChild();
            GUI.SameLine();
            if (GUI.Button("Open", new Vector2(openButtonWidth, 0))) {
                ImGui.OpenPopup("CreateNewProject_BrowserPopup");
            }

            GUI.Text($"Final directory: {projectDirectory}\\{projectName}\\");

            // TODO: Templates.

            if (GUI.Button("CREATE", new Vector2(100, 65))) {
                ProjectConfig projConfig = ProjectConfig.Default;
                projConfig.Name = projectName;
                ProjectManager.CreateNewProject(projectDirectory, projConfig, ProjectTemplateManager.Default);
            }

            // Open folder popup.
            if (ImGui.BeginPopupModal("CreateNewProject_BrowserPopup", ref fileDialogueOpened, ImGuiWindowFlags.NoTitleBar)) {
                FilePicker picker = FilePicker.GetFolderPicker(this, ProjectManager.DefaultProjectsDirectory);
                if (picker.Draw()) {
                    projectDirectory = picker.SelectedFile;
                    FilePicker.RemoveFilePicker(this);
                }
                ImGui.EndPopup();
            }

            GUI.End();

            //GUI.EndChild();
        }

        private void DrawSidePanel()
        {
            const int sizeY = 50;

            GUI.SetNextWindowPos(new Vector2(0.0f, 0.0f));
            GUI.BeginChild("##sidePanel",
                sidePanelSize,
                true,
                GUIWindowFlags.None);

            if (GUI.Button("Projects", new Vector2(sidePanelSize.X, sizeY))) {
                curSubmenu = Submenu.Projects;
            }
            if (GUI.Button("Documentation", new Vector2(sidePanelSize.X, sizeY))) {
                curSubmenu = Submenu.Documentation;
            }
            if (GUI.Button("Blah blah", new Vector2(sidePanelSize.X, sizeY))) {
                curSubmenu = Submenu.Test1;
            }

            GUI.EndChild();
        }

        private void DrawMainPanel()
        {
            const int sizeY = 50;

            GUI.SetNextWindowPos(new Vector2(sidePanelSize.X, 0.0f));
            GUI.BeginChild("##mainPanel",
                mainPanelSize,
                true,
                GUIWindowFlags.None);

            switch (curSubmenu) {
                case Submenu.Projects:
                    DrawProjectsMenu();
                    break;
                case Submenu.Documentation: 
                    break;
                case Submenu.Test1: 
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GUI.EndChild();
        }

        private void DrawProjectsMenu()
        {
            Vector2 bannerSize = new Vector2(mainPanelSize.X - sidePanelSize.X, 80);

            GUI.SetNextWindowPos(new Vector2(sidePanelSize.X, 0.0f));
            GUI.BeginChild("##projects##banner",
                bannerSize);

            GUI.Text("PROJECTS");
            GUI.SameLine();

            if (GUI.Button("Create new project", new Vector2(150, 40))) {
                curMenu = OpenedMenu.CreateProject;
            }

            GUI.EndChild();
        }

        private enum OpenedMenu
        {
            Main,
            CreateProject
        }

        private enum Submenu
        {
            Projects,
            Documentation,
            Test1
        }
    }
}
