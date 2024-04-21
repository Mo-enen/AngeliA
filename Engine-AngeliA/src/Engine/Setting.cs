using System.Collections;
using System.Collections.Generic;
using System;
using AngeliA;

namespace AngeliaEngine;

[Serializable]
public class EngineSetting {

	[Serializable]
	public class ProjectData {
		public string Name;
		public string Path;
		public bool FolderExists;
		public long LastEditTime;
	}

	public enum ProjectSortMode { Name, OpenTime, }

	public bool Maximize = true;
	public bool FullsizeMenu = true;
	public int WindowSizeX = 1024;
	public int WindowSizeY = 1024;
	public int WindowPositionX = 128;
	public int WindowPositionY = 128;
	public string LastOpenProject = "";
	public bool OpenLastProjectOnStart = false;
	public bool UseTooltip = true;
	public bool UseNotification = true;
	public ProjectSortMode ProjectSort = ProjectSortMode.OpenTime;
	public List<ProjectData> Projects = new();

	public void RefreshProjectCache () {
		foreach (var project in Projects) {
			project.FolderExists = Util.FolderExists(project.Path);
		}
	}

	public void SortProjects () {
		switch (ProjectSort) {
			case ProjectSortMode.Name:
				Projects.Sort((a, b) => a.Name.CompareTo(b.Name));
				break;
			case ProjectSortMode.OpenTime:
				Projects.Sort((a, b) => b.LastEditTime.CompareTo(a.LastEditTime));
				break;
		}
	}

}