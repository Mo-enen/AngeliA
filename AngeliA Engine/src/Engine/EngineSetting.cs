using AngeliA;

namespace AngeliaEngine;


[EngineSetting]
internal class EngineSetting {

	// Engine
	[EngineSetting("Engine", "Open Last Project on Start")] public static readonly SavingBool OpenLastProjectOnStart = new("Engine.OpenLastProjectOnStart", false, SavingLocation.Global);
	[EngineSetting("Engine", "Show Tooltip")] public static readonly SavingBool UseTooltip = new("Engine.UseTooltip", true, SavingLocation.Global);
	[EngineSetting("Engine", "Show Notification")] public static readonly SavingBool UseNotification = new("Engine.UseNotification", true, SavingLocation.Global);
	[EngineSetting("Engine", "Auto Recompile when Script Changed", gameOnly: true)] public static readonly SavingBool AutoRecompile = new("Engine.AutoRecompile", true, SavingLocation.Global);
	[EngineSetting("Engine", "Show Key Press UI when Frame Debugging", gameOnly: true)] public static readonly SavingBool ShowKeyPressWhenFrameDebugging = new("Engine.ShowKeyFrameDebug", true, SavingLocation.Global);

	// Pixel Editor
	[EngineSetting("Artwork", "Background Color")] public static readonly SavingColor32NoAlpha BackgroundColor = new("PixEdt.BGColor", new Color32(32, 33, 37, 255), SavingLocation.Global);
	[EngineSetting("Artwork", "Gradient Background")] public static readonly SavingBool GradientBackground = new("PixEdt.GradientBackground", true, SavingLocation.Global);
	[EngineSetting("Artwork", "Solid Painting Preview")] public static readonly SavingBool SolidPaintingPreview = new("PixEdt.SolidPaintingPreview", true, SavingLocation.Global);
	[EngineSetting("Artwork", "Show Preview on Tag Button")] public static readonly SavingBool ShowTagPreview = new("PixEdt.ShowTagPreview", true, SavingLocation.Global);
	[EngineSetting("Artwork", "Always Expand Painting Color Field")] public static readonly SavingBool AlwaysExpandPaintingColor = new("PixEdt.AlwaysExpandPaintingColor", false, SavingLocation.Global);
	[EngineSetting("Artwork", "Show Marks for Sprites Outside Canvas")] public static readonly SavingBool ShowOutBoundsMark = new("PixEdt.ShowOutBoundsMark", true, SavingLocation.Global);

	// Console
	[EngineSetting("Console", "Show Log Time", gameOnly: true)] public static readonly SavingBool ShowLogTime = new("Console.ShowLogTime", false, SavingLocation.Global);
	[EngineSetting("Console", "Blink When Having Compile Error", gameOnly: true)] public static readonly SavingBool BlinkWhenError = new("Console.BlinkWhenError", true, SavingLocation.Global);
	[EngineSetting("Console", "Add Prefix Mark for Messages from Game", gameOnly: true)] public static readonly SavingBool AddPrefixMarkForMessageFromGame = new("Console.PrefixMark", true, SavingLocation.Global);

	// Map Editor
	[EngineSetting("MapEditor", "Start Play when Release Space Key", gameOnly: true)] public static readonly SavingBool MapEditor_QuickPlayerDrop = new("MapEditor.QuickPlayerDrop", false, SavingLocation.Global);
	[EngineSetting("MapEditor", "Show State Info on Bottom-Right", gameOnly: true)] public static readonly SavingBool MapEditor_ShowState = new("MapEditor.ShowState", true, SavingLocation.Global);
	[EngineSetting("MapEditor", "Show Map Behind", gameOnly: true)] public static readonly SavingBool MapEditor_ShowBehind = new("MapEditor.ShowBehind", true, SavingLocation.Global);
	[EngineSetting("MapEditor", "Show Gizmos", gameOnly: true)] public static readonly SavingBool MapEditor_ShowGizmos = new("MapEditor.ShowGizmos", true, SavingLocation.Global);

