using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public static class CursorSystem {


		// Data
		private static Float2[] CursorPivots = null;
		private static int CurrentCursorIndex = -1;
		private static int CursorEndFrame = int.MinValue;
		private static int CursorPriority = int.MinValue;
		private static Texture2D[] Cursors = null;


		// API
		public static void Initialize (Texture2D[] cursors) {
#if !UNITY_EDITOR
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
#endif
			Cursors = cursors;
			if (cursors == null || cursors.Length == 0) {
				CursorPivots = new Float2[0];
				return;
			}
			CursorPivots = new Float2[cursors.Length];
			for (int i = 0; i < cursors.Length; i++) {
				var texture = cursors[i];
				if (texture == null) continue;
				var pivot = new Float2(texture.width / 2f, texture.height / 2f);
				var oic = System.StringComparison.OrdinalIgnoreCase;
				switch (texture.name) {

					case var _name when _name.EndsWith("#bottom", oic):
						pivot = new Float2(texture.width / 2f, texture.height);
						break;
					case var _name when _name.EndsWith("#top", oic):
						pivot = new Float2(texture.width / 2f, 0);
						break;
					case var _name when _name.EndsWith("#left", oic):
						pivot = new Float2(0, texture.height / 2f);
						break;
					case var _name when _name.EndsWith("#right", oic):
						pivot = new Float2(texture.width, texture.height / 2f);
						break;

					case var _name when _name.EndsWith("#bottomleft", oic):
						pivot = new Float2(0, texture.height);
						break;
					case var _name when _name.EndsWith("#bottomright", oic):
						pivot = new Float2(texture.width, texture.height);
						break;
					case var _name when _name.EndsWith("#topleft", oic):
						pivot = new Float2(0, 0);
						break;
					case var _name when _name.EndsWith("#topright", oic):
						pivot = new Float2(texture.width, 0);
						break;
				}
				CursorPivots[i] = pivot;
			}
		}


		public static void FrameUpdate () {
			if (CursorEndFrame != int.MinValue && Game.GlobalFrame > CursorEndFrame) {
				// No Cursor
				CursorPriority = int.MinValue;
				CursorEndFrame = int.MinValue;
				Cursor.SetCursor(null, default, CursorMode.Auto);
#if !UNITY_EDITOR
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
#endif
			} else {
				// Has Cursor
				if (CursorPriority != int.MinValue) {
					CursorPriority = int.MinValue;
					if (CurrentCursorIndex >= 0 && CurrentCursorIndex < Cursors.Length) {
						var texture = Cursors[CurrentCursorIndex];
						if (texture == null) return;
						var pivot = Float2.zero;
						if (CurrentCursorIndex < CursorPivots.Length) {
							pivot = CursorPivots[CurrentCursorIndex];
						}
						Cursor.SetCursor(texture, pivot, CursorMode.Auto);
					} else {
						Cursor.SetCursor(null, default, CursorMode.Auto);
					}
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}
			}
		}


		public static void RequireCursor (int priority = -1) => SetCursor(-1, priority);


		public static void SetCursor (int cursorIndex, IRect mouseRange, int priority = 0) {
			if (!mouseRange.Contains(FrameInput.MouseGlobalPosition)) return;
			SetCursor(cursorIndex, priority);
		}
		public static void SetCursor (int cursorIndex, int priority = 0) {
			if (Cursors == null) return;
			priority++; // for int.Min 
			if (priority < CursorPriority) return;
			CursorPriority = priority;
			CursorEndFrame = Game.GlobalFrame + 1;
			if (cursorIndex < 0 || cursorIndex >= Cursors.Length) {
				CurrentCursorIndex = -1;
			} else {
				CurrentCursorIndex = cursorIndex;
			}
		}


		public static void SetCursorAsHand (int priority = 0) => SetCursor(Const.CURSOR_HAND, priority);
		public static void SetCursorAsHand (IRect mouseRange, int priority = 0) => SetCursor(Const.CURSOR_HAND, mouseRange, priority);


		public static void SetCursorAsBeam (int priority = 0) => SetCursor(Const.CURSOR_BEAM, priority);
		public static void SetCursorAsBeam (IRect mouseRange, int priority = 0) => SetCursor(Const.CURSOR_BEAM, mouseRange, priority);


		public static void SetCursorAsMove (int priority = 0) => SetCursor(Const.CURSOR_MOVE, priority);
		public static void SetCursorAsMove (IRect mouseRange, int priority = 0) => SetCursor(Const.CURSOR_MOVE, mouseRange, priority);


	}
}