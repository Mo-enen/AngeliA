using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eStoveCabinet : eFurniture {


		private static readonly int[] CODES = new int[] { "Stove Cabinet 0".AngeHash(), "Stove Cabinet 1".AngeHash(), "Stove Cabinet 2".AngeHash(), "Stove Cabinet 3".AngeHash(), };

		protected override Direction3 ModuleType => Direction3.None;
		protected override int[] ArtworkCodes_LeftDown => CODES;
		protected override int[] ArtworkCodes_Mid => CODES;
		protected override int[] ArtworkCodes_RightUp => CODES;
		protected override int[] ArtworkCodes_Single => CODES;

		private int CabinetLeft = -1;
		private int CabinetRight = -1;



		public override void OnActived (int frame) {
			base.OnActived(frame);
			CabinetLeft = -1;
			CabinetRight = -1;
		}


		public override void PhysicsUpdate (int frame) {
			base.PhysicsUpdate(frame);
			if (CabinetLeft < 0 || CabinetRight < 0) {
				// L
				CabinetLeft = 0;
				for (int i = 1; i <= CODES.Length; i++) {
					var rect = Rect.Shift(-i * Const.CELL_SIZE, 0);
					if (CellPhysics.HasEntity<eStoveCabinet>(
						rect, (int)PhysicsMask.Environment, this, OperationMode.TriggerOnly
					)) {
						CabinetLeft++;
					} else break;
				}
				// R
				CabinetRight = 0;
				for (int i = 1; i <= CODES.Length; i++) {
					var rect = Rect.Shift(i * Const.CELL_SIZE, 0);
					if (CellPhysics.HasEntity<eStoveCabinet>(
						rect, (int)PhysicsMask.Environment, this, OperationMode.TriggerOnly
					)) {
						CabinetRight++;
					} else break;
				}
				// Index
				ArtworkIndex = (CabinetLeft + CabinetRight).Clamp(0, CODES.Length - 1);
			}
		}


	}
}
