using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eWardrobe : Entity {


		private static readonly int[] CODES = new int[] { "Wardrobe 0".AngeHash(), "Wardrobe 1".AngeHash(), "Wardrobe 2".AngeHash(), "Wardrobe 3".AngeHash(), };


		// Api
		public override int Capacity => 8;

		// Short
		private int Code => CODES[0];


		// MSG
		public override void OnActived (int frame) {
			base.OnActived(frame);
			if (CellRenderer.GetSprite(Code, out var rect)) {
				Width = rect.GlobalWidth;
				Height = rect.GlobalHeight;
			}
		}


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			CellRenderer.Draw(Code, Rect);
		}


	}
}
