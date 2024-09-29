using System;
using AngeliA;

namespace AngeliaEngine;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false)]
public class EngineSettingAttribute : Attribute {
	public string Group;
	public string DisplayLabel;
	public string RequireSettingName;
	public EngineSettingAttribute () {
		Group = null;
		DisplayLabel = null;
	}
	public EngineSettingAttribute (string group, string displayLabel, string requireSettingPath = "") {
		Group = group;
		DisplayLabel = displayLabel;
		RequireSettingName = requireSettingPath;
	}
}


[EngineSetting]
public class EngineSetting {

	// Engine
	[EngineSetting("Engine", "Open Last Project on Start")] public static readonly SavingBool OpenLastProjectOnStart = new("Engine.OpenLastProjectOnStart", false, SavingLocation.Global);
	[EngineSetting("Engine", "Show Tooltip")] public static readonly SavingBool UseTooltip = new("Engine.UseTooltip", true, SavingLocation.Global);
	[EngineSetting("Engine", "Show Notification")] public static readonly SavingBool UseNotification = new("Engine.UseNotification", true, SavingLocation.Global);
	[EngineSetting("Engine", "Auto Recompile when Script Changed")] public static readonly SavingBool AutoRecompile = new("Engine.AutoRecompile", true, SavingLocation.Global);

	// Pixel Editor
	[EngineSetting("Artwork", "Background Color")] public static readonly SavingColor32NoAlpha BackgroundColor = new("PixEdt.BGColor", new Color32(32, 33, 37, 255), SavingLocation.Global);
	[EngineSetting("Artwork", "Gradient Background")] public static readonly SavingBool GradientBackground = new("PixEdt.GradientBackground", true, SavingLocation.Global);
	[EngineSetting("Artwork", "Solid Painting Preview")] public static readonly SavingBool SolidPaintingPreview = new("PixEdt.SolidPaintingPreview", true, SavingLocation.Global);
	[EngineSetting("Artwork", "Show Preview on Tag Button")] public static readonly SavingBool ShowTagPreview = new("PixEdt.ShowTagPreview", true, SavingLocation.Global);

	// Console
	[EngineSetting("Console", "Show Log Time")] public static readonly SavingBool ShowLogTime = new("Console.ShowLogTime", false, SavingLocation.Global);
	[EngineSetting("Console", "Blink When Having Compile Error")] public static readonly SavingBool BlinkWhenError = new("Console.BlinkWhenError", true, SavingLocation.Global);
	[EngineSetting("Console", "Add Prefix Mark for Messages from Game")] public static readonly SavingBool AddPrefixMarkForMessageFromGame = new("Console.PrefixMark", true, SavingLocation.Global);

	// Map Editor
	[EngineSetting("Map Editor", "Use Map Editor in Engine")] public static readonly SavingBool MapEditor_Enable = new("MapEditor.Enable", true, SavingLocation.Global);
	[EngineSetting("Map Editor", "Drop Player when Release Space Key", nameof(MapEditor_Enable))] public static readonly SavingBool MapEditor_QuickPlayerDrop = new("MapEditor.QuickPlayerDrop", false, SavingLocation.Global);
	[EngineSetting("Map Editor", "Auto Zoom when Editing", nameof(MapEditor_Enable))] public static readonly SavingBool MapEditor_AutoZoom = new("MapEditor.AutoZoom", true, SavingLocation.Global);
	[EngineSetting("Map Editor", "Show State Info on Bottom-Right", nameof(MapEditor_Enable))] public static readonly SavingBool MapEditor_ShowState = new("MapEditor.ShowState", false, SavingLocation.Global);
	[EngineSetting("Map Editor", "Show Map Behind", nameof(MapEditor_Enable))] public static readonly SavingBool MapEditor_ShowBehind = new("MapEditor.ShowBehind", true, SavingLocation.Global);
	[EngineSetting("Map Editor", "Show Gizmos", nameof(MapEditor_Enable))] public static readonly SavingBool MapEditor_ShowGizmos = new("MapEditor.ShowGizmos", true, SavingLocation.Global);
	public static readonly SavingInt LastMapEditorViewX = new("Map.LastMapEditorViewX", 0, SavingLocation.Global);
	public static readonly SavingInt LastMapEditorViewY = new("Map.LastMapEditorViewY", 0, SavingLocation.Global);
	public static readonly SavingInt LastMapEditorViewZ = new("Map.LastMapEditorViewZ", 0, SavingLocation.Global);
	public static readonly SavingInt LastMapEditorViewHeight = new("Map.LastMapEditorViewHeight", -1, SavingLocation.Global);

