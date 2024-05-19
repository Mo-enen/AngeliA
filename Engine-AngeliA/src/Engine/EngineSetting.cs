using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public class EngineSetting {

	// Engine
	public static readonly SavingBool OpenLastProjectOnStart = new("Engine.OpenLastProjectOnStart", false);
	public static readonly SavingBool UseTooltip = new("Engine.UseTooltip", true);
	public static readonly SavingBool UseNotification = new("Engine.UseNotification", true);

	// Pixel Editor
	public static readonly SavingColor32 BackgroundColor = new("PixEdt.BGColor", new Color32(32, 33, 37, 255));
	public static readonly SavingBool SolidPaintingPreview = new("PixEdt.SolidPaintingPreview", true);

	// Console
	public static readonly SavingBool ShowLogTime = new("Console.ShowLogTime", false);


}