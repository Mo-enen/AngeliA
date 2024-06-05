using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;


public class Console : WindowUI {




	#region --- SUB ---


	private struct Line {
		public int Level;
		public string Content;
		public int Hour;
		public int Minute;
		public int Second;
		public int Frame;
		public Line (int level, string content, int frame) {
			Level = level;
			Content = content;
			Hour = frame / 3600 / 60;
			Minute = (frame / 3600) % 60;
			Second = (frame / 60) % 60;
			Frame = frame % 60;
		}
	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly SpriteCode UI_TOOLBAR = "UI.ToolbarBackground";
	private static readonly SpriteCode ICON_INFO = "Console.Info";
	private static readonly SpriteCode ICON_WARNING = "Console.Warning";
	private static readonly SpriteCode ICON_ERROR = "Console.Error";
	private static readonly SpriteCode PANEL_BG = "UI.GeneralPanel";
	private static readonly LanguageCode HINT_EMPTY_MSG = ("Hint.EmptyMsg", "No message here...");
	private static readonly LanguageCode TIP_CLEAR = ("Tip.ConsoleClear", "Clear messages (Ctrl + Shift + C)");
	
	// Api
	public static Console Instance { get; private set; }
	public bool HasCompileError => CompileErrorLines.Length > 0;
	public override string DefaultName => "Console";

	// Data
	private readonly Pipe<Line> Lines = new(512);
	private readonly Pipe<Line> CompileErrorLines = new(128);
	private int ScrollY = 0;
	private int ScrollErrorY = 0;
	private bool LoggingCompileError = false;


	#endregion




	#region --- MSG ---


	public Console () {
		Instance = this;
		Debug.OnLog += OnLog;
		Debug.OnLogError += OnLogError;
		Debug.OnLogWarning += OnLogWarning;
		Debug.OnLogException += OnLogException;
	}


	public override void UpdateWindowUI () {

		bool hasError = CompileErrorLines.Length > 0;

		// Toolbar
		int barHeight = Unify(42);
		OnGUI_Toolbar(WindowRect.EdgeInside(Direction4.Up, barHeight));

		// Lines
		var errorPanelRect = hasError ? WindowRect.ShrinkUp(barHeight).TopHalf().Shrink(Unify(48)) : default;
		if (!hasError || Lines.Length > 0) {
			OnGUI_Lines(
				37823176,
				WindowRect.ShrinkUp(barHeight),
				Lines, ref ScrollY,
				errorPanelRect.MouseInside(),
				extraSpaces: 6
			);
		}

		// Error Lines
		if (hasError) {
			errorPanelRect = WindowRect.EdgeInside(Direction4.Up, Unify(400)).Shrink(Unify(48), Unify(48), 0, barHeight + Unify(32));
			GUI.DrawSlice(PANEL_BG, errorPanelRect);
			OnGUI_Lines(
				23783177,
				errorPanelRect,
				CompileErrorLines, ref ScrollErrorY,
				!errorPanelRect.MouseInside(),
				extraSpaces: 1
			);
		}

	}


	private void OnGUI_Toolbar (IRect barRect) {

		// BG
		GUI.DrawSliceOrTile(UI_TOOLBAR, barRect);

		int padding = Unify(6);
		barRect = barRect.Shrink(padding);
		var rect = barRect.EdgeInside(Direction4.Left, barRect.height);

		// Clear Button
		if (GUI.Button(rect, BuiltInSprite.ICON_CLEAR, Skin.SmallDarkButton)) {
			Clear();
		}
		RequireTooltip(rect, TIP_CLEAR);
		rect.SlideRight(padding);

	}


	private void OnGUI_Lines (int controlID, IRect panelRect, Pipe<Line> lines, ref int scrollY, bool ignoreScrollWheel, int extraSpaces) {

		// Empty Hint
		if (lines.Length == 0) {
			GUI.Label(panelRect.EdgeInside(Direction4.Up, Unify(42)), HINT_EMPTY_MSG, Skin.SmallCenterGreyLabel);
			return;
		}

		// Lines
		var fullPanelRect = panelRect;
		panelRect = panelRect.Shrink(Unify(12));
		bool hasCompileError = CompileErrorLines.Length > 0;
		int lineHeight = Unify(32);
		int pageHeight = panelRect.height;
		int pageLineCount = pageHeight / lineHeight;
		int scrollMin = hasCompileError ? 0 : -pageLineCount / 2 - 1;
		int scrollMax = (lines.Length - pageLineCount + extraSpaces).GreaterOrEquelThanZero();
		int start = scrollY.Clamp(scrollMin, scrollMax);
		int end = Util.Min(lines.Length, start + pageLineCount);
		var rect = panelRect.EdgeInside(Direction4.Up, lineHeight);
		int iconSize = lineHeight;
		int iconShrink = iconSize / 10;
		int padding = Unify(6);
		int smallPadding = Unify(3);
		int scrollBarWidth = Unify(16);
		bool showLogTime = EngineSetting.ShowLogTime.Value;

		for (int i = start; i < end; i++) {
			// Step Tint
			if (i.UMod(2) == 0) Renderer.DrawPixel(rect, Color32.WHITE_6);
			// Line
			if (i < 0) {
				rect.SlideDown();
				continue;
			}
			var line = lines[lines.Length - i - 1];
			// Icon
			Renderer.Draw(
				line.Level == 0 ? ICON_INFO : line.Level == 1 ? ICON_WARNING : ICON_ERROR,
				rect.EdgeInside(Direction4.Left, iconSize).Shrink(iconShrink)
			);
			// Label
			GUI.InputField(
				controlID + i,
				rect.ShrinkLeft(iconSize + padding),
				line.Content,
				bodyStyle: GUI.Skin.SmallLabel
			);
			// Time
			if (showLogTime) {
				var tChars = GUI.GetTimeChars(line.Hour, line.Minute, line.Second, line.Frame);
				GUI.BackgroundLabel(
					rect.ShrinkRight(scrollBarWidth + padding),
					tChars, Color32.BLACK, smallPadding, GUI.Skin.SmallRightLabel
				);
			}
			rect.SlideDown();
		}

		// Scroll
		if (scrollMax >= 0) {
			// Wheel
			if (!ignoreScrollWheel) {
				int delta = Input.MouseWheelDelta;
				if (delta != 0) {
					scrollY = (scrollY - delta).Clamp(scrollMin, scrollMax);
				}
			}
			// Scroll Bar
			scrollY = GUI.ScrollBar(
				controlID,
				fullPanelRect.EdgeInside(Direction4.Right, Unify(12)),
				scrollY, scrollMax + pageLineCount, pageLineCount
			);
		}
	}


	#endregion




	#region --- API ---


	public void Clear () => Lines.Reset();


	public void RemoveAllCompileErrors () => CompileErrorLines.Reset();


	public void BeginCompileError () => LoggingCompileError = true;


	public void EndCompileError () => LoggingCompileError = false;


	#endregion




	#region --- LGC ---


	private void OnLog (object obj) => LogLogic(obj.ToString(), 0);
	private void OnLogWarning (object obj) => LogLogic(obj.ToString(), 1);
	private void OnLogError (object obj) => LogLogic(obj.ToString(), 2);
	private void OnLogException (System.Exception ex) => OnLogError($"{ex.Source}; {ex.GetType().Name}; {ex.Message}");
	private void LogLogic (string content, int level) {
		const int MAX_CHAR_COUNT = 256;
		if (string.IsNullOrEmpty(content)) return;
		var lines = LoggingCompileError ? CompileErrorLines : Lines;
		if (lines.IsFull && !lines.TryPopHead(out _)) return;
		if (content.Length > MAX_CHAR_COUNT) {
			content = content[..MAX_CHAR_COUNT];
		}
		lines.LinkToTail(new Line(level, content.Replace("\n", "; ").Replace("\r", ""), Game.GlobalFrame));
	}


	#endregion




}