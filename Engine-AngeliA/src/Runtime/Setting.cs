using System.Collections;
using System.Collections.Generic;
using System;

namespace AngeliaEngine;

[Serializable]
public class EngineSetting {

	public bool Maximize = true;
	public bool FullsizeMenu = true;
	public int FloatX = 0;
	public int FloatY = 0;
	public int WindowSizeX = 1024;
	public int WindowSizeY = 1024;
	public int WindowPositionX = 128;
	public int WindowPositionY = 128;
	public List<string> Projects = new();

}