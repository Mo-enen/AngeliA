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
		public Line (int level, string content) {
			Level = level;
			Content = content;
		}
	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly SpriteCode ICON_INFO = "Console.Info";
	private static readonly SpriteCode ICON_WARNING = "Console.Warning";
	private static readonly SpriteCode ICON_ERROR = "Console.Error";

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


	~Console () {
		Debug.OnLog -= OnLog;
		Debug.OnLogError -= OnLogError;
		Debug.OnLogWarning -= OnLogWarning;
		Debug.OnLogException -= OnLogException;
	}


	public override void UpdateWindowUI () {

		var windowRect = WindowRect.Shrink(Unify(12));

		// Lines
		int lineHeight = Unify(32);
		int pageHeight = windowRect.height;
		int pageLineCount = pageHeight / lineHeight;
		int scrollMax = (Lines.Length - pageLineCount + 6).GreaterOrEquelThanZero();
		int start = ScrollY.Clamp(0, scrollMax);
		int end = Util.Min(Lines.Length, start + pageLineCount);
		var rect = windowRect.EdgeInside(Direction4.Up, lineHeight);
		int iconSize = lineHeight;
		int iconShrink = iconSize / 10;
		int padding = Unify(6);
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
			GUI.InputField(86754 + i, rect.ShrinkLeft(iconSize + padding), line.Content, bodyStyle: GUISkin.SmallLabel);
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
				89234111, WindowRect.EdgeInside(Direction4.Right, Unify(16)),
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
		if (string.IsNullOrEmpty(content)) return;
		if (Lines.IsFull && !Lines.TryPopHead(out _)) return;
		Lines.LinkToTail(new Line(level, content.Replace("\n", "; ").Replace("\r", "")));
	}


	#endregion




}