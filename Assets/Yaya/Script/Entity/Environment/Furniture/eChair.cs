using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eChair : eFurniture {

		private static readonly int[] CODES = new int[] { "Chair 0".AngeHash(), "Chair 1".AngeHash(), };
		private static readonly int[] CODES_DOCKED = new int[] { "Chair Docked 0".AngeHash(), "Chair Docked 1".AngeHash(), };

		protected override Direction3 Direction => Direction3.None;
		protected override int[] ArtworkCodes_LeftDown => CODES;
		protected override int[] ArtworkCodes_Mid => CODES;
		protected override int[] ArtworkCodes_RightUp => CODES;
		protected override int[] ArtworkCodes_Single => DockPose == FurniturePose.Single ? CODES : CODES_DOCKED;

		private FurniturePose DockPose = FurniturePose.Unknown;


		public override void OnActived (int frame) {
			base.OnActived(frame);
			DockPose = FurniturePose.Unknown;
		}


		public override void PhysicsUpdate (int frame) {
			var oldRect = Rect;
			base.PhysicsUpdate(frame);
			if (DockPose == FurniturePose.Unknown) {
				var rect = oldRect;
				rect.x = oldRect.x - Const.CELL_SIZE;
				bool hasLeft = CellPhysics.HasEntity<eDiningTable>(rect, (int)PhysicsMask.Environment, this, OperationMode.ColliderAndTrigger);
				rect.x = oldRect.xMax;
				bool hasRight = CellPhysics.HasEntity<eDiningTable>(rect, (int)PhysicsMask.Environment, this, OperationMode.ColliderAndTrigger);
				DockPose =
					!hasLeft && hasRight ? FurniturePose.Right :
					hasLeft && !hasRight ? FurniturePose.Left :
					FurniturePose.Single;
				if (DockPose == FurniturePose.Left) {
					RenderingRect = oldRect;
					RenderingRect.x += RenderingRect.width;
					RenderingRect.width = -RenderingRect.width;
				}
			}
		}


	}
}
