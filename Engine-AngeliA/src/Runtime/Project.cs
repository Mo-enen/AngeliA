using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public class Project {


	public bool IsValid => !string.IsNullOrEmpty(ProjectPath);
	public string ProjectPath { get; private set; } = "";


	public void Load (string path) {
		ProjectPath = "";
		if (Util.FolderExists(path)) {
			ProjectPath = path;
		}
	}


}
