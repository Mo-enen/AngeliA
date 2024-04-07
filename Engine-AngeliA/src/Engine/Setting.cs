using System.Collections;
using System.Collections.Generic;
using System;
using AngeliA;

namespace AngeliaEngine;

[Serializable]
public class EngineSetting {

	[Serializable]
	public class ProjectData {
		public string Path;
		public bool FolderExists;
	}

	public bool Maximize = true;
	public bool FullsizeMenu = true;
	public int WindowSizeX = 1024;
	public int WindowSizeY = 1024;
	public int WindowPositionX = 128;
	public int WindowPositionY = 128;
	public string LastOpenProject = "";
	public bool OpenLastProjectOnStart = false;
	public List<ProjectData> Projects = new();

	public void RefreshProjectFileExistsCache () {
		foreach (var project in Projects) {
			project.FolderExists = Util.FolderExists(project.Path);
		}
	}

}