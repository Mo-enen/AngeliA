using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public interface IActionTarget {

		public virtual bool LockInput => false;
		public bool IsHighlighted => Player.Selecting != null && Player.Selecting.TargetActionEntity == this;
		public bool AllowInvokeOnStand => true;
		public bool AllowInvokeOnSquat => false;

		public void Invoke ();
		public bool AllowInvoke () => true;

		public static void HighlightBlink (Cell cell) {
			if (Game.GlobalFrame % 30 <= 15) {
				const int OFFSET = Const.CEL / 20;
				cell.Width += OFFSET * 2;
				cell.Height += OFFSET * 2;
			}
		}
		public static void HighlightBlink (Cell cell, Direction3 moduleType, FittingPose pose) {
			// Highlight
			int offset = Game.GlobalFrame % 30 > 15 ? 0 : Const.CEL / 20;
			if (moduleType == Direction3.Horizontal) {
				// Horizontal
				if (pose == FittingPose.Left || pose == FittingPose.Single) {
					cell.X -= offset;
				}
				if (pose != FittingPose.Mid) {
					if (pose == FittingPose.Left) {
						cell.Width += offset;
					} else {
						cell.Width += offset * 2;
					}
				}
				cell.Y -= offset;
				cell.Height += offset * 2;
			} else {
				// Vertical
				if (pose == FittingPose.Down || pose == FittingPose.Single) {
					cell.Y -= offset;
				}
				if (pose != FittingPose.Mid) {
					if (pose == FittingPose.Down) {
						cell.Height += offset;
					} else {
						cell.Height += offset * 2;
					}
				}
				cell.X -= offset;
				cell.Width += offset * 2;
			}
		}

	}
}
