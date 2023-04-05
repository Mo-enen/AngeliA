using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eBox : BreakableRigidbody {

		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, base.Rect);
		}

	}
}
