using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityBounds(0, -Const.CELL_SIZE * (LEAF_LENGTH - 1), Const.CELL_SIZE, Const.CELL_SIZE * LEAF_LENGTH)]
	public class eTreeWillow : eTree {


		// Const
		private const int LEAF_LENGTH = 10;

		// Api
		protected override string LeafCode => "Leaf Willow";
		private bool HasLeaf => Direction == Direction3.Horizontal;

		// Data
		private int GroundDistance = -1;


		public override void OnActived () {
			base.OnActived();
			GroundDistance = -1;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (GroundDistance < 0 && HasLeaf) {
				for (int i = 1; i <= LEAF_LENGTH + 1; i++) {
					GroundDistance = i;
					if (CellPhysics.Overlap(
						YayaConst.MASK_LEVEL,
						new(X + Const.CELL_SIZE / 2, Y + Const.CELL_SIZE / 2 - i * Const.CELL_SIZE, 1, 1),
						this
					)) break;
				}
			}
		}


		protected override void DrawLeaf () {
			if (!HasLeaf) return;
			for (int i = 0; i < GroundDistance - 1; i++) {
				var rect = Rect.Shift(GetLeafShiftY(i * 12), -i * Const.CELL_SIZE);
				CellRenderer.Draw(LeafArtworkCode, rect);
			}
		}


	}
}