using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace Yaya {
	public abstract class eOpenableFurniture : eFurniture {


		public bool Open { get; private set; } = false;
		public override bool AnimateOnHighlight => !Open;
		public override bool LockInput => Open;


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Open && !IsHighlighted) {
				Open = false;
			}
		}


		public override bool Invoke (Entity target) {
			if (!Open) {
				SetOpen(true);
			}
			return true;
		}


		public override void CancelInvoke (Entity target) {
			if (Open) SetOpen(false);
		}


		public override bool AllowInvoke (Entity target) => true;


		// LGC
		private void SetOpen (bool open) {
			Open = open;
			for (eFurniture i = FurnitureLeftOrDown; i != null; i = i.FurnitureLeftOrDown) {
				if (i is eOpenableFurniture oFur) oFur.Open = open;
			}
			for (eFurniture i = FurnitureRightOrUp; i != null; i = i.FurnitureRightOrUp) {
				if (i is eOpenableFurniture oFur) oFur.Open = open;
			}
		}


	}
}