	// Hotkey
	[EngineSetting("Hotkey", "Recompile", gameOnly: true)] public static readonly SavingHotkey Hotkey_Recompile = new("Hotkey.Recompile", new Hotkey(KeyboardKey.R, ctrl: true), SavingLocation.Global);
	[EngineSetting("Hotkey", "Clear Console", gameOnly: true)] public static readonly SavingHotkey Hotkey_ClearConsole = new("Hotkey.ClearConsole", new Hotkey(KeyboardKey.C, ctrl: true, shift: true), SavingLocation.Global);
	[EngineSetting("Hotkey", "Open Map Editor", gameOnly: true)] public static readonly SavingHotkey Hotkey_Window_MapEditor = new("Hotkey.Window.MapEditor", new Hotkey(KeyboardKey.F1), SavingLocation.Global);
	[EngineSetting("Hotkey", "Open Artwork")] public static readonly SavingHotkey Hotkey_Window_Artwork = new("Hotkey.Window.Artwork", new Hotkey(KeyboardKey.F2), SavingLocation.Global);
	[EngineSetting("Hotkey", "Open Language Editor", gameOnly: true)] public static readonly SavingHotkey Hotkey_Window_Language = new("Hotkey.Window.Language", new Hotkey(KeyboardKey.F3), SavingLocation.Global);
	[EngineSetting("Hotkey", "Open Console", gameOnly: true)] public static readonly SavingHotkey Hotkey_Window_Console = new("Hotkey.Window.Console", new Hotkey(KeyboardKey.F4), SavingLocation.Global);
	[EngineSetting("Hotkey", "Open Project Editor")] public static readonly SavingHotkey Hotkey_Window_Project = new("Hotkey.Window.Project", new Hotkey(KeyboardKey.F5), SavingLocation.Global);
	[EngineSetting("Hotkey", "Open Package Manager")] public static readonly SavingHotkey Hotkey_Window_Package = new("Hotkey.Window.Package", new Hotkey(KeyboardKey.F6), SavingLocation.Global);
	[EngineSetting("Hotkey", "Open Setting")] public static readonly SavingHotkey Hotkey_Window_Setting = new("Hotkey.Window.Setting", new Hotkey(KeyboardKey.F7), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Rect Tool")] public static readonly SavingHotkey h_Tool_Rect = new("Hotkey.Pix.Rect", new Hotkey(KeyboardKey.U), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Circle Tool")] public static readonly SavingHotkey h_Tool_Circle = new("Hotkey.Pix.Circle", new Hotkey(KeyboardKey.C), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Line Tool")] public static readonly SavingHotkey h_Tool_Line = new("Hotkey.Pix.Line", new Hotkey(KeyboardKey.L), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Bucket Tool")] public static readonly SavingHotkey h_Tool_Bucket = new("Hotkey.Pix.Bucket", new Hotkey(KeyboardKey.G), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Select Tool")] public static readonly SavingHotkey h_Tool_Select = new("Hotkey.Pix.Select", new Hotkey(KeyboardKey.M), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Sprite Tool")] public static readonly SavingHotkey h_Tool_Sprite = new("Hotkey.Pix.Sprite", new Hotkey(KeyboardKey.S), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Prev Palette Color")] public static readonly SavingHotkey h_PalettePrev = new("Hotkey.Pix.PalPrev", new Hotkey(KeyboardKey.Digit1), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Next Palette Color")] public static readonly SavingHotkey h_PaletteNext = new("Hotkey.Pix.PalNext", new Hotkey(KeyboardKey.Digit2), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Flip Selection Horizontal")] public static readonly SavingHotkey h_FlipX = new("Hotkey.Pix.FlipX", new Hotkey(KeyboardKey.H, shift: true), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Flip Selection Vertical")] public static readonly SavingHotkey h_FlipY = new("Hotkey.Pix.FlipY", new Hotkey(KeyboardKey.V, shift: true), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Rotate Selection Clockwise")] public static readonly SavingHotkey h_RotC = new("Hotkey.Pix.RotC", new Hotkey(KeyboardKey.W, shift: true), SavingLocation.Global);
	[EngineSetting("Hotkey", "Artwork - Rotate Selection Counter-Clockwise")] public static readonly SavingHotkey h_RotCC = new("Hotkey.Pix.RotCC", new Hotkey(KeyboardKey.Q, shift: true), SavingLocation.Global);
	[EngineSetting("Hotkey", "Frame Debug - Next Frame", gameOnly: true)] public static readonly SavingHotkey Hotkey_FrameDebug_Next = new("Hotkey.FrameDebug.Next", new Hotkey(KeyboardKey.Period), SavingLocation.Global);
	[EngineSetting("Hotkey", "Frame Debug - End Debug", gameOnly: true)] public static readonly SavingHotkey Hotkey_FrameDebug_End = new("Hotkey.FrameDebug.End", new Hotkey(KeyboardKey.Comma), SavingLocation.Global);

}