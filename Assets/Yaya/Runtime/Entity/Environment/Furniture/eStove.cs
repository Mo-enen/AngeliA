using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eStove : eFurniture {




		private int LeftStoveCount = -1;


		public override void OnActived () {
			base.OnActived();
			LeftStoveCount = -1;
		}


		public override void FillPhysics () {
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (LeftStoveCount < 0) {
				LeftStoveCount = 0;
				for (int i = 1; i < 1024; i++) {
					var rect = new RectInt(
						X + Const.CEL / 2 - i * Const.CEL,
						Y + Const.CEL / 2,
						1, 1
					);
					if (CellPhysics.HasEntity<eStove>(
						rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly
					)) {
						LeftStoveCount++;
					} else break;
				}
				ArtworkIndex = LeftStoveCount;
			}
		}


	}
}
