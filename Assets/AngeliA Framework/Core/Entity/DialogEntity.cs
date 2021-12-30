using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AngeliaFramework {
	public class DialogEntity : Entity {




		#region --- VAR ---


		// Api
		public override bool Despawnable => false;

		// Data
		private int Width;
		private int Height;
		private string Title = "";
		private string Message = "";
		private string Label_OK = "";
		private string Label_Cancel = "";
		private string Label_Alt = "";
		private System.Action OK = null;
		private System.Action Cancel = null;
		private System.Action Alt = null;


		#endregion




		#region --- MSG ---


		public override void FrameUpdate () {

			// Const
			int CHAR_SIZE_TITLE = 90;
			int CHAR_SIZE_CONTENT = 100;
			int TITLE_HEIGHT = 128;
			int TITLE_PADDING = 64;
			int TITLE_SPRITE = "Pixel".GetAngeliaHashCode();
			Color32 TITLE_BG = new(0, 153, 188, 255);
			Color32 TITLE_CHAR = new(255, 255, 255, 255);

			Color32 WINDOW_BG = new(240, 240, 240, 255);
			int WINDOW_SPRITE = "Pixel".GetAngeliaHashCode();

			Color32 CONTENT_CHAR = new(12, 12, 12, 255);
			int CONTENT_PADDING_X = 164;
			int CONTENT_PADDING_Y = 128;
			Alignment CONTENT_AL = Alignment.TopLeft;

			int BUTTON_WIDTH = 512;
			int BUTTON_HEIGHT = 128;
			int BUTTON_PADDING_X = 48;
			int BUTTON_PADDING_Y = 48;
			int BUTTON_GAP = 42;
			int BUTTON_CHAR_SIZE = 84;
			int BUTTON_SPRITE = "Pixel".GetAngeliaHashCode();
			Color32 BUTTON_NORMAL = new(220, 220, 220, 255);
			Color32 BUTTON_HIGHLIGHT = new(200, 200, 200, 255);
			Color32 BUTTON_PRESS = new(180, 180, 180, 255);
			Color32 BUTTON_CHAR = new(12, 12, 12, 255);

			// Pos
			X = CellRenderer.CameraRect.x + (CellRenderer.CameraRect.width - Width) / 2;
			Y = CellRenderer.CameraRect.y + (CellRenderer.CameraRect.height - Height) / 2;

			// Title
			var titleRect = new RectInt(X, Y + Height - TITLE_HEIGHT, Width, TITLE_HEIGHT);
			CellRenderer.Draw(
				TITLE_SPRITE,
				titleRect.x, titleRect.y, 0, 0, 0,
				titleRect.width, titleRect.height, TITLE_BG
			);
			CellRenderer.DrawLabel(
				Title,
				titleRect.Shrink(TITLE_PADDING, 0, 0, 0),
				TITLE_CHAR, CHAR_SIZE_TITLE, 0, 0, false, Alignment.MidLeft
			);

			// Main
			var mainRect = new RectInt(X, Y, Width, Height - TITLE_HEIGHT);
			CellRenderer.Draw(
				WINDOW_SPRITE,
				mainRect.x, mainRect.y, 0, 0, 0,
				mainRect.width, mainRect.height, WINDOW_BG
			);

			// Content
			var contentRect = new RectInt(
				mainRect.x,
				mainRect.y + BUTTON_HEIGHT,
				mainRect.width,
				mainRect.height - BUTTON_HEIGHT
			);
			contentRect = contentRect.Shrink(CONTENT_PADDING_X, CONTENT_PADDING_X, CONTENT_PADDING_Y, CONTENT_PADDING_Y);
			CellRenderer.DrawLabel(
				Message, contentRect,
				CONTENT_CHAR, CHAR_SIZE_CONTENT,
				0, 0, true, CONTENT_AL
			);

			// Buttons 
			var buttonRect = new RectInt(
				mainRect.x + BUTTON_PADDING_X,
				mainRect.y + BUTTON_PADDING_Y,
				mainRect.width - BUTTON_PADDING_X * 2,
				BUTTON_HEIGHT
			);
			int buttonCount = 1;
			if (Cancel != null) {
				if (CellRenderer.DrawButton(
					new RectInt(
						buttonRect.xMax - buttonCount * (BUTTON_WIDTH + BUTTON_GAP),
						buttonRect.y, BUTTON_WIDTH, BUTTON_HEIGHT
					),
					Label_Cancel, BUTTON_CHAR_SIZE,
					BUTTON_NORMAL, BUTTON_HIGHLIGHT, BUTTON_PRESS, BUTTON_CHAR,
					BUTTON_SPRITE
				)) {
					Cancel();
				}
				buttonCount++;
			}
			if (Alt != null) {
				if (CellRenderer.DrawButton(
					new RectInt(
						buttonRect.xMax - buttonCount * (BUTTON_WIDTH + BUTTON_GAP),
						buttonRect.y, BUTTON_WIDTH, BUTTON_HEIGHT
					),
					Label_Alt, BUTTON_CHAR_SIZE,
					BUTTON_NORMAL, BUTTON_HIGHLIGHT, BUTTON_PRESS, BUTTON_CHAR,
					BUTTON_SPRITE
				)) {
					Alt();
				}
				buttonCount++;
			}
			if (OK != null) {
				if (CellRenderer.DrawButton(
					new RectInt(
						buttonRect.xMax - buttonCount * (BUTTON_WIDTH + BUTTON_GAP),
						buttonRect.y, BUTTON_WIDTH, BUTTON_HEIGHT
					),
					Label_OK, BUTTON_CHAR_SIZE,
					BUTTON_NORMAL, BUTTON_HIGHLIGHT, BUTTON_PRESS, BUTTON_CHAR,
					BUTTON_SPRITE
				)) {
					OK();
				}
			}

		}


		#endregion




		#region --- API ---


		public DialogEntity () {
			Width = 2048;
			Height = 1024;
			Title = "Title";
			Message = "Message";
			Label_OK = "OK";
			Label_Cancel = "Cancel";
			Label_Alt = "Alt";
			OK = () => { Debug.Log("OK " + Random.value); };
			Alt = () => { Debug.Log("Alt " + Random.value); };
			Cancel = () => { Debug.Log("Cancel " + Random.value); };
		}


		public DialogEntity (
			int width, int height,
			string title, string message,
			string label_ok, string label_cancel, string label_alt,
			System.Action ok = null, System.Action cancel = null, System.Action alt = null
		) {
			Width = width;
			Height = height;
			Title = title;
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




		#endregion




	}
}
