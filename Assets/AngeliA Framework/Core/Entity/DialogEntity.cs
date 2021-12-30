using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AngeliaFramework {
	public class DialogEntity : Entity {




		#region --- VAR ---


		// Const
		private static readonly int PIXEL_ID = "Pixel".GetAngeliaHashCode();

		// Api
		public override bool Despawnable => false;

		// Data
		public int Width;
		public int Height;
		public int CharSize_Title;
		public int CharSize_Content;
		public string Title = "";
		public string Message = "";
		public string Label_OK = "";
		public string Label_Cancel = "";
		public string Label_Alt = "";
		private System.Action OK = null;
		private System.Action Cancel = null;
		private System.Action Alt = null;


		public int TITLE_HEIGHT = 128;
		public int TITLE_PADDING = 64;
		public string TITLE_SPRITE = "Pixel";
		public Color32 TITLE_BG = new(0, 153, 188, 255);
		public Color32 TITLE_CHAR = new(255, 255, 255, 255);

		public Color32 WINDOW_BG = new(240, 240, 240, 255);
		public string WINDOW_SPRITE = "Pixel";

		public Color32 CONTENT_CHAR = new(12, 12, 12, 255);
		public int CONTENT_PADDING_X = 164;
		public int CONTENT_PADDING_Y = 128;

		public int BUTTON_WIDTH = 512;
		public int BUTTON_HEIGHT = 256;
		public int BUTTON_PADDING_X = 32;
		public int BUTTON_PADDING_Y = 32;
		public int BUTTON_GAP = 64;
		public string BUTTON_SPRITE = "Pixel";
		public int BUTTON_CHAR_SIZE = 110;
		public Color32 BUTTON_NORMAL = new(12, 12, 12, 255);
		public Color32 BUTTON_HIGHLIGHT = new(24, 24, 24, 255);
		public Color32 BUTTON_CHAR = new(12, 12, 12, 255);


		#endregion




		#region --- MSG ---


		public override void FrameUpdate () {

			X = ViewRect.x + (CellRenderer.CameraRect.width - Width) / 2;
			Y = ViewRect.y + (CellRenderer.CameraRect.height - Height) / 2;

			// Title
			var titleRect = new RectInt(X, Y + Height - TITLE_HEIGHT, Width, TITLE_HEIGHT);
			CellRenderer.Draw(
				TITLE_SPRITE.GetAngeliaHashCode(),
				titleRect.x, titleRect.y, 0, 0, 0,
				titleRect.width, titleRect.height, TITLE_BG
			);
			CellRenderer.DrawLabel(
				Title,
				titleRect.Shrink(TITLE_PADDING, 0, 0, 0),
				TITLE_CHAR, CharSize_Title, 0, 0, false, true
			);

			// Main
			var mainRect = new RectInt(X, Y, Width, Height - TITLE_HEIGHT);
			CellRenderer.Draw(
				WINDOW_SPRITE.GetAngeliaHashCode(),
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
				CONTENT_CHAR, CharSize_Content,
				0, 0, true, true
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
					"Cancel", BUTTON_CHAR_SIZE,
					BUTTON_NORMAL, BUTTON_HIGHLIGHT, BUTTON_CHAR,
					BUTTON_SPRITE.GetAngeliaHashCode()
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
					"Alt", BUTTON_CHAR_SIZE,
					BUTTON_NORMAL, BUTTON_HIGHLIGHT, BUTTON_CHAR,
					BUTTON_SPRITE.GetAngeliaHashCode()
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
					"OK", BUTTON_CHAR_SIZE,
					BUTTON_NORMAL, BUTTON_HIGHLIGHT, BUTTON_CHAR,
					BUTTON_SPRITE.GetAngeliaHashCode()
				)) {
					OK();
				}
			}

		}


		#endregion




		#region --- API ---


		public DialogEntity () {

			X = 512;
			Y = 512;
			Width = 2048;
			Height = 1024;
			CharSize_Title = 90;
			CharSize_Content = 120;
			Title = "Title";
			Message = "Message";
			Label_OK = "OK";
			Label_Cancel = "Cancel";
			Label_Alt = "Alt";
			OK = () => { Debug.Log("OK"); };
			Alt = () => { Debug.Log("Alt"); };
			Cancel = () => { Debug.Log("Cancel"); };

			TITLE_HEIGHT = 128;
			TITLE_PADDING = 64;
			TITLE_SPRITE = "Pixel";
			TITLE_BG = new(0, 153, 188, 255);
			TITLE_CHAR = new(255, 255, 255, 255);

			WINDOW_BG = new(240, 240, 240, 255);
			WINDOW_SPRITE = "Pixel";

			CONTENT_CHAR = new(12, 12, 12, 255);
			CONTENT_PADDING_X = 164;
			CONTENT_PADDING_Y = 128;

			BUTTON_WIDTH = 512;
			BUTTON_HEIGHT = 128;
			BUTTON_PADDING_X = 32;
			BUTTON_PADDING_Y = 32;
			BUTTON_GAP = 42;
			BUTTON_SPRITE = "Pixel";
			BUTTON_CHAR_SIZE = 100;
			BUTTON_NORMAL = new(220, 220, 220, 255);
			BUTTON_HIGHLIGHT = new(200, 200, 200, 255);
			BUTTON_CHAR = new(12, 12, 12, 255);
		}


		public DialogEntity (
			int width, int height, int charSize_title, int charSize_content,
			string title, string message, string label_ok, string label_cancel, string label_alt,
			System.Action ok = null, System.Action cancel = null, System.Action alt = null
		) {
			Width = width;
			Height = height;
			CharSize_Title = charSize_title;
			CharSize_Content = charSize_content;
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
