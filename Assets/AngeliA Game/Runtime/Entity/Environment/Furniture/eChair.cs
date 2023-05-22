using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace AngeliaGame {
	public class eChairWood : Furniture, ICombustible {


		// Const
		private static readonly int CODE_DOCKED = "ChairWood Docked".AngeHash();

		// Api
		protected override int ArtworkCode_Single => DockPose == FittingPose.Single ? TypeID : CODE_DOCKED;
		protected override RectInt RenderingRect {
			get {
				if (DockPose == FittingPose.Left) {
					var rect = base.RenderingRect;
					rect.FlipHorizontal();
					return rect;
				}
				return base.RenderingRect;
			}
		}
		int ICombustible.BurnStartFrame { get; set; }

		// Data
		private FittingPose DockPose = FittingPose.Unknown;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			DockPose = FittingPose.Unknown;
		}


		public override void PhysicsUpdate () {
			var oldRect = Rect;
			base.PhysicsUpdate();
			if (DockPose == FittingPose.Unknown) {
				var rect = oldRect;
				rect.x = oldRect.x - Const.CEL;
				bool hasLeft = CellPhysics.HasEntity<eDiningTable>(rect, Const.MASK_ENVIRONMENT, this, OperationMode.ColliderAndTrigger);
				rect.x = oldRect.xMax;
				bool hasRight = CellPhysics.HasEntity<eDiningTable>(rect, Const.MASK_ENVIRONMENT, this, OperationMode.ColliderAndTrigger);
				DockPose =
					!hasLeft && hasRight ? FittingPose.Right :
					hasLeft && !hasRight ? FittingPose.Left :
					FittingPose.Single;
			}
		}


	}
}
