using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.Json.Serialization;
using AngeliA;
using AngeliA.Framework;

namespace AngeliaEngine;

[Serializable]
public class Setting {

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
		if (!Game.IsWindowDecorated) return;
		var windowPos = Game.GetWindowPosition();
		Maximize = Game.IsWindowMaximized;
		WindowSizeX = Game.ScreenWidth;
		WindowSizeY = Game.ScreenHeight;
		WindowPositionX = windowPos.x;
		WindowPositionY = windowPos.y;
	}

}