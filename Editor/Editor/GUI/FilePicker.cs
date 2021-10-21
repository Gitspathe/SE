using System;
using System.Collections.Generic;
using System.IO;
using ImGuiNET;
using Vector2 = System.Numerics.Vector2;
using Microsoft.Xna.Framework;
using SE.Utility;

namespace SE.Editor.GUI
{
	public class FilePicker
	{
		private static readonly Dictionary<object, FilePicker> filePickers = new Dictionary<object, FilePicker>();

		public string RootFolder;
		public string CurrentFolder;
		public string SelectedFile;
		public QuickList<string> AllowedExtensions;
		public bool OnlyAllowFolders;

		public static FilePicker GetFolderPicker(object o, string startingPath)
			=> GetFilePicker(o, startingPath, null, true);

		public static FilePicker GetFilePicker(object o, string startingPath, string searchFilter = null, bool onlyAllowFolders = false)
		{
			if (File.Exists(startingPath)) {
				startingPath = new FileInfo(startingPath).DirectoryName;
			} else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath)) {
				startingPath = Environment.CurrentDirectory;
                if (string.IsNullOrEmpty(startingPath)) {
                    startingPath = AppContext.BaseDirectory;
                }
			}

            if (filePickers.TryGetValue(o, out FilePicker fp)) 
                return fp;

            fp = new FilePicker {
                RootFolder = startingPath, 
                CurrentFolder = startingPath, 
                OnlyAllowFolders = onlyAllowFolders
            };

            if (searchFilter != null) {
                if (fp.AllowedExtensions != null) {
                    fp.AllowedExtensions.Clear();
                } else {
                    fp.AllowedExtensions = new QuickList<string>();
                }
                fp.AllowedExtensions.AddRange(searchFilter.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            }

            filePickers.Add(o, fp);
            return fp;
		}

		public static void RemoveFilePicker(object o) => filePickers.Remove(o);

		public bool Draw()
		{
			ImGui.Text("Current Folder: " + Path.GetFileName(RootFolder) + CurrentFolder.Replace(RootFolder, ""));
			bool result = false;

			if (ImGui.BeginChildFrame(1, new Vector2(400, 400))) {
				DirectoryInfo dirInfo = new DirectoryInfo(CurrentFolder);
				if (dirInfo.Exists) {
					if (dirInfo.Parent != null && CurrentFolder != RootFolder) {
						ImGui.PushStyleColor(ImGuiCol.Text, Color.Yellow.PackedValue);
                        if (ImGui.Selectable("../", false, ImGuiSelectableFlags.DontClosePopups)) {
                            CurrentFolder = dirInfo.Parent.FullName;
                        }

						ImGui.PopStyleColor();
					}

					QuickList<string> fileSystemEntries = GetFileSystemEntries(dirInfo.FullName);
					foreach (string fse in fileSystemEntries) {
						if (Directory.Exists(fse)) {
							string name = Path.GetFileName(fse);
							ImGui.PushStyleColor(ImGuiCol.Text, Color.Yellow.PackedValue);
							if (ImGui.Selectable(name + "/", false, ImGuiSelectableFlags.DontClosePopups)) {
                                CurrentFolder = fse;
                            }
							ImGui.PopStyleColor();
						} else {
							string name = Path.GetFileName(fse);
							bool isSelected = SelectedFile == fse;
							if (ImGui.Selectable(name, isSelected, ImGuiSelectableFlags.DontClosePopups)) {
                                SelectedFile = fse;
                            }
                            if (!ImGui.IsMouseDoubleClicked(0)) 
                                continue;

                            result = true;
                            ImGui.CloseCurrentPopup();
                        }
					}
				}
			}
			ImGui.EndChildFrame();

            if (ImGui.Button("Cancel")) {
				result = false;
				ImGui.CloseCurrentPopup();
			}
            if (OnlyAllowFolders) {
				ImGui.SameLine();
                if (!ImGui.Button("Open")) 
                    return result;

                result = true;
                SelectedFile = CurrentFolder;
                ImGui.CloseCurrentPopup();
            }
			else if (SelectedFile != null) {
				ImGui.SameLine();
                if (!ImGui.Button("Open"))
                    return result;

                result = true;
                ImGui.CloseCurrentPopup();
            }
            return result;
		}

		private bool TryGetFileInfo(string fileName, out FileInfo realFile)
		{
			try {
				realFile = new FileInfo(fileName);
				return true;
			} catch {
				realFile = null;
				return false;
			}
		}

        private QuickList<string> GetFileSystemEntries(string fullName)
		{
			QuickList<string> files = new QuickList<string>();
			QuickList<string> dirs = new QuickList<string>();
            foreach (string fse in Directory.GetFileSystemEntries(fullName, "")) {
				if (Directory.Exists(fse)) {
					dirs.Add(fse);
				}
				else if (!OnlyAllowFolders) {
					if (AllowedExtensions != null) {
						string ext = Path.GetExtension(fse);
                        if (AllowedExtensions.Contains(ext)) {
                            files.Add(fse);
						}
					} else {
						files.Add(fse);
					}
				}
			}

			QuickList<string> ret = new QuickList<string>();
            ret.AddRange(dirs);
			ret.AddRange(files);
            return ret;
		}

	}
}