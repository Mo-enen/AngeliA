using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eChair : eFurniture {


		// Const
		private static readonly int CODE_DOCKED = "Chair Docked".AngeHash();

		// Api
		protected override int ArtworkCode_Single => DockPose == FurniturePose.Single ? TrimedTypeID : CODE_DOCKED;
		protected override RectInt RenderingRect => DockPose == FurniturePose.Left ? base.RenderingRect.FlipHorizontal() : base.RenderingRect;

		// Data
		private FurniturePose DockPose = FurniturePose.Unknown;


		// MSG
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
			}
		}


	}
}
