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

		public int HighlightFrame { get; set; } = int.MinValue;

		protected override Direction3 ModuleType => Direction3.Horizontal;
		protected override int ArtworkCode_LeftDown => CODE_LEFT;
		protected override int ArtworkCode_Mid => CODE_MID;
		protected override int ArtworkCode_RightUp => CODE_RIGHT;
		protected override int ArtworkCode_Single => CODE_SINGLE;


		public void Invoke (Entity target) {
			if (target is not eCharacter ch) return;
			int bedX = Rect.x;
			if (Pose != FurniturePose.Left) {
				var rect = Rect;
				for (int i = 1; i < 1024; i++) {
					rect.x = X - i * Const.CELL_SIZE;
					if (CellPhysics.HasEntity<eBed>(rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly)) {
						bedX = rect.x;
					} else break;
				}
			}
			ch.Sleep();
			ch.X = bedX;
			ch.Y = Y;
		}


		public void Highlight () {
			HighlightFrame = Game.GlobalFrame;
			// Highlight all Neighbors
			for (eBed i = FurnitureLeftOrDown as eBed; i != null; i = i.FurnitureLeftOrDown as eBed) {
				i.HighlightFrame = Game.GlobalFrame;
			}
			for (eBed i = FurnitureRightOrUp as eBed; i != null; i = i.FurnitureRightOrUp as eBed) {
				i.HighlightFrame = Game.GlobalFrame;
			}
		}


		public override void FrameUpdate () {

			if (Pose == FurniturePose.Unknown) return;

			// Draw
			if (TryGetSprite(Pose, out var sprite)) {
				CellRenderer.Draw(sprite.GlobalID, RenderingRect);
				// Highlight
				if ((this as IActionEntity).IsHighlighted) {
					CellRenderer.SetLayer(YayaConst.SHADER_ADD);
					CellRenderer.Draw(sprite.GlobalID, RenderingRect);
					CellRenderer.SetLayerToDefault();
				}
			}
		}


	}
}
