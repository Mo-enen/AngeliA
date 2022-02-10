using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public static class CellGUI {




		#region --- VAR ---


		// Api
		public static bool HoverOnGUI { get; private set; } = false;

		// Data
		private static System.Action PressAction = null;
		private static System.Action NavigationAction = null;
		private static RectInt? ButtonNavigation = null;
		private static NineSliceSprites ButtonNavigationFrame = new() {
			TopLeft = "Button Highlight UL".ACode(),
			TopMid = "Button Highlight UM".ACode(),
			TopRight = "Button Highlight UR".ACode(),
			MidLeft = "Button Highlight ML".ACode(),
			MidRight = "Button Highlight MR".ACode(),
			BottomLeft = "Button Highlight DL".ACode(),
			BottomMid = "Button Highlight DM".ACode(),
			BottomRight = "Button Highlight DR".ACode(),
			border = new(30, 30, 30, 30),
		};


		#endregion




		#region --- API ---


		public static void Draw_9Slice (RectInt rect, Color32 tint, NineSliceSprites sprite) {

			var border = sprite.border;

			if (border.horizontal > rect.width) {
				float oldH = border.horizontal;
				border.left = Mathf.RoundToInt(rect.width * (border.left / oldH));
				border.right = Mathf.RoundToInt(rect.width * (border.right / oldH));
			}

			if (border.vertical > rect.height) {
				float oldV = border.vertical;
				border.bottom = Mathf.RoundToInt(rect.height * (border.bottom / oldV));
				border.top = Mathf.RoundToInt(rect.height * (border.top / oldV));
			}

			CellRenderer.Draw(sprite.TopLeft, rect.x, rect.yMax - border.top, 0, 0, 0, border.left, border.top, tint);
			CellRenderer.Draw(sprite.TopMid, rect.x + border.left, rect.yMax - border.top, 0, 0, 0, rect.width - border.horizontal, border.top, tint);
			CellRenderer.Draw(sprite.TopRight, rect.xMax - border.right, rect.yMax - border.top, 0, 0, 0, border.right, border.top, tint);

			CellRenderer.Draw(sprite.MidLeft, rect.x, rect.y + border.bottom, 0, 0, 0, border.left, rect.height - border.vertical, tint);
			CellRenderer.Draw(sprite.MidMid, rect.x + border.left, rect.y + border.bottom, 0, 0, 0, rect.width - border.horizontal, rect.height - border.vertical, tint);
			CellRenderer.Draw(sprite.MidRight, rect.xMax - border.right, rect.y + border.bottom, 0, 0, 0, border.right, rect.height - border.vertical, tint);

			CellRenderer.Draw(sprite.BottomLeft, rect.x, rect.y, 0, 0, 0, border.left, border.bottom, tint);
			CellRenderer.Draw(sprite.BottomMid, rect.x + border.left, rect.y, 0, 0, 0, rect.width - border.horizontal, border.bottom, tint);
			CellRenderer.Draw(sprite.BottomRight, rect.xMax - border.right, rect.y, 0, 0, 0, border.right, border.bottom, tint);
		}


		// Text Label
		public static void DrawLabel (string content, RectInt rect, Color32 color, int charSize, int charSpace = 0, int lineSpace = 0, bool wrap = true, Alignment alignment = Alignment.TopLeft) {
			int count = content.Length;
			int x = rect.x;
			int y = rect.yMax - charSize;
			int offsetX = 0;
			int offsetY = 0;
			if (alignment != Alignment.TopLeft) {
				var textSize = GetLabelSize(content, rect.width, charSize, charSpace, lineSpace, wrap);
				offsetX = alignment switch {
					Alignment.TopMid or Alignment.MidMid or Alignment.BottomMid => (rect.width - textSize.x) / 2,
					Alignment.TopRight or Alignment.MidRight or Alignment.BottomRight => rect.width - textSize.x,
					_ => 0,
				};
				offsetY = alignment switch {
					Alignment.MidLeft or Alignment.MidMid or Alignment.MidRight => (rect.height - textSize.y) / 2,
					Alignment.BottomLeft or Alignment.BottomMid or Alignment.BottomRight => rect.height - textSize.y,
					_ => 0,
				};
			}
			for (int i = 0; i < count; i++) {
				char c = content[i];
				// Line
				if (c == '\r' || c == '\n') {
					x = rect.x;
					y -= charSize + lineSpace;
					continue;
				}
				// Draw Char
				CellRenderer.DrawChar(
					("c_" + c).ACode(),
					x + offsetX, y - offsetY, charSize, charSize,
					color, out bool fullWidth
				);
				x += fullWidth ? charSize + charSpace : charSize / 2 + charSpace;
				if (wrap && x > rect.xMax - charSize) {
					x = rect.x;
					y -= charSize + lineSpace;
				}
			}
		}


		public static Vector2Int GetLabelSize (string content, float width, int charSize, int charSpace = 0, int lineSpace = 0, bool wrap = true) {
			int x = 0;
			int y = charSize;
			int count = content.Length;
			int xMax = 0;
			for (int i = 0; i < count; i++) {
				char c = content[i];
				if (c == '\r' || c == '\n') {
					x = 0;
					y += charSize + lineSpace;
					continue;
				}
				int id = ("c_" + c).ACode();
				bool fullWidth = CellRenderer.IsFullWidth(id);
				x += fullWidth ? charSize + charSpace : charSize / 2 + charSpace;
				xMax = Mathf.Max(x, xMax);
				if (wrap && i < count - 1 && x > width - charSize) {
					x = 0;
					y += charSize + lineSpace;
				}
			}
			return new Vector2Int(xMax + 1, y + 1);
		}


		// Button
		public static void DrawButton (System.Action click, RectInt rect, string label, int charSize, Color32 normal, Color32 highlight, Color32 pressed, Color32 labelColor, int spriteID, bool navigation = false) {
			bool hover = rect.Contains(ScreenToCameraPosition(FrameInput.MousePosition01));
			HoverOnGUI = HoverOnGUI || hover;
			bool pressing = FrameInput.MouseLeft;
			var tint = hover ? pressing ? pressed : highlight : normal;
			CellRenderer.Draw(spriteID, rect.x, rect.y, 0, 0, 0, rect.width, rect.height, tint);
			DrawLabel(label, rect, labelColor, charSize, 0, 0, false, Alignment.MidMid);
			if (hover && FrameInput.MouseLeftDown) {
				PressAction = click;
			}
			if (navigation) {
				ButtonNavigation = rect;
			}
		}


		public static void DrawButton (System.Action click, RectInt rect, string label, int charSize, Color32 normal, Color32 highlight, Color32 pressed, Color32 labelColor, NineSliceSprites sprite, bool nagigation = false) {
			bool hover = rect.Contains(ScreenToCameraPosition(FrameInput.MousePosition01));
			HoverOnGUI = HoverOnGUI || hover;
			bool pressing = FrameInput.MouseLeft;
			var tint = hover ? pressing ? pressed : highlight : normal;
			Draw_9Slice(rect, tint, sprite);
			DrawLabel(label, rect, labelColor, charSize, 0, 0, false, Alignment.MidMid);
			if (hover && FrameInput.MouseLeftDown) {
				PressAction = click;
			}
			if (nagigation) {
				ButtonNavigation = rect;
			}
		}


		public static void ActiveNavigation (System.Action action) => NavigationAction = action;


		// Event
		public static void PerformFrame (int frame) {
			if (PressAction != null) {
				PressAction();
				PressAction = null;
				NavigationAction = null;
			}
			if (NavigationAction != null) {
				NavigationAction();
				PressAction = null;
				NavigationAction = null;
			}
			if (ButtonNavigation != null) {
				Draw_9Slice(
					ButtonNavigation.Value.Expand(
						12 + (int)Mathf.PingPong(frame % (48 * 2), 48) / 4
					),
					new Color32(9, 181, 161, 255),
					ButtonNavigationFrame
				);
				ButtonNavigation = null;
			}
			HoverOnGUI = false;
		}


		#endregion




		#region --- LGC ---


		private static Vector2Int ScreenToCameraPosition (Vector2 screenPos01) => new(
			(int)Mathf.LerpUnclamped(CellRenderer.CameraRect.x, CellRenderer.CameraRect.xMax, screenPos01.x),
			(int)Mathf.LerpUnclamped(CellRenderer.CameraRect.y, CellRenderer.CameraRect.yMax, screenPos01.y)
		);


		#endregion




	}
}
