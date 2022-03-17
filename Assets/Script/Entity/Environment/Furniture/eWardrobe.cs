using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eWardrobe : Entity {


		private static readonly int[] CODES = new int[] { "Wardrobe 0".AngeHash(), "Wardrobe 1".AngeHash(), "Wardrobe 2".AngeHash(), "Wardrobe 3".AngeHash(), };


		// Api
		public override int Layer => (int)EntityLayer.Environment;

		// Data
		private int CODE = 0;


		// MSG
		public override void OnCreate (int frame) {
			base.OnCreate(frame);
			CODE = CODES[Mathf.Clamp(Data, 0, CODES.Length - 1)];
			if (CellRenderer.GetUVRect(CODE, out var rect)) {
				Width = rect.Width;
				Height = rect.Height;
			}
		}


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			CellRenderer.Draw(CODE, Rect);
		}


	}
}
