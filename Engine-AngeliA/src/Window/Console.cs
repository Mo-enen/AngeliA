using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

[RequireSpriteFromField]
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
	private static readonly SpriteCode ICON_INFO = "Console.Info";
	private static readonly SpriteCode ICON_WARNING = "Console.Warning";
	private static readonly SpriteCode ICON_ERROR = "Console.Error";
	private static readonly LanguageCode HINT_EMPTY_MSG = ("Hint.EmptyMsg", "No message here...");

	// Api
	public override string DefaultName => "Console";
	public static readonly SavingBool ShowLogTime = new("Console.ShowLogTime", true);

	// Data
	private readonly Pipe<Line> Lines = new(512);
	private int ScrollY = 0;


	#endregion




	#region --- MSG ---


	public Console () {
		Debug.OnLog += OnLog;
		Debug.OnLogError += OnLogError;
		Debug.OnLogWarning += OnLogWarning;
		Debug.OnLogException += OnLogException;
	}


	public override void UpdateWindowUI () {

		var panelRect = WindowRect.Shrink(Unify(12));

		// Empty Hint
		if (Lines.Length == 0) {
			GUI.Label(panelRect.EdgeInside(Direction4.Up, Unify(42)), HINT_EMPTY_MSG, Skin.SmallCenterGreyLabel);
			return;
		}

		// Lines
		int lineHeight = Unify(32);
		int pageHeight = panelRect.height;
		int pageLineCount = pageHeight / lineHeight;
		int scrollMax = (Lines.Length - pageLineCount + 6).GreaterOrEquelThanZero();
		int start = ScrollY.Clamp(0, scrollMax);
		int end = Util.Min(Lines.Length, start + pageLineCount);
		var rect = panelRect.EdgeInside(Direction4.Up, lineHeight);
		int iconSize = lineHeight;
		int iconShrink = iconSize / 10;
		int padding = Unify(6);
		int smallPadding = Unify(3);
		int scrollBarWidth = Unify(16);
		bool showLogTime = ShowLogTime.Value;

		for (int i = start; i < end; i++) {
			var line = Lines[Lines.Length - i - 1];
			// Step Tint
			if (i % 2 == 0) Renderer.DrawPixel(rect, Color32.WHITE_6);
			// Icon
			Renderer.Draw(
				line.Level == 0 ? ICON_INFO : line.Level == 1 ? ICON_WARNING : ICON_ERROR,
				rect.EdgeInside(Direction4.Left, iconSize).Shrink(iconShrink)
			);
			// Label
			GUI.InputField(
				86754 + i,
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
			int delta = Input.MouseWheelDelta;
			if (delta != 0) {
				ScrollY = (ScrollY - delta).Clamp(0, scrollMax);
			}
			// Scroll Bar
			ScrollY = GUI.ScrollBar(
				89234111, WindowRect.EdgeInside(Direction4.Right, scrollBarWidth),
				ScrollY, scrollMax + pageLineCount, pageLineCount
			);
		}
	}


	#endregion




	#region --- LGC ---


	private void OnLog (object obj) => LogLogic(obj.ToString(), 0);
	private void OnLogWarning (object obj) => LogLogic(obj.ToString(), 1);
	private void OnLogError (object obj) => LogLogic(obj.ToString(), 2);
	private void OnLogException (System.Exception ex) => OnLogError($"{ex.Source}; {ex.GetType().Name}; {ex.Message}");
	private void LogLogic (string content, int level) {
		const int MAX_CHAR_COUNT = 256;
		if (string.IsNullOrEmpty(content)) return;
		if (Lines.IsFull && !Lines.TryPopHead(out _)) return;
		if (content.Length > MAX_CHAR_COUNT) {
			content = content[..MAX_CHAR_COUNT];
		}
		Lines.LinkToTail(new Line(level, content.Replace("\n", "; ").Replace("\r", ""), Game.GlobalFrame));
	}


	#endregion




}