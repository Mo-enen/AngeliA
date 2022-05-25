using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eStove : eFurniture {


		private static readonly int[] CODES = new int[] { "Stove 0".AngeHash(), "Stove 1".AngeHash(), "Stove 2".AngeHash(), "Stove 3".AngeHash(), };

		protected override Direction3 ModuleType => Direction3.None;
		protected override int[] ArtworkCodes_LeftDown => CODES;
		protected override int[] ArtworkCodes_Mid => CODES;
		protected override int[] ArtworkCodes_RightUp => CODES;
		protected override int[] ArtworkCodes_Single => CODES;

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
				for (int i = 1; i <= CODES.Length; i++) {
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
				ArtworkIndex = LeftStoveCount % CODES.Length;
			}
		}


	}
}
