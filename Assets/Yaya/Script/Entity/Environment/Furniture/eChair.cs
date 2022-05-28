using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eChair : eFurniture {

		private static readonly int CODE = "Chair".AngeHash();
		private static readonly int CODE_DOCKED = "Chair Docked".AngeHash();

		protected override Direction3 ModuleType => Direction3.None;
		protected override int ArtworkCode_LeftDown => CODE;
		protected override int ArtworkCode_Mid => CODE;
		protected override int ArtworkCode_RightUp => CODE;
		protected override int ArtworkCode_Single => DockPose == FurniturePose.Single ? CODE : CODE_DOCKED;

		private FurniturePose DockPose = FurniturePose.Unknown;


		public override void OnActived () {
			base.OnActived();
			DockPose = FurniturePose.Unknown;
		}


		public override void PhysicsUpdate () {
			var oldRect = Rect;
			base.PhysicsUpdate();
			if (DockPose == FurniturePose.Unknown) {
				var rect = oldRect;
				rect.x = oldRect.x - Const.CELL_SIZE;
				bool hasLeft = CellPhysics.HasEntity<eDiningTable>(rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.ColliderAndTrigger);
				rect.x = oldRect.xMax;
				bool hasRight = CellPhysics.HasEntity<eDiningTable>(rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.ColliderAndTrigger);
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
