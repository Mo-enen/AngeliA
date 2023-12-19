using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.Capacity(4, 0)]
	public abstract class MovableBullet : Bullet {

		// Api
		protected virtual int SpeedX => 42;
		protected virtual int SpeedY => 0;
		protected virtual Int2 AriDrag => default;
		protected virtual int Gravity => 0;
		protected virtual int StartRotation => 0;
		protected virtual int RotateSpeed => 0;
		protected virtual int EndRotation => 0;
		protected virtual int EndRotationRandomRange => 180;
		protected virtual int ResidueParticleID => 0;
		protected virtual int ArtworkID => TypeID;
		protected override int Duration => 600;
		protected override bool DestroyOnHitEnvironment => true;
		protected override bool DestroyOnHitReceiver => true;

		// Data
		private Int2 Velocity;
		private int Rotation;

		// MSG
		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();

			if (!Active) return;

			X += Velocity.x;
			Y += Velocity.y;
			if (AriDrag != default) {
				Velocity = Velocity.MoveTowards(Int2.zero, AriDrag);
			}
			if (Gravity > 0) {
				Velocity = new Int2(Velocity.x, Velocity.y - Gravity);
			}

			// Out of Range Check
			if (!Stage.ViewRect.Overlaps(Rect)) {
				Active = false;
				return;
			}

			// Oneway Check
			var hits = CellPhysics.OverlapAll(EnvironmentMask, Rect, out int count, Sender, OperationMode.TriggerOnly);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Tag == 0 || !AngeUtil.TryGetOnewayDirection(hit.Tag, out var onewayDir)) continue;
				// Collide Check X
				bool collide = false;
				if (Velocity.x > 0) {
					int xMax = Rect.xMax;
					if (onewayDir == Direction4.Left && xMax >= hit.Rect.xMin && xMax - Velocity.x <= hit.Rect.xMin) {
						collide = true;
					}
				} else if (Velocity.x < 0) {
					int xMin = Rect.xMin;
					if (onewayDir == Direction4.Right && xMin <= hit.Rect.xMax && xMin - Velocity.x >= hit.Rect.xMax) {
						collide = true;
					}
				}
				// Collide Check Y
				if (!collide) {
					if (Velocity.y > 0) {
						int yMax = Rect.yMax;
						if (onewayDir == Direction4.Down && yMax >= hit.Rect.yMin && yMax - Velocity.y <= hit.Rect.yMin) {
							collide = true;
						}
					} else if (Velocity.y < 0) {
						int yMin = Rect.yMin;
						if (onewayDir == Direction4.Up && yMin <= hit.Rect.yMax && yMin - Velocity.y >= hit.Rect.yMax) {
							collide = true;
						}
					}
				}
				// Collide with Oneway
				if (collide) {
					if (DestroyOnHitEnvironment) Active = false;
					SpawnResidue(null);
					break;
				}
			}
		}

		public override void PhysicsUpdate () {
			int stepCount = Mathf.Max(Velocity.x.Abs().CeilDivide(Width), Velocity.y.Abs().CeilDivide(Height));
			if (stepCount <= 1) {
				ReceiverHitCheck(Rect);
			} else {
				var rect = Rect;
				int fromX = X - Velocity.x;
				int fromY = Y - Velocity.y;
				for (int i = 0; i < stepCount; i++) {
					rect.x = Util.RemapUnclamped(0, stepCount, fromX, X, i + 1);
					rect.y = Util.RemapUnclamped(0, stepCount, fromY, Y, i + 1);
					ReceiverHitCheck(rect);
					if (!Active) break;
				}
			}
		}

		public override void FrameUpdate () {
			base.FrameUpdate();
			if (CellRenderer.TryGetSprite(ArtworkID, out var sprite)) {
				Rotation += Velocity.x.Sign() * RotateSpeed;
				CellRenderer.Draw(
					ArtworkID,
					X + Width / 2, Y + Height / 2,
					sprite.PivotX, sprite.PivotY, Rotation,
					Velocity.x.Sign() * sprite.GlobalWidth, sprite.GlobalHeight
				);
			}
		}

		protected override void SpawnResidue (IDamageReceiver receiver) {

			base.SpawnResidue(receiver);

			int particleID = ResidueParticleID != 0 ? ResidueParticleID : FreeFallParticle.TYPE_ID;
			if (Stage.SpawnEntity(particleID, X + Width / 2, Y + Height / 2) is not FreeFallParticle particle) return;
			if (!CellRenderer.TryGetSprite(TypeID, out var sprite)) return;

			particle.ArtworkID = TypeID;
			particle.Width = sprite.GlobalWidth;
			particle.Height = sprite.GlobalHeight;

			if (EndRotationRandomRange == -1) {
				particle.Rotation = Util.QuickRandom(Game.GlobalFrame).UMod(360);
			} else if (EndRotationRandomRange == 0) {
				particle.Rotation = EndRotation;
			} else {
				int endDelta = ((Rotation - EndRotation + 180).UMod(360) - 180).Clamp(-EndRotationRandomRange, EndRotationRandomRange);
				particle.Rotation = EndRotation + endDelta;
			}
			if (Velocity.x < 0) {
				particle.Rotation += 180;
				particle.FlipX = true;
			}
			particle.CurrentSpeedX = -Velocity.x / 2;

			particle.AirDragX = 2;
			if (receiver == null) {
				// Environmnet
				particle.RotateSpeed = 0;
				particle.CurrentSpeedX = 0;
				particle.CurrentSpeedY = 0;
				particle.Gravity = 0;
			} else {
				// Receiver
				particle.RotateSpeed = 12;
				particle.CurrentSpeedY = 42;
				particle.Gravity = 5;
			}
		}

		// API
		public void StartMove (bool facingRight) {
			Velocity.x = facingRight ? SpeedX : -SpeedX;
			Velocity.y = SpeedY;
			Rotation = facingRight ? StartRotation : -StartRotation;
		}

	}
}