using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.Json.Serialization;
using Raylib_cs;
using AngeliA;

namespace AngeliaEngine;

[Serializable]
public class EngineSetting {

	public bool WindowMode = true;
	public bool Maximize = true;
	public int FloatX = 0;
	public int FloatY = 0;
	public int WindowSizeX = 1024;
	public int WindowSizeY = 1024;
	public int WindowPositionX = 128;
	public int WindowPositionY = 128;
	
	[JsonIgnore] public bool Initialized = false;

	public void LoadValueFromWindow () {
		if (Raylib.IsWindowState(ConfigFlags.UndecoratedWindow)) return;
		var windowPos = Raylib.GetWindowPosition();
		Maximize = Raylib.IsWindowMaximized();
		WindowSizeX = Raylib.GetRenderWidth();
		WindowSizeY = Raylib.GetRenderHeight();
		WindowPositionX = windowPos.X.RoundToInt();
		WindowPositionY = windowPos.Y.RoundToInt();
	}

}