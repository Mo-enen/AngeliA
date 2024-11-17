using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public class ConsoleWindow : WindowUI {




	#region --- SUB ---


	private struct Line (int level, string content, int frame) {
		public int Level = level;
		public string Content = content;
		public int Hour = frame / 3600 / 60;
		public int Minute = (frame / 3600) % 60;
		public int Second = (frame / 60) % 60;
		public int Frame = frame % 60;
	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly SpriteCode UI_BG = "UI.Console.BG";
	private static readonly SpriteCode UI_TOOLBAR = "UI.Console.Toolbar";
	private static readonly SpriteCode ICON_CODE_ANA = "Console.Analysis";
	private static readonly SpriteCode PANEL_BG = "UI.Console.ErrorPanel";
	private static readonly LanguageCode HINT_EMPTY_MSG = ("Hint.EmptyMsg", "No message here...");
	private static readonly LanguageCode TIP_CLEAR = ("Tip.ConsoleClear", "Clear messages (Ctrl + Shift + C)");
	private static readonly LanguageCode TIP_HASH_COL = ("Tip.HashCol", "Check hash collision for all scripts and artwork");

	// Api
	public static ConsoleWindow Instance { get; private set; }
	public bool HaveRunningRigGame { get; set; } = false;
	public bool HasCompileError => CompileErrorLines.Length > 0;
	public sbyte RequireCodeAnalysis { get; set; } = 0;
	public override string DefaultWindowName => "Console";

	// Data
	private readonly char[] TimingChars = new char[12]; // 99+°59'59"59
	private readonly Pipe<Line> Lines = new(512);
	private readonly Pipe<Line> CompileErrorLines = new(128);
	private int ScrollY = 0;
	private int ScrollErrorY = 0;
	private bool LoggingCompileError = false;


	#endregion




	#region --- MSG ---


	public ConsoleWindow () {
		Instance = this;
		Debug.OnLog += OnLog;
		Debug.OnLogError += OnLogError;
		Debug.OnLogWarning += OnLogWarning;
		Debug.OnLogException += OnLogException;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Cursor.RequireCursor();
	}


	public override void UpdateWindowUI () {

		bool hasLog = Lines.Length > 0;
		bool hasError = CompileErrorLines.Length > 0;
		int barHeight = GUI.ToolbarSize;
		var windowRect = WindowRect;

		// BG
		GUI.DrawSlice(UI_BG, windowRect.ShrinkUp(GUI.ToolbarSize));

		// Toolbar
		OnGUI_Toolbar(windowRect.Edge(Direction4.Up, barHeight));

		// Lines
		if (hasLog) {
			var panelRect = hasError ? windowRect.ShrinkUp(barHeight).BottomHalf() : windowRect.ShrinkUp(barHeight);
			OnGUI_Lines(37823176, panelRect, Lines, ref ScrollY);
		}

		// Error Lines
		if (hasError) {
			var panelRect = hasLog ? windowRect.ShrinkUp(barHeight).TopHalf() : windowRect.ShrinkUp(barHeight);
			GUI.DrawSlice(PANEL_BG, panelRect);
			OnGUI_Lines(23783177, panelRect, CompileErrorLines, ref ScrollErrorY);
		}

	}


	private void OnGUI_Toolbar (IRect barRect) {

		// BG
		GUI.DrawSlice(UI_TOOLBAR, barRect);

		int padding = Unify(6);
		barRect = barRect.Shrink(padding);
		var rect = barRect.Edge(Direction4.Left, barRect.height);

		// Clear Button
		if (GUI.Button(rect, BuiltInSprite.ICON_CLEAR, Skin.SmallDarkButton)) {
			Clear();
		}
		RequireTooltip(rect, TIP_CLEAR);
		rect.SlideRight(padding);

		// Code Analysis
		if (GUI.Button(rect, ICON_CODE_ANA, Skin.SmallDarkButton)) {
			if (HaveRunningRigGame) {
				RequireCodeAnalysis = 1;
			}
#if DEBUG
			string projectFolder = Util.GetParentPath(Universe.BuiltIn.UniverseRoot);
			FrameworkUtil.RunEmptyScriptFileAnalysis(Util.GetParentPath(projectFolder));
			FrameworkUtil.RunBuiltInSpriteAnalysys();
#endif
		}
		RequireTooltip(rect, TIP_HASH_COL);

		rect.SlideRight(padding);

	}


	private void OnGUI_Lines (int controlID, IRect panelRect, Pipe<Line> lines, ref int scrollY) {

		// Empty Hint
		if (lines.Length == 0) {
			GUI.Label(panelRect.Edge(Direction4.Up, Unify(42)), HINT_EMPTY_MSG, Skin.SmallCenterGreyLabel);
			return;
		}

		// Lines
		const int EXTRA_SPACES = 2;
		var fullPanelRect = panelRect;
		panelRect = panelRect.Shrink(Unify(12));
		bool hasCompileError = CompileErrorLines.Length > 0;
		int lineHeight = Unify(32);
		int pageHeight = panelRect.height;
		int pageLineCount = pageHeight / lineHeight;
		int scrollMin = hasCompileError ? 0 : -pageLineCount / 2 - 1;
		int scrollMax = (lines.Length - pageLineCount + EXTRA_SPACES).GreaterOrEquelThanZero();
		int start = scrollY.Clamp(scrollMin, scrollMax);
		int end = Util.Min(lines.Length, start + pageLineCount);
		var rect = panelRect.Edge(Direction4.Up, lineHeight);
		int iconSize = lineHeight;
		int iconShrink = iconSize / 10;
		int padding = Unify(6);
		int smallPadding = Unify(3);
		int scrollBarWidth = Unify(16);
		bool showLogTime = EngineSetting.ShowLogTime.Value;
		bool ignoreScrollWheel = !panelRect.MouseInside();

		for (int i = start; i < end; i++) {
			// Step Tint
			if (i.UMod(2) == 0) Renderer.DrawPixel(rect, Color32.WHITE_6);
			// Line
			if (i < 0) {
				rect.SlideDown();
				continue;
			}
			var line = lines[i];
			// Icon
			Renderer.Draw(
				line.Level == 0 ? BuiltInSprite.ICON_INFO : line.Level == 1 ? BuiltInSprite.ICON_WARNING : BuiltInSprite.ICON_ERROR,
				rect.Edge(Direction4.Left, iconSize).Shrink(iconShrink)
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
				FillTimeChars(line.Hour, line.Minute, line.Second, line.Frame);
				GUI.BackgroundLabel(
					rect.ShrinkRight(scrollBarWidth + padding),
					TimingChars, Color32.BLACK, smallPadding, GUI.Skin.SmallRightLabel
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
				fullPanelRect.Edge(Direction4.Right, GUI.ScrollbarSize),
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


	private void OnLog (object obj) => LogLogic(obj != null ? obj.ToString() : "null", 0);
	private void OnLogWarning (object obj) => LogLogic(obj != null ? obj.ToString() : "null", 1);
	private void OnLogError (object obj) => LogLogic(obj != null ? obj.ToString() : "null", 2);
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
		ScrollY = int.MaxValue;
	}


	private void FillTimeChars (int hour, int min, int sec, int frame = -1, bool ignoreZero = true) {

		int index = 0;

		// Hour
		if (hour > 0 || (hour == 0 && !ignoreZero)) {
			if (hour < 100) {
				IntToChars.Int_to_Chars(hour, TimingChars, ref index);
				TimingChars[index] = '°';
				index++;
			} else {
				TimingChars[0] = '9';
				TimingChars[1] = '9';
				TimingChars[2] = '+';
				TimingChars[3] = '°';
				index = 4;
			}
		}

		// Min
		if (min > 0 || (min == 0 && !ignoreZero)) {
			min %= 60;
			TimingChars[index] = (char)('0' + (min / 10));
			TimingChars[index + 1] = (char)('0' + (min % 10));
			TimingChars[index + 2] = '\'';
			index += 3;
		}

		// Sec
		if (sec > 0 || (sec == 0 && !ignoreZero)) {
			sec %= 60;
			TimingChars[index] = (char)('0' + (sec / 10));
			TimingChars[index + 1] = (char)('0' + (sec % 10));
			TimingChars[index + 2] = '\'';
			index += 3;
		}

		// Frame
		if (frame > 0 || (frame == 0 && !ignoreZero)) {
			frame %= 60;
			TimingChars[index] = (char)('0' + (frame / 10));
			TimingChars[index + 1] = (char)('0' + (frame % 10));
			TimingChars[index + 2] = '\'';
			index += 3;
		}

		// Zero
		if (index == 0) {
			TimingChars[0] = '0';
			index++;
		}

		// End
		if (index < TimingChars.Length) {
			TimingChars[index] = '\0';
		}

	}


	#endregion




}