using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {
	public abstract class eOpenableFurniture : eFurniture, IActionEntity {


		public bool Open { get; private set; } = false;
		protected override bool UseHighlightAnimation => !Open;



		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Open && !(this as IActionEntity).IsHighlighted) {
				Open = false;
			}
		}


		public bool Invoke (Entity target) {
			if (Open) return true;
			SetOpen(true);
			return true;
		}


		public bool CancelInvoke (Entity target) {
			if (!Open) return false;
			SetOpen(false);
			return true;
		}


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