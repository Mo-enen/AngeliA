using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eDialog : Entity {




		#region --- VAR ---


		// Style
		private static NineSliceSprites MAIN_SPRITES = new() {
			TopLeft = "Window UL".AngeHash(),
			TopMid = "Window UM".AngeHash(),
			TopRight = "Window UR".AngeHash(),
			MidLeft = "Window ML".AngeHash(),
			MidMid = "Window MM".AngeHash(),
			MidRight = "Window MR".AngeHash(),
			BottomLeft = "Window DL".AngeHash(),
			BottomMid = "Window DM".AngeHash(),
			BottomRight = "Window DR".AngeHash(),
			border = new(32, 32, 32, 32),
		};
		private static NineSliceSprites BUTTON_SPRITES = new() {
			TopLeft = "Button UL".AngeHash(),
			TopMid = "Button UM".AngeHash(),
			TopRight = "Button UR".AngeHash(),
			MidLeft = "Button ML".AngeHash(),
			MidMid = "Button MM".AngeHash(),
			MidRight = "Button MR".AngeHash(),
			BottomLeft = "Button DL".AngeHash(),
			BottomMid = "Button DM".AngeHash(),
			BottomRight = "Button DR".AngeHash(),
			border = new(24, 24, 24, 24),
		};
		private static int PIXEL_ID { get; } = "Pixel".AngeHash();
		private static Color32 WINDOW_BG = new(240, 240, 240, 255);
		private static Color32 CONTENT_CHAR = new(12, 12, 12, 255);
		private static Color32 BUTTON_NORMAL = new(220, 220, 220, 255);
		private static Color32 BUTTON_HIGHLIGHT = new(200, 200, 200, 255);
		private static Color32 BUTTON_PRESS = new(180, 180, 180, 255);
		private static Color32 BUTTON_CHAR = new(12, 12, 12, 255);
		private static Color32 BG_PANEL = new(0, 0, 0, 64);
		private const int CHAR_SIZE_CONTENT = 124;
		private const int CONTENT_PADDING_X = 164;
		private const int CONTENT_PADDING_TOP = 128;
		private const int CONTENT_PADDING_BOTTOM = 256;
		private const int BUTTON_WIDTH = 600;
		private const int BUTTON_HEIGHT = 186;
		private const int BUTTON_PADDING_X = 48;
		private const int BUTTON_PADDING_Y = 48;
		private const int BUTTON_GAP = 42;
		private const int BUTTON_CHAR_SIZE = 84;

		// Api
		public override bool Despawnable => false;
		public override bool ForceUpdate => true;
		public override int Layer => (int)EntityLayer.UI;

		// Data
		private string Message = "";
		private string Label_OK = "";
		private string Label_Cancel = "";
		private string Label_Alt = "";
		private System.Action OK = null;
		private System.Action Cancel = null;
		private System.Action Alt = null;
		private int NavIndex = 0;
		private bool PrevLeftDown = false;
		private bool PrevRightDown = false;


		#endregion




		#region --- MSG ---


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);

			CellRenderer.BeginCharacterDraw();

			// Pos
			RefreshHeight();
			var cameraRect = CameraRect;
			X = cameraRect.x + (cameraRect.width - Width) / 2;
			Y = cameraRect.y + (cameraRect.height - Height) / 2;

			// Rect
			var mainRect = new RectInt(X, Y, Width, Height);
			var contentRect = new RectInt(
				mainRect.x,
				mainRect.y + BUTTON_HEIGHT,
				mainRect.width,
				mainRect.height - BUTTON_HEIGHT
			).Shrink(
				CONTENT_PADDING_X, CONTENT_PADDING_X,
				CONTENT_PADDING_BOTTOM, CONTENT_PADDING_TOP
			);
			var buttonRect = new RectInt(
				mainRect.x + BUTTON_PADDING_X,
				mainRect.y + BUTTON_PADDING_Y,
				mainRect.width - BUTTON_PADDING_X * 2,
				BUTTON_HEIGHT
			);

			// BG Panel
			CellGUI.DrawButton(
				() => { }, cameraRect, "", 0,
				BG_PANEL, BG_PANEL, BG_PANEL, BG_PANEL, PIXEL_ID
			);

			// Main
			CellGUI.Draw_9Slice(mainRect, WINDOW_BG, MAIN_SPRITES);

			// Content
			CellGUI.DrawLabel(
				Message, contentRect,
				CONTENT_CHAR, CHAR_SIZE_CONTENT,
				0, 0, true, Alignment.TopLeft
			);

			// Nav
			bool leftPress = FrameInput.KeyPressing(GameKey.Left);
			bool rightPress = FrameInput.KeyPressing(GameKey.Right);
			if (leftPress) {
				FrameInput.ClearKeyState(GameKey.Left);
				if (!PrevLeftDown) {
					for (int i = 0; i < 3; i++) {
						NavIndex = Mathf.Clamp(NavIndex - 1, 0, 2);
						var action = NavIndex == 0 ? OK : NavIndex == 1 ? Alt : Cancel;
						if (action != null) { break; }
					}
				}
			}
			if (rightPress) {
				FrameInput.ClearKeyState(GameKey.Right);
				if (!PrevRightDown) {
					for (int i = 0; i < 3; i++) {
						NavIndex = Mathf.Clamp(NavIndex + 1, 0, 2);
						var action = NavIndex == 0 ? OK : NavIndex == 1 ? Alt : Cancel;
						if (action != null) { break; }
					}
				}
			}
			PrevLeftDown = leftPress;
			PrevRightDown = rightPress;
			if (FrameInput.KeyDown(GameKey.Start) || FrameInput.KeyDown(GameKey.Action)) {
				CellGUI.ActiveNavigation(NavIndex == 0 ? OK : NavIndex == 1 ? Alt : Cancel);
				Active = false;
				FrameInput.ClearKeyState(GameKey.Start);
				FrameInput.ClearKeyState(GameKey.Action);
			}
			if (FrameInput.KeyDown(GameKey.Select) || FrameInput.KeyDown(GameKey.Jump)) {
				Active = false;
				FrameInput.ClearKeyState(GameKey.Select);
				FrameInput.ClearKeyState(GameKey.Jump);
			}

			// Buttons 
			int buttonCount = 1;
			for (int i = 2; i >= 0; i--) {
				var action = i == 0 ? OK : i == 1 ? Alt : Cancel;
				if (action == null) { continue; }
				CellGUI.DrawButton(
					() => {
						action();
						Active = false;
					},
					new RectInt(
						buttonRect.xMax - buttonCount * (BUTTON_WIDTH + BUTTON_GAP),
						buttonRect.y, BUTTON_WIDTH, BUTTON_HEIGHT
					),
					GetButtonLabel(i), BUTTON_CHAR_SIZE,
					BUTTON_NORMAL, BUTTON_HIGHLIGHT, BUTTON_PRESS, BUTTON_CHAR,
					BUTTON_SPRITES, i == NavIndex
				);
				buttonCount++;
			}

			if (CellGUI.ButtonPressed) {
				FrameInput.ClearMouseLeftEvent();
			}

		}


		#endregion




		#region --- API ---


		public eDialog (
			int width, string message,
			string label_ok, string label_cancel, string label_alt,
			System.Action ok = null, System.Action cancel = null, System.Action alt = null
		) {
			Width = width;
			Message = message;
			Label_OK = label_ok;
			Label_Cancel = label_cancel;
			Label_Alt = label_alt;
			OK = ok;
			Cancel = cancel;
			Alt = alt;
		}


		#endregion




		#region --- LGC ---


		private string GetButtonLabel (int index) => index switch {
			2 => Label_Cancel,
			1 => Label_Alt,
			0 => Label_OK,
			_ => "",
		};


		private void RefreshHeight () {
			int contentHeight = CellGUI.GetLabelSize(
				Message,
				Width - CONTENT_PADDING_X * 2,
				CHAR_SIZE_CONTENT
			).y;
			Height = contentHeight + CONTENT_PADDING_TOP + CONTENT_PADDING_BOTTOM + BUTTON_HEIGHT;
		}


		#endregion




	}
}
