using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eBarrel : BreakableRigidbody, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}


	public class eBarrelIron : Rigidbody, IDamageReceiver {


		// Const
		private const int ROLL_SPEED = 12;

		// Api
		public int Team => Const.TEAM_ENVIRONMENT;
		protected override int PhysicsLayer => Const.LAYER_ENVIRONMENT;
		public override bool AllowBeingPush => false;

		// Data
		private bool Rolling = false;
		private int RollingSpeed = 0;
		private int RollingRotation = 0;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			Rolling = false;
			RollingSpeed = 0;
			RollingRotation = 0;
			const int SIZE_DELTA = 16;
			Width -= SIZE_DELTA;
			Height -= SIZE_DELTA;
			X += SIZE_DELTA / 2;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (Rolling && RollingSpeed != 0) {
				int prevX = X;
				PerformMove(RollingSpeed, 0);
				if (X == prevX) RollingSpeed = 0;
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (!Rolling) {
				// Normal
				if (CellRenderer.TryGetSpriteFromGroup(TypeID, 0, out var sprite, false, true)) {
					CellRenderer.Draw(sprite.GlobalID, Rect);
					AngeUtil.DrawShadow(sprite.GlobalID, Rect);
				}
			} else {
				// Rolling
				if (CellRenderer.TryGetSpriteFromGroup(TypeID, 1, out var sprite, false, true)) {
					RollingRotation += RollingSpeed;
					CellRenderer.Draw(
						sprite.GlobalID, X + Width / 2, Y + Height / 2, 500, 500, RollingRotation, Width, Height
					);
					AngeUtil.DrawShadow(sprite.GlobalID, Rect);
				}
			}
		}


		public override void Push (int speedX) {
			base.Push(speedX);
			if (RollingSpeed == 0) return;
			if (speedX.Sign() != RollingSpeed.Sign()) {
				RollingSpeed = 0;
				return;
			}
			RollingRotation += speedX;
			RollingSpeed = speedX.Sign3() * ROLL_SPEED;
		}


		void IDamageReceiver.TakeDamage (int damage, Entity sender) {
			if (damage <= 0) return;
			Rolling = true;
			RollingRotation = 0;
			if (sender is Character character) {
				RollingSpeed = (character.FacingRight ? 1 : -1) * ROLL_SPEED;
			} else {
				RollingSpeed = (Rect.CenterX() - sender.Rect.CenterX()).Sign3() * ROLL_SPEED;
			}
		}


	}
}
