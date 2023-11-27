using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	public class Photo : Furniture, ICombustible, IActionTarget {

		private int PhotoIndex = 0;
		int ICombustible.BurnStartFrame { get; set; }

		public override void OnActivated () {
			base.OnActivated();
			PhotoIndex = Random.Range(0, int.MaxValue);
		}

		public override void FillPhysics () {
			CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		}

		public override void FrameUpdate () {
			if (CellRenderer.TryGetSpriteFromGroup(TypeID, PhotoIndex, out var sprite, true, false)) {
				var cell = CellRenderer.Draw(sprite.GlobalID, RenderingRect);
				if ((this as IActionTarget).IsHighlighted) {
					IActionTarget.HighlightBlink(cell, ModuleType, Pose);
				}
			}
		}

		void IActionTarget.Invoke () => PhotoIndex++;

		bool IActionTarget.AllowInvoke () => true;


	}
}
