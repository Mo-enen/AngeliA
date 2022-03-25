using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eBrittleDirtGrass : eBrittle {


		private static readonly int DIRT_GRASS_CODE = "Dirt Grass Brittle".AngeHash();
		protected override BreakMode BreakCondition => BreakMode.BreakOnCollideGround;
		protected override int HoldDuration => 60;


		public override void OnCreate (int frame) {
			Height = Const.CELL_SIZE / 2;
			base.OnCreate(frame);
		}


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			var tint = IsHolding || IsFalling ? new Color32(255, 196, 164, 255) : new Color32(255, 255, 255, 255);
			int rot = IsHolding ? GetHoldedFrame(frame * 4).PingPong(12) - 6 : 0;
			CellRenderer.Draw(
				DIRT_GRASS_CODE,
				X + OffsetX + Width / 2,
				Y + OffsetY + Height / 2,
				500, 250, rot, Width, Const.CELL_SIZE,
				tint
			);

		}


	}
}
