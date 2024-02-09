namespace AngeliaFramework {
	public static class CursorSystem {


		// Data
		private static int CurrentCursorIndex = -1;
		private static int CursorEndFrame = int.MinValue;
		private static int CursorPriority = int.MinValue;


		// API
		[OnGameUpdatePauseless(64)]
		internal static void FrameUpdate () {
			if (CursorEndFrame != int.MinValue && Game.GlobalFrame > CursorEndFrame) {
				// No Cursor
				CursorPriority = int.MinValue;
				CursorEndFrame = int.MinValue;
				Game.SetCursorToNormal();
				if (!Game.IsEdittime) {
					Game.HideCursor();
				}
			} else {
				// Has Cursor
				if (CursorPriority != int.MinValue) {
					CursorPriority = int.MinValue;
					Game.ShowCursor();
					if (CurrentCursorIndex >= 0 && CurrentCursorIndex < Const.CURSOR_COUNT) {
						Game.SetCursor(CurrentCursorIndex);
					} else {
						Game.SetCursorToNormal();
					}
				}
			}
		}


		public static void RequireCursor (int priority = -1) => SetCursor(-1, priority);


		private static void SetCursor (int cursorIndex, IRect mouseRange, int priority = 0) {
			if (!mouseRange.MouseInside()) return;
			SetCursor(cursorIndex, priority);
		}
		private static void SetCursor (int cursorIndex, int priority = 0) {
			priority++; // for int.Min 
			if (priority < CursorPriority) return;
			CursorPriority = priority;
			CursorEndFrame = Game.GlobalFrame + 1;
			if (cursorIndex < 0 || cursorIndex >= Const.CURSOR_COUNT) {
				CurrentCursorIndex = -1;
			} else {
				CurrentCursorIndex = cursorIndex;
			}
		}


		public static void SetCursorAsNormal (int priority = 0) => SetCursor(-1, priority);


		public static void SetCursorAsHand (int priority = 0) => SetCursor(Const.CURSOR_HAND, priority);
		public static void SetCursorAsHand (IRect mouseRange, int priority = 0) => SetCursor(Const.CURSOR_HAND, mouseRange, priority);


		public static void SetCursorAsBeam (int priority = 0) => SetCursor(Const.CURSOR_BEAM, priority);
		public static void SetCursorAsBeam (IRect mouseRange, int priority = 0) => SetCursor(Const.CURSOR_BEAM, mouseRange, priority);


		public static void SetCursorAsMove (int priority = 0) => SetCursor(Const.CURSOR_MOVE, priority);
		public static void SetCursorAsMove (IRect mouseRange, int priority = 0) => SetCursor(Const.CURSOR_MOVE, mouseRange, priority);


		public static void SetCursorPriority (int priority) => CursorPriority = priority;


	}
}