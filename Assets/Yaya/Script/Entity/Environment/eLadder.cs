using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.Capacity(128)]
	public class eLadder : Entity {


		private static readonly int LADDER_CODE = "Ladder".AngeHash();

		public override RectInt Rect => new(
			X,
			Y,
			Const.CELL_SIZE,
			HasLadderOnTop.HasValue && HasLadderOnTop.Value ? Const.CELL_SIZE : Const.CELL_SIZE / 4
		);

		private bool? HasLadderOnTop = null;


		public override void OnActived () {
			base.OnActived();
			HasLadderOnTop = null;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true, YayaConst.CLIMB_STABLE_TAG);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (!HasLadderOnTop.HasValue) {
				HasLadderOnTop = CellPhysics.HasEntity<eLadder>(
					Rect.Shift(0, Const.CELL_SIZE),
					YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly, YayaConst.CLIMB_STABLE_TAG
				);
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
            CellRenderer.Draw(LADDER_CODE, new(X, Y, Const.CELL_SIZE, Const.CELL_SIZE));
		}


	}
}
