using AngeliA;

namespace AngeliaEngine;

public class EngineSetting {

	// Engine
	public static readonly SavingBool OpenLastProjectOnStart = new("Engine.OpenLastProjectOnStart", false);
	public static readonly SavingBool UseTooltip = new("Engine.UseTooltip", true);
	public static readonly SavingBool UseNotification = new("Engine.UseNotification", true);
	public static readonly SavingBool AutoRecompile = new("Engine.AutoRecompile", true);
	public static readonly SavingBool ClearCharacterConfigBeforeGameStart = new("Engine.ClearCharacterConfigBeforeGameStart", true);

	// Pixel Editor
	public static readonly SavingColor32 BackgroundColor = new("PixEdt.BGColor", new Color32(32, 33, 37, 255));
	public static readonly SavingBool GradientBackground = new("PixEdt.GradientBackground", true);
	public static readonly SavingBool SolidPaintingPreview = new("PixEdt.SolidPaintingPreview", true);

	// Char Ani Editor
	public static readonly SavingBool ReverseMouseScrollForTimeline = new("CharAni.TimelineReverseMouseScroll", false);
	public static readonly SavingBool MouseScrollVerticalForTimeline = new("CharAni.TimelineMouseScrollVertical", true);
	public static readonly SavingBool MidDragHorizontalOnlyForTimeline = new("CharAni.TimelineMidDragHorizontalOnly", true);

	// Console
	public static readonly SavingBool ShowLogTime = new("Console.ShowLogTime", false);
	public static readonly SavingBool BlinkWhenError = new("Console.BlinkWhenError", true);

	// Map Editor
	public static readonly SavingInt LastMapEditorViewX = new("Map.LastMapEditorViewX", 0);
	public static readonly SavingInt LastMapEditorViewY = new("Map.LastMapEditorViewY", 0);
	public static readonly SavingInt LastMapEditorViewZ = new("Map.LastMapEditorViewZ", 0);
	public static readonly SavingInt LastMapEditorViewHeight = new("Map.LastMapEditorViewHeight", -1);
	public static readonly SavingBool MapEditor_Enable = new("MapEditor.Enable", true);
	public static readonly SavingBool MapEditor_QuickPlayerDrop = new("MapEditor.QuickPlayerDrop", false);
	public static readonly SavingBool MapEditor_AutoZoom = new("MapEditor.AutoZoom", true);
	public static readonly SavingBool MapEditor_ShowState = new("MapEditor.ShowState", false);
	public static readonly SavingBool MapEditor_ShowBehind = new("MapEditor.ShowBehind", true);

	// Hotkey
	public static readonly SavingHotkey Hotkey_Recompile = new("Hotkey.Recompile", new Hotkey(KeyboardKey.R, ctrl: true));
	public static readonly SavingHotkey Hotkey_Run = new("Hotkey.Run", new Hotkey(KeyboardKey.R, ctrl: true, shift: true));
	public static readonly SavingHotkey Hotkey_ClearConsole = new("Hotkey.ClearConsole", new Hotkey(KeyboardKey.C, ctrl: true, shift: true));
	public static readonly SavingHotkey Hotkey_Window_MapEditor = new("Hotkey.Window.MapEditor", new Hotkey(KeyboardKey.F1));
	public static readonly SavingHotkey Hotkey_Window_Artwork = new("Hotkey.Window.Artwork", new Hotkey(KeyboardKey.F2));
	public static readonly SavingHotkey Hotkey_Window_CharAni = new("Hotkey.Window.CharAni", new Hotkey(KeyboardKey.F3));
	public static readonly SavingHotkey Hotkey_Window_Language = new("Hotkey.Window.Language", new Hotkey(KeyboardKey.F4));
	public static readonly SavingHotkey Hotkey_Window_Console = new("Hotkey.Window.Console", new Hotkey(KeyboardKey.F5));
	public static readonly SavingHotkey Hotkey_Window_Project = new("Hotkey.Window.Project", new Hotkey(KeyboardKey.F6));
	public static readonly SavingHotkey Hotkey_Window_Setting = new("Hotkey.Window.Setting", new Hotkey(KeyboardKey.F7));
	public static readonly SavingHotkey Hotkey_PixTool_Rect = new("Hotkey.Pix.Rect", new Hotkey(KeyboardKey.U));
	public static readonly SavingHotkey Hotkey_PixTool_Line = new("Hotkey.Pix.Line", new Hotkey(KeyboardKey.L));
	public static readonly SavingHotkey Hotkey_PixTool_Bucket = new("Hotkey.Pix.Bucket", new Hotkey(KeyboardKey.G));
	public static readonly SavingHotkey Hotkey_PixTool_Select = new("Hotkey.Pix.Select", new Hotkey(KeyboardKey.M));
	public static readonly SavingHotkey Hotkey_PixTool_Sprite = new("Hotkey.Pix.Sprite", new Hotkey(KeyboardKey.S));
	public static readonly SavingHotkey Hotkey_Pix_PalettePrev = new("Hotkey.Pix.PalPrev", new Hotkey(KeyboardKey.Q));
	public static readonly SavingHotkey Hotkey_Pix_PaletteNext = new("Hotkey.Pix.PalNext", new Hotkey(KeyboardKey.W));
	public static readonly SavingHotkey Hotkey_FrameDebug_Next = new("Hotkey.FrameDebug.Next", new Hotkey(KeyboardKey.Period));


}