using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eBrittleDirt : eBrittle {


		private static readonly int DIRT_CODE = "Dirt Brittle".AngeHash();
		protected override BreakMode BreakCondition => BreakMode.BreakOnCollideGround;
		protected override int HoldDuration => 60;


		public override void FrameUpdate () {
			base.FrameUpdate();
			int frame = Game.GlobalFrame;
			var tint = IsHolding || IsFalling ? new Color32(255, 196, 164, 255) : new Color32(255, 255, 255, 255);
			int rot = IsHolding ? GetHoldedFrame(frame * 4).PingPong(12) - 6 : 0;
            AngeliaFramework.Renderer.Draw(
                DIRT_CODE,
                X + OffsetX + Width / 2,
                Y + OffsetY + Height / 2, 500, 500,
				rot, Width, Height, tint
			);
		}


	}
}
