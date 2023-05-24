using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace AngeliaGame {
	[EntityAttribute.Capacity(12)]
	public class eYayaPaw : Bullet {


		// Const
		private static readonly int[] PAW_X = new int[] { -128, -96, 0, -16, -32, };
		private static readonly int[] PAW_Y = new int[] { 512, 480, 0, -64, -48, };
		private static readonly int[] PAW_SIZE = new int[] { 1200, 1600, 1800, 1400, 1000 };
		private static readonly int[] PAW_ROT = new int[] { -30, -15, 0, 15, 30, };
		private static readonly int SMOKE_CODE = "YayaPaw Smoke".AngeHash();

		// Api
		protected override bool DestroyOnCollide => false;
		protected override bool DestroyOnHit => false;
		protected override int Duration => 10;
		protected override int Speed => 0;

		// Data
		private bool FacingRight = false;
		private bool Grounded = true;
		private Character Character = null;


		// MSG
		public override void Release (Entity entity, int team, Vector2Int direction, int combo, int chargeDuration) {
			base.Release(entity, team, direction, combo, chargeDuration);
			if (entity == null || entity is not AngeliaFramework.Character) return;
			Width = 384;
			Height = 512;
			Character = entity as Character;
			FacingRight = Character.FacingRight;
			var rect = entity.Rect;
			X = FacingRight ? rect.xMax : rect.xMin - Width;
			Y = entity.Y - 1;
			Grounded =
				CellPhysics.Overlap(Const.MASK_MAP, Rect.Edge(Direction4.Down, 4), this) ||
				CellPhysics.Overlap(Const.MASK_MAP, Rect.Edge(Direction4.Down, 4), this, OperationMode.TriggerOnly, Const.ONEWAY_UP_TAG);
		}


		public override void FillPhysics () {
			if (Game.GlobalFrame - StartFrame >= 4) {
				base.FillPhysics();
			}
		}


		public override void FrameUpdate () {

			var characterRect = Character.Rect;
			X = Character.FacingRight ? characterRect.xMax : characterRect.xMin - Width;
			Y = Character.Y - 1;


			int localFrame = (Game.GlobalFrame - StartFrame) / 2;
			localFrame = localFrame.Clamp(0, PAW_Y.Length - 1);
			int spriteFrame = localFrame;
			int rot = PAW_ROT[localFrame];
			if (!Grounded) {
				spriteFrame = spriteFrame.Clamp(0, 1);
			} else {
				rot = PAW_ROT[localFrame.Clamp(0, 2)];
			}

			// Paw
			if (CellRenderer.TryGetSpriteFromGroup(TypeID, spriteFrame, out var sprite, false, true)) {
				CellRenderer.Draw(
					sprite.GlobalID,
					X + Width / 2 + (FacingRight ? PAW_X[localFrame] : -PAW_X[localFrame]),
					Y + PAW_Y[localFrame],
					500, 0, FacingRight ? rot : -rot,
					(FacingRight ? sprite.GlobalWidth : -sprite.GlobalWidth) * PAW_SIZE[localFrame] / 1000,
					sprite.GlobalHeight * PAW_SIZE[localFrame] / 1000
				).Z = int.MaxValue;
			}

			// Smoke 
			if (Grounded && localFrame >= 2) {
				int smokeDuration = Duration - 4;
				int smokeFrame = (Game.GlobalFrame - StartFrame - 4).LargerThanZero();
				int _smokeFrame = smokeDuration * smokeDuration - (smokeDuration - smokeFrame) * (smokeDuration - smokeFrame);
				var tint = new Color32(255, 255, 255, 255);
				if (CellRenderer.TryGetSprite(Character.GroundedID, out var groundSprite)) {
					tint = groundSprite.SummaryTint;
				}
				tint.a = (byte)Util.Remap(0, smokeDuration, 512, 0, smokeFrame);
				var cell = CellRenderer.Draw(
					SMOKE_CODE, X + Width / 2, Y,
					500, 500, (smokeFrame + Game.GlobalFrame) * 12,
					Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE, tint
				);
				cell.X += (FacingRight ? _smokeFrame : -_smokeFrame) * 6;
				cell.Y += cell.Height / 2;
				cell.Z = int.MaxValue - 1;
				if (!FacingRight) cell.Width *= -1;
				cell.Width = Util.Remap(0, smokeDuration, cell.Width, cell.Width * 2, smokeFrame);
				cell.Height = Util.Remap(0, smokeDuration, cell.Height, cell.Height * 2, smokeFrame);
			}

		}


	}
}