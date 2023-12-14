using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class BulletResidueParticle : FreeFallParticle {

		private static readonly int TYPE_ID = typeof(BulletResidueParticle).AngeHash();
		public override int Duration => _Duration;
		public bool Blink { get; set; } = false;
		private int _Duration = 1;

		[OnGameInitialize]
		public static void OnGameInitialize () {
			Bullet.OnResidueSpawn -= OnResidueSpawn;
			Bullet.OnResidueSpawn += OnResidueSpawn;
			static void OnResidueSpawn (Bullet bullet, IDamageReceiver receiver, int artwork) {
				if (bullet.Active || artwork == 0 || !CellRenderer.TryGetSprite(artwork, out var sprite)) return;
				if (Stage.SpawnEntity(TYPE_ID, bullet.X + bullet.Width / 2, bullet.Y + bullet.Height / 2) is not BulletResidueParticle particle) return;
				particle.UserData = artwork;
				particle.Width = sprite.GlobalWidth;
				particle.Height = sprite.GlobalHeight;
				if (bullet is MovableBullet mBullet) {
					if (mBullet.EndRotationRange == -1) {
						particle.Rotation = Util.QuickRandom(Game.GlobalFrame).UMod(360);
					} else if (mBullet.EndRotationRange == 0) {
						particle.Rotation = mBullet.EndRotation;
					} else {
						int endDelta = ((mBullet.CurrentRotation - mBullet.EndRotation + 180).UMod(360) - 180).Clamp(-mBullet.EndRotationRange, mBullet.EndRotationRange);
						particle.Rotation = mBullet.EndRotation + endDelta;
					}
					if (mBullet.Velocity.x < 0) {
						particle.Rotation += 180;
						particle.FlipX = true;
					}
					particle.CurrentSpeedX = -mBullet.Velocity.x / 2;
				} else {
					particle.Rotation = 0;
				}
				particle.AirDragX = 2;
				if (receiver == null) {
					// Environmnet
					particle.RotateSpeed = 0;
					particle.CurrentSpeedX = 0;
					particle.CurrentSpeedY = 0;
					particle._Duration = 30;
					particle.Gravity = 0;
					particle.Blink = true;
				} else {
					// Receiver
					particle.RotateSpeed = 12;
					particle.CurrentSpeedY = 42;
					particle._Duration = 60;
					particle.Gravity = 5;
					particle.Blink = false;
				}
			}
		}

		public override void FrameUpdate () {
			if (Blink && LocalFrame > Duration / 2 && LocalFrame % 6 < 3) return;
			base.FrameUpdate();
		}

	}
}