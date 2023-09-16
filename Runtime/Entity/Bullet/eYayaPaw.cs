using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


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
		protected override int Duration => 10;
		protected override int Damage => 1;

		// Data
		private Color32 Tint = Const.WHITE;
		private bool FacingRight = false;
		private bool Grounded = true;


		// MSG
		public override void Release (Entity sender, int targetTeam, int speedX, int speedY, int combo, int chargeDuration) {
			base.Release(sender, targetTeam, speedX, speedY, combo, chargeDuration);
			if (sender == null) return;
			SpeedX = 0;
			SpeedY = 0;
			Width = 384;
			Height = 512;
			FacingRight = speedX > 0;
			var rect = sender.Rect;
			X = FacingRight ? rect.xMax : rect.xMin - Width;
			Y = sender.Y - 1;
			Tint = Const.WHITE;
			Grounded =
				CellPhysics.Overlap(Const.MASK_MAP, Rect.Edge(Direction4.Down, 4), out var hit, this) ||
				CellPhysics.Overlap(Const.MASK_MAP, Rect.Edge(Direction4.Down, 4), out hit, this, OperationMode.TriggerOnly, Const.ONEWAY_UP_TAG);
			if (Grounded && CellRenderer.TryGetSprite(hit.SourceID, out var groundSprite)) {
				Tint = groundSprite.SummaryTint;
			}
		}


		public override void FrameUpdate () {

			var characterRect = Sender.Rect;
			X = FacingRight ? characterRect.xMax : characterRect.xMin - Width;
			Y = Sender.Y - 1;

			int localFrame = (Game.GlobalFrame - SpawnFrame) / 2;
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
				int smokeFrame = (Game.GlobalFrame - SpawnFrame - 4).GreaterOrEquelThanZero();
				int _smokeFrame = smokeDuration * smokeDuration - (smokeDuration - smokeFrame) * (smokeDuration - smokeFrame);
				var tint = Tint;
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


		protected override void OnHit (IDamageReceiver receiver) {
			if (receiver is Entity e && !e.Active) return;
			receiver?.TakeDamage(Damage, Sender);
		}


	}
}
