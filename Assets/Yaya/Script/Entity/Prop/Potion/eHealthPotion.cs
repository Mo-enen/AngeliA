using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eHealthPotion : eItem {


		private static readonly int ITEM_CODE = "Potion Red".AngeHash();

		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			CellRenderer.Draw(ITEM_CODE, RenderRect);
		}


	}
}
