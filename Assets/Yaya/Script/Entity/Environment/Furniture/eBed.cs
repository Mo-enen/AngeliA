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
			ch.SetCharacterState(CharacterState.Sleep);
			var finalBed = this;
			for (int safe = 0; safe < 1024; safe++) {
				if (finalBed.FurnitureLeftOrDown is eBed leftBed) {
					finalBed = leftBed;
				} else break;
			}
			int offsetY = 0;
			if (TryGetSprite(Pose, out var sprite)) {
				offsetY += sprite.GlobalHeight - sprite.GlobalBorder.Up;
			}
			ch.X = finalBed.Rect.xMin;
			ch.Y = Y + offsetY;
			return true;
		}


		public bool CancelInvoke (Entity target) => false;


	}
}
