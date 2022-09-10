using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eBed : eFurniture, IActionEntity {


		private static readonly int CODE_LEFT = "Bed Left".AngeHash();
		private static readonly int CODE_MID = "Bed Mid".AngeHash();
		private static readonly int CODE_RIGHT = "Bed Right".AngeHash();
		private static readonly int CODE_SINGLE = "Bed Single".AngeHash();

		protected override Direction3 ModuleType => Direction3.Horizontal;
		protected override int ArtworkCode_LeftDown => CODE_LEFT;
		protected override int ArtworkCode_Mid => CODE_MID;
		protected override int ArtworkCode_RightUp => CODE_RIGHT;
		protected override int ArtworkCode_Single => CODE_SINGLE;


		public bool Invoke (Entity target) {
			if (target is not eCharacter ch) return false;
			int bedX = Rect.x;
			if (Pose != FurniturePose.Left) {
				var rect = Rect;
				for (int i = 1; i < 1024; i++) {
					rect.x = X - i * Const.CELL_SIZE;
					if (CellPhysics.HasEntity<eBed>(rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly)) {
						bedX = rect.x;
					} else break;
				}
			}
			ch.Sleep();
			ch.X = bedX;
			ch.Y = Y;
			return true;
		}


		public bool CancelInvoke (Entity target) => false;


	}
}
