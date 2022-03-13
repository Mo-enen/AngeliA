using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eLadder : eVine {
		private static readonly int VINE_CODE = "Ladder".AngeHash();
		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			CellRenderer.Draw(VINE_CODE, Rect);
		}
	}
}
