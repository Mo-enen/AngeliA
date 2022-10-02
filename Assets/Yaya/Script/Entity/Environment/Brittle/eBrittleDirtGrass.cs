using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eBrittleDirtGrass : eBrittle {


		protected override BreakMode BreakCondition => BreakMode.BreakOnCollideGround;
		protected override int HoldDuration => 60;


		public override void OnActived () {
			base.OnActived();
			Height = Const.CELL_SIZE / 2;
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			int frame = Game.GlobalFrame;
			var tint = IsHolding || IsFalling ? new Color32(255, 196, 164, 255) : new Color32(255, 255, 255, 255);
			int rot = IsHolding ? GetHoldedFrame(frame * 4).PingPong(12) - 6 : 0;
			CellRenderer.Draw(
				TypeID,
				X + OffsetX + Width / 2,
				Y + OffsetY + Height / 2,
				500, 250, rot, Width, Const.CELL_SIZE,
				tint
			);

		}


	}
}
