using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eBox : BreakableRigidbody, ICombustible {
		public int BurnedDuration => 320;
		public int BurnStartFrame { get; set; }
		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, base.Rect);
		}

	}
}
