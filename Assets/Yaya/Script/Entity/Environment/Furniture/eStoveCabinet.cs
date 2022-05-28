using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eStoveCabinet : eFurniture {


		private static readonly int CODE = "Stove Cabinet".AngeHash();

		protected override Direction3 ModuleType => Direction3.None;
		protected override int ArtworkCode_LeftDown => CODE;
		protected override int ArtworkCode_Mid => CODE;
		protected override int ArtworkCode_RightUp => CODE;
		protected override int ArtworkCode_Single => CODE;

		private int CabinetLeft = -1;
		private int CabinetRight = -1;



		public override void OnActived () {
			base.OnActived();
			CabinetLeft = -1;
			CabinetRight = -1;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (CabinetLeft < 0 || CabinetRight < 0) {
				// L
				CabinetLeft = 0;
				for (int i = 1; i < 1024; i++) {
					var rect = Rect.Shift(-i * Const.CELL_SIZE, 0);
					if (CellPhysics.HasEntity<eStoveCabinet>(
						rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly
					)) {
						CabinetLeft++;
					} else break;
				}
				// R
				CabinetRight = 0;
				for (int i = 1; i < 1024; i++) {
					var rect = Rect.Shift(i * Const.CELL_SIZE, 0);
					if (CellPhysics.HasEntity<eStoveCabinet>(
						rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly
					)) {
						CabinetRight++;
					} else break;
				}
				// Index
				ArtworkIndex = CabinetLeft + CabinetRight;
			}
		}


	}
}
