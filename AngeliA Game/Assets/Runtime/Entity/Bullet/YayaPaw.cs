using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace AngeliaGame {
	[EntityAttribute.Capacity(12)]
	public class YayaPaw : MeleeBullet {


		// Const
		private static readonly int[] PAW_X = new int[] { -128, -96, 0, -16, -32, };
		private static readonly int[] PAW_Y = new int[] { 512, 480, 0, -64, -48, };
		private static readonly int[] PAW_SIZE = new int[] { 1200, 1600, 1800, 1400, 1000 };
		private static readonly int[] PAW_ROT = new int[] { -30, -15, 0, 15, 30, };
		private static readonly int SMOKE_ID = typeof(QuickSmokeBigParticle).AngeHash();
		public override int SmokeParticleID => SMOKE_ID;

		// MSG
		public override void FrameUpdate () {

			if (Sender is not Character character) return;

			bool facingRight = character.FacingRight;
			int localFrame = (Game.GlobalFrame - SpawnFrame) / 2;
			localFrame = localFrame.Clamp(0, PAW_Y.Length - 1);
			int spriteFrame = localFrame;
			int rot = PAW_ROT[localFrame];
			if (!GroundCheck(out _)) {
				spriteFrame = spriteFrame.Clamp(0, 1);
			} else {
				rot = PAW_ROT[localFrame.Clamp(0, 2)];
			}

			// Paw
			if (CellRenderer.TryGetSpriteFromGroup(TypeID, spriteFrame, out var sprite, false, true)) {
				CellRenderer.Draw(
					sprite,
					X + Width / 2 + (facingRight ? PAW_X[localFrame] : -PAW_X[localFrame]),
					Y + PAW_Y[localFrame],
					500, 0, facingRight ? rot : -rot,
					(facingRight ? sprite.GlobalWidth : -sprite.GlobalWidth) * PAW_SIZE[localFrame] / 1000,
					sprite.GlobalHeight * PAW_SIZE[localFrame] / 1000
				).Z = int.MaxValue;
			}

		}


	}
}