using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityCapacity(128)]
	public class eLadder : Entity {
		private static readonly int LADDER_CODE = "Ladder".AngeHash();
		public override RectInt Rect => new(X + Const.CELL_SIZE / 4, Y, Const.CELL_SIZE / 2, Const.CELL_SIZE);
		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.ENVIRONMENT, this, true, YayaConst.CLIMB_STABLE_TAG);
		}
		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(LADDER_CODE, new(X, Y, Const.CELL_SIZE, Const.CELL_SIZE));
		}
	}
}
