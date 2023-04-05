using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.Capacity(32)]
	public class eBarrel : BreakableRigidbody {


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, base.Rect);
		}


	}
}