	// Hotkey
	[EngineSetting("Hotkey", "Recompile")] public static readonly SavingHotkey Hotkey_Recompile = new("Hotkey.Recompile", new Hotkey(KeyboardKey.R, ctrl: true), SavingLocation.Global);
	[EngineSetting("Hotkey", "Run")] public static readonly SavingHotkey Hotkey_Run = new("Hotkey.Run", new Hotkey(KeyboardKey.R, ctrl: true, shift: true), SavingLocation.Global);
	[EngineSetting("Hotkey", "Clear Console")] public static readonly SavingHotkey Hotkey_ClearConsole = new("Hotkey.ClearConsole", new Hotkey(KeyboardKey.C, ctrl: true, shift: true), SavingLocation.Global);
	[EngineSetting("Hotkey", "Open Map Editor")] public static readonly SavingHotkey Hotkey_Window_MapEditor = new("Hotkey.Window.MapEditor", new Hotkey(KeyboardKey.F1), SavingLocation.Global);
	[EngineSetting("Hotkey", "Open Artwork")] public static readonly SavingHotkey Hotkey_Window_Artwork = new("Hotkey.Window.Artwork", new Hotkey(KeyboardKey.F2), SavingLocation.Global);
	[EngineSetting("Hotkey", "Open Language Editor")] public static readonly SavingHotkey Hotkey_Window_Language = new("Hotkey.Window.Language", new Hotkey(KeyboardKey.F3), SavingLocation.Global);
	[EngineSetting("Hotkey", "Open Console")] public static readonly SavingHotkey Hotkey_Window_Console = new("Hotkey.Window.Console", new Hotkey(KeyboardKey.F4), SavingLocation.Global);
	[EngineSetting("Hotkey", "Open Project Editor")] public static readonly SavingHotkey Hotkey_Window_Project = new("Hotkey.Window.Project", new Hotkey(KeyboardKey.F5), SavingLocation.Global);
	[EngineSetting("Hotkey", "Open Package Manager")] public static readonly SavingHotkey Hotkey_Window_Package = new("Hotkey.Window.Package", new Hotkey(KeyboardKey.F6), SavingLocation.Global);
	[EngineSetting("Hotkey", "Open Setting")] public static readonly SavingHotkey Hotkey_Window_Setting = new("Hotkey.Window.Setting", new Hotkey(KeyboardKey.F7), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Rect Tool")] public static readonly SavingHotkey Hotkey_PixTool_Rect = new("Hotkey.Pix.Rect", new Hotkey(KeyboardKey.U), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Circle Tool")] public static readonly SavingHotkey Hotkey_PixTool_Circle = new("Hotkey.Pix.Circle", new Hotkey(KeyboardKey.C), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Line Tool")] public static readonly SavingHotkey Hotkey_PixTool_Line = new("Hotkey.Pix.Line", new Hotkey(KeyboardKey.L), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Bucket Tool")] public static readonly SavingHotkey Hotkey_PixTool_Bucket = new("Hotkey.Pix.Bucket", new Hotkey(KeyboardKey.G), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Select Tool")] public static readonly SavingHotkey Hotkey_PixTool_Select = new("Hotkey.Pix.Select", new Hotkey(KeyboardKey.M), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Sprite Tool")] public static readonly SavingHotkey Hotkey_PixTool_Sprite = new("Hotkey.Pix.Sprite", new Hotkey(KeyboardKey.S), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Prev Palette Color")] public static readonly SavingHotkey Hotkey_Pix_PalettePrev = new("Hotkey.Pix.PalPrev", new Hotkey(KeyboardKey.Q), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Next Palette Color")] public static readonly SavingHotkey Hotkey_Pix_PaletteNext = new("Hotkey.Pix.PalNext", new Hotkey(KeyboardKey.W), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Flip Selection Horizontal")] public static readonly SavingHotkey Hotkey_Pix_FlipX = new("Hotkey.Pix.FlipX", new Hotkey(KeyboardKey.H, shift: true), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Flip Selection Vertical")] public static readonly SavingHotkey Hotkey_Pix_FlipY = new("Hotkey.Pix.FlipY", new Hotkey(KeyboardKey.V, shift: true), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Rotate Selection Clockwise")] public static readonly SavingHotkey Hotkey_Pix_RotC = new("Hotkey.Pix.RotC", new Hotkey(KeyboardKey.W, shift: true), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Rotate Selection Counter-Clockwise")] public static readonly SavingHotkey Hotkey_Pix_RotCC = new("Hotkey.Pix.RotCC", new Hotkey(KeyboardKey.Q, shift: true), SavingLocation.Global);
	[EngineSetting("Hotkey", "Frame Debug - Next Frame")] public static readonly SavingHotkey Hotkey_FrameDebug_Next = new("Hotkey.FrameDebug.Next", new Hotkey(KeyboardKey.Period), SavingLocation.Global);

}