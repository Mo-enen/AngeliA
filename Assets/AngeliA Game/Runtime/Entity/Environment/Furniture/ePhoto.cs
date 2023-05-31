using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class ePhoto : Furniture, ICombustible, IActionEntity {

		private int PhotoIndex = 0;
		int ICombustible.BurnStartFrame { get; set; }

		public override void OnActivated () {
			base.OnActivated();
			PhotoIndex = Random.Range(0, int.MaxValue);
		}

		public override void FillPhysics () {
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
		}

		public override void FrameUpdate () {
			if (CellRenderer.TryGetSpriteFromGroup(TypeID, PhotoIndex, out var sprite, true, false)) {
				var cell = CellRenderer.Draw(sprite.GlobalID, RenderingRect);
				if ((this as IActionEntity).IsHighlighted) {
					IActionEntity.HighlightBlink(cell, ModuleType, Pose);
				}
			}
		}

		void IActionEntity.Invoke () => PhotoIndex++;

		bool IActionEntity.AllowInvoke () => true;


	}
}
