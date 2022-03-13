using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class YayaWorldSquad : WorldSquad {


		public YayaWorldSquad (string mapRoot) : base(mapRoot) { }


		protected override void DrawLevelBlock (World.Block block, int unitX, int unitY) {
			base.DrawLevelBlock(block, unitX, unitY);
			if (block.HasCollider) {
				var rect = new RectInt(unitX * Const.CELL_SIZE, unitY * Const.CELL_SIZE, Const.CELL_SIZE, Const.CELL_SIZE);
				CellPhysics.FillBlock(
					(int)PhysicsLayer.Level,
					rect.Shrink(block.ColliderBorder.Left, block.ColliderBorder.Right, block.ColliderBorder.Down, block.ColliderBorder.Up),
					block.IsTrigger,
					block.Tag
				);
			}
		}


	}
}
