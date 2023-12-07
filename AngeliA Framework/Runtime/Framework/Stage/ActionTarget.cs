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

		public static void HighlightBlink (Cell cell, float pivotX = 0.5f, float pivotY = 0f, bool horizontal = true, bool vertical = true) {
			if (Game.GlobalFrame % 30 > 15) return;
			const int OFFSET = Const.CEL / 20;
			cell.ReturnPivots(pivotX, pivotY);
			if (horizontal) cell.Width += OFFSET * 2;
			if (vertical) cell.Height += OFFSET * 2;
		}

	}
}
