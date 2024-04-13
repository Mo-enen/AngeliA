namespace AngeliA;

public static class Cursor {


	// Api
	public static int CursorPriority { get; set; } = int.MinValue;
	public static int CurrentCursorIndex { get; private set; } = -1;

	// Data
	private static int CursorEndFrame = int.MinValue + 1;


	// API
	[OnGameUpdatePauseless(64)]
	internal static void FrameUpdate () {
		if (CursorEndFrame != int.MinValue && Game.GlobalFrame > CursorEndFrame) {
			// No Cursor
			CursorPriority = int.MinValue;
			CursorEndFrame = int.MinValue;
			if (Game.IsEdittime) {
				Game.ShowCursor();
				Game.SetCursorToNormal();
			} else {
				Game.HideCursor();
			}
		} else {
			// Has Cursor
			if (CursorPriority != int.MinValue) {
				CursorPriority = int.MinValue;
				if (CurrentCursorIndex == Const.CURSOR_NONE) {
					Game.HideCursor();
					Game.SetCursor(CurrentCursorIndex);
				} else {
					if (!Game.CursorVisible) {
						Game.ShowCursor();
						if (Game.ProjectType == ProjectType.Game) {
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
	}


	public static void RequireCursor (int priority = -1) => SetCursor(-1, priority);


	public static void SetCursor (int cursorIndex, IRect mouseRange, int priority = 0) {
		if (!mouseRange.MouseInside()) return;
		SetCursor(cursorIndex, priority);
	}
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


	public static void SetCursorAsNormal (int priority = 0) => SetCursor(-1, priority);


	public static void SetCursorAsHand (int priority = 0) => SetCursor(Const.CURSOR_HAND, priority);
	public static void SetCursorAsHand (IRect mouseRange, int priority = 0) => SetCursor(Const.CURSOR_HAND, mouseRange, priority);


	public static void SetCursorAsBeam (int priority = 0) => SetCursor(Const.CURSOR_BEAM, priority);
	public static void SetCursorAsBeam (IRect mouseRange, int priority = 0) => SetCursor(Const.CURSOR_BEAM, mouseRange, priority);


	public static void SetCursorAsMove (int priority = 0) => SetCursor(Const.CURSOR_RESIZE_CROSS, priority);
	public static void SetCursorAsMove (IRect mouseRange, int priority = 0) => SetCursor(Const.CURSOR_RESIZE_CROSS, mouseRange, priority);


	public static void SetCursorAsNone (int priority = 0) => SetCursor(Const.CURSOR_NONE, priority);


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