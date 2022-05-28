using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eStove : eFurniture {


		private static readonly int CODE = "Stove".AngeHash();

		protected override Direction3 ModuleType => Direction3.None;
		protected override int ArtworkCode_LeftDown => CODE;
		protected override int ArtworkCode_Mid => CODE;
		protected override int ArtworkCode_RightUp => CODE;
		protected override int ArtworkCode_Single => CODE;

		private int LeftStoveCount = -1;


		public override void OnActived () {
			base.OnActived();
			LeftStoveCount = -1;
		}


		public override void FillPhysics () {
			CellPhysics.FillEntity(YayaConst.ENVIRONMENT, this, true);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (LeftStoveCount < 0) {
				LeftStoveCount = 0;
				for (int i = 1; i < 1024; i++) {
					var rect = new RectInt(
						X + Const.CELL_SIZE / 2 - i * Const.CELL_SIZE,
						Y + Const.CELL_SIZE / 2,
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
