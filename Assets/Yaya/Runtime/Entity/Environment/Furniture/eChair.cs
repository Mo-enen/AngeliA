using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eChair : eFurniture {


		// Const
		private static readonly int CODE_DOCKED = "Chair Docked".AngeHash();

		// Api
		protected override int ArtworkCode_Single => DockPose == FittingPose.Single ? TypeID : CODE_DOCKED;
		protected override RectInt RenderingRect => DockPose == FittingPose.Left ? base.RenderingRect.FlipHorizontal() : base.RenderingRect;

		// Data
		private FittingPose DockPose = FittingPose.Unknown;


		// MSG
		public override void OnActived () {
			base.OnActived();
			DockPose = FittingPose.Unknown;
		}


		public override void PhysicsUpdate () {
			var oldRect = Rect;
			base.PhysicsUpdate();
			if (DockPose == FittingPose.Unknown) {
				var rect = oldRect;
				rect.x = oldRect.x - Const.CEL;
				bool hasLeft = CellPhysics.HasEntity<eDiningTable>(rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.ColliderAndTrigger);
				rect.x = oldRect.xMax;
				bool hasRight = CellPhysics.HasEntity<eDiningTable>(rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.ColliderAndTrigger);
				DockPose =
					!hasLeft && hasRight ? FittingPose.Right :
					hasLeft && !hasRight ? FittingPose.Left :
					FittingPose.Single;
			}
		}


	}
}
