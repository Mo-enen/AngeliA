using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eRock : Entity {

		private static readonly int[] CODES = new int[] {
			"Rock 0".AngeHash(), "Rock 1".AngeHash(), "Rock 2".AngeHash(),
			"Rock 3".AngeHash(), "Rock 4".AngeHash(), "Rock 5".AngeHash(),
			"Rock 6".AngeHash(), "Rock 7".AngeHash(), "Rock 8".AngeHash(),
			"Rock 9".AngeHash(), "Rock 10".AngeHash(), "Rock 11".AngeHash(), "Rock 12".AngeHash(),
		};
		private int ArtworkIndex = 0;


		public override void FillPhysics (int frame) {
			base.FillPhysics(frame);
			CellPhysics.FillEntity((int)PhysicsLayer.Environment, this);
		}


		public override void FrameUpdate (int frame) {
			CellRenderer.Draw(CODES[ArtworkIndex % CODES.Length], Rect);
			base.FrameUpdate(frame);
		}

	}
}
