using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class BulletParticle : FreeFallParticle {

		private static readonly int TYPE_ID = typeof(BulletParticle).AngeHash();
		public override int Duration => _Duration;
		private int _Duration = 1;

		[OnGameInitialize]
		public static void OnGameInitialize () {
			Bullet.OnBulletHit -= OnBulletHit;
			Bullet.OnBulletHit += OnBulletHit;
			static void OnBulletHit (Bullet bullet, IDamageReceiver receiver, int artwork) {
				if (bullet.Active || artwork == 0 || !CellRenderer.TryGetSprite(artwork, out var sprite)) return;
				if (Stage.SpawnEntity(TYPE_ID, bullet.X + bullet.Width / 2, bullet.Y + bullet.Height / 2) is not BulletParticle particle) return;
				particle.UserData = artwork;
				particle.Width = sprite.GlobalWidth;
				particle.Height = sprite.GlobalHeight;
				if (bullet is MovableBullet mBullet) {
					particle.Rotation = mBullet.CurrentRotation;
					particle.CurrentSpeedX = mBullet.Velocity.x / 2;
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
				} else {
					// Receiver
					particle.RotateSpeed = 42;
					particle.CurrentSpeedY = 42;
					particle._Duration = 60;
					particle.Gravity = 5;
				}
			}
		}

		public override void FrameUpdate () {
			// Blink

			base.FrameUpdate();
		}

	}
}