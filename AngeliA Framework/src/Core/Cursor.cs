namespace AngeliA;

/// <summary>
/// Core system for set appearance of mouse cursor
/// </summary>
public static class Cursor {


	// Api
	internal static int CursorPriority { get; set; } = int.MinValue;
	internal static int CurrentCursorIndex { get; private set; } = -1;

	// Data
	private static int CursorEndFrame = int.MinValue + 1;


	// API
	[OnGameUpdatePauseless(64)]
	internal static void FrameUpdate () {
		if (CursorEndFrame != int.MinValue && Game.GlobalFrame > CursorEndFrame) {
			// No Cursor
			CursorPriority = int.MinValue;
			CursorEndFrame = int.MinValue;

#if DEBUG
			const bool IS_DEBUG = true;
#else
			const bool IS_DEBUG = false;
#endif
			if (IS_DEBUG || Game.IsToolApplication) {
				Game.ShowCursor();
				Game.SetCursorToNormal();
			} else {
				Game.HideCursor();
			}

		} else if (CursorPriority != int.MinValue) {
			// Has Cursor
			CursorPriority = int.MinValue;
			if (CurrentCursorIndex == Const.CURSOR_NONE) {
				Game.HideCursor();
				Game.SetCursor(CurrentCursorIndex);
			} else {
				if (!Game.CursorVisible) {
					Game.ShowCursor();
					if (!Game.IsToolApplication) {
						Game.CenterCursor();
					}
				}
				if (CurrentCursorIndex >= 0 && CurrentCursorIndex < Const.CURSOR_COUNT) {
					Game.SetCursor(CurrentCursorIndex);
				} else {
					Game.SetCursorToNormal();
				}
			}
		}
	}


	/// <summary>
	/// Need to display the cursor for current frame
	/// </summary>
	public static void RequireCursor (int priority = -1) => SetCursor(-1, priority);


	/// <param name="cursorIndex">Get this value with Const.CURSOR_XXX</param>
	/// <param name="mouseRange">Only work when mouse inside given range in global space</param>
	/// <param name="priority"></param>
	public static void SetCursor (int cursorIndex, IRect mouseRange, int priority = 0) {
		if (!mouseRange.MouseInside()) return;
		SetCursor(cursorIndex, priority);
	}
	/// <inheritdoc cref="SetCursor(int, IRect, int)"/>
	public static void SetCursor (int cursorIndex, int priority = 0) {
		priority = priority != int.MaxValue ? priority++ : priority; // for int.Min 
		if (priority < CursorPriority) return;
		CursorPriority = priority;
		CursorEndFrame = Game.GlobalFrame + 1;
		if (cursorIndex < 0 || cursorIndex >= Const.CURSOR_COUNT) {
			CurrentCursorIndex = cursorIndex == Const.CURSOR_NONE ? Const.CURSOR_NONE : -1;
		} else {
			CurrentCursorIndex = cursorIndex;
		}
	}


	/// <summary>
	/// Make cursor to default style
	/// </summary>
	public static void SetCursorAsNormal (int priority = 0) => SetCursor(-1, priority);


	/// <inheritdoc cref="SetCursorAsHand(IRect, int)"/>
	public static void SetCursorAsHand (int priority = 0) => SetCursor(Const.CURSOR_HAND, priority);
	/// <summary>
	/// Make cursor to point hand for current frame.
	/// </summary>
	/// <param name="mouseRange">Only work when mouse inside given range in global space</param>
	/// <param name="priority"></param>
	public static void SetCursorAsHand (IRect mouseRange, int priority = 0) => SetCursor(Const.CURSOR_HAND, mouseRange, priority);


	/// <inheritdoc cref="SetCursorAsBeam(IRect, int)"/>
	public static void SetCursorAsBeam (int priority = 0) => SetCursor(Const.CURSOR_BEAM, priority);
	/// <summary>
	/// Make cursor to typing beam for current frame.
	/// </summary>
	/// <param name="mouseRange">Only work when mouse inside given range in global space</param>
	/// <param name="priority"></param>
	public static void SetCursorAsBeam (IRect mouseRange, int priority = 0) => SetCursor(Const.CURSOR_BEAM, mouseRange, priority);


	/// <inheritdoc cref="SetCursorAsMove(IRect, int)"/>
	public static void SetCursorAsMove (int priority = 0) => SetCursor(Const.CURSOR_RESIZE_CROSS, priority);
	/// <summary>
	/// Make cursor to cross with arrows for current frame.
	/// </summary>
	/// <param name="mouseRange">Only work when mouse inside given range in global space</param>
	/// <param name="priority"></param>
	public static void SetCursorAsMove (IRect mouseRange, int priority = 0) => SetCursor(Const.CURSOR_RESIZE_CROSS, mouseRange, priority);


	/// <summary>
	/// Set cursor into none
	/// </summary>
	public static void SetCursorAsNone (int priority = 0) => SetCursor(Const.CURSOR_NONE, priority);


	/// <summary>
	/// Get resize cursor index from given direction
	/// </summary>
	public static int GetResizeCursorIndex (Direction8 direction) => direction switch {
		Direction8.Top => Const.CURSOR_RESIZE_VERTICAL,
		Direction8.Bottom => Const.CURSOR_RESIZE_VERTICAL,
		Direction8.Left => Const.CURSOR_RESIZE_HORIZONTAL,
		Direction8.Right => Const.CURSOR_RESIZE_HORIZONTAL,
		Direction8.TopRight => Const.CURSOR_RESIZE_TOPRIGHT,
		Direction8.BottomRight => Const.CURSOR_RESIZE_TOPLEFT,
		Direction8.BottomLeft => Const.CURSOR_RESIZE_TOPRIGHT,
		Direction8.TopLeft => Const.CURSOR_RESIZE_TOPLEFT,
		_ => Const.CURSOR_DEFAULT,
	};


}