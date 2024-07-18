using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA;

public enum ProjectileValidDirection {
	Two = 2,    // ← →
	Three = 3,  // ← → ↑
	Four = 4,   // ← → ↑ ↓
	Five = 5,   // ← → ↑ ↖ ↗
	Eight = 8,  // ← → ↑ ↖ ↗ ↓ ↙ ↘
}

public abstract class ProjectileWeapon<B> : Weapon<B> where B : MovableBullet {

	public virtual int BulletCountInOneShot => 1;
	protected virtual int BulletPivotY => 500;
	protected virtual int AdditionalBulletSpeedForward => 0;
	protected virtual int AdditionalBulletSpeedSide => 0;
	public virtual int AngleSpeedDelta => 0;
	protected virtual ProjectileValidDirection ValidDirection => ProjectileValidDirection.Two;
	protected int ForceBulletCountNextShot { get; set; } = -1;

	public override Bullet SpawnBullet (Character sender) {

		Bullet result = null;

		int bulletCount = ForceBulletCountNextShot < 0 ? BulletCountInOneShot : ForceBulletCountNextShot;
		ForceBulletCountNextShot = -1;

		// Spawn All Bullets
		for (int i = 0; i < bulletCount; i++) {
			if (base.SpawnBullet(sender) is not MovableBullet bullet) break;

			result = bullet;

			// Move
			bullet.X = sender.Rect.CenterX();
			bullet.Y = sender.Rect.yMin;
			int senderW = sender.Width;
			int senderH = sender.Height;
			var aim = sender.AimingDirection;

			if (aim.IsRight()) bullet.X += senderW / 2;
			if (aim.IsLeft()) bullet.X -= senderW / 2;
			if (aim.IsTop()) bullet.Y += senderH / 2;
			if (aim.IsBottom()) bullet.Y -= senderH / 2;

			if (!sender.FacingRight) bullet.X -= bullet.Width;

			bullet.Y += senderH * BulletPivotY / 1000 - bullet.Height / 2;
			bullet.Y = bullet.Y.Clamp(sender.Y + 1, sender.Rect.yMax - 1);

			switch (ValidDirection) {

				case ProjectileValidDirection.Two: {
					bullet.StartMove(
						sender.FacingRight ? Direction8.Right : Direction8.Left,
						bullet.SpeedForward + AdditionalBulletSpeedForward,
						bullet.SpeedSide + AdditionalBulletSpeedSide
					);
					break;
				}

				case ProjectileValidDirection.Three: {
					if (aim.IsTop()) {
						int bSpeedY = !sender.FacingRight ? -bullet.SpeedSide : bullet.SpeedSide;
						bullet.StartMove(
							Direction8.Top,
							bullet.SpeedForward + AdditionalBulletSpeedForward,
							bSpeedY + AdditionalBulletSpeedSide
						);
					} else {
						bullet.StartMove(
							sender.FacingRight ? Direction8.Right : Direction8.Left,
							bullet.SpeedForward + AdditionalBulletSpeedForward,
							bullet.SpeedSide + AdditionalBulletSpeedSide
						);
					}
					break;
				}

				case ProjectileValidDirection.Four: {
					int bSpeedY = !sender.FacingRight && (aim.IsTop() || aim.IsBottom()) ? (-bullet.SpeedSide - AdditionalBulletSpeedSide) : (bullet.SpeedSide + AdditionalBulletSpeedSide);
					bullet.StartMove(aim switch {
						Direction8.TopRight or Direction8.TopLeft => Direction8.Top,
						Direction8.BottomRight or Direction8.BottomLeft => Direction8.Bottom,
						_ => aim,
					},
						bullet.SpeedForward + AdditionalBulletSpeedForward,
						bSpeedY
					);
					break;
				}

				case ProjectileValidDirection.Five: {
					if (aim.IsBottom()) {
						int bSpeedY = !sender.FacingRight ? (-bullet.SpeedSide - AdditionalBulletSpeedSide) : (bullet.SpeedSide + AdditionalBulletSpeedSide);
						bullet.StartMove(
							sender.FacingRight ? Direction8.Right : Direction8.Left,
							bullet.SpeedForward + AdditionalBulletSpeedForward,
							bSpeedY
						);
					} else {
						int bSpeedY = !sender.FacingRight && aim.IsTop() ? (-bullet.SpeedSide - AdditionalBulletSpeedSide) : (bullet.SpeedSide + AdditionalBulletSpeedSide);
						bullet.StartMove(
							aim,
							bullet.SpeedForward + AdditionalBulletSpeedForward,
							bSpeedY
						);
					}
					break;
				}

				case ProjectileValidDirection.Eight: {
					int bSpeedY = !sender.FacingRight && (aim.IsTop() || aim.IsBottom()) ? (-bullet.SpeedSide - AdditionalBulletSpeedSide) : (bullet.SpeedSide + AdditionalBulletSpeedSide);
					bullet.StartMove(
						aim,
						bullet.SpeedForward + AdditionalBulletSpeedForward,
						bSpeedY
					);
					break;
				}
			}

			// Set Bullet Angle
			if (AngleSpeedDelta != 0) {
				int deltaAngle = (int)Util.Atan(bullet.SpeedForward, bullet.SpeedSide);
				int offset = (i % 2 == 0 ? 1 : -1) * ((i + 1) / 2);
				bullet.CurrentRotation += offset * deltaAngle;
				bullet.Velocity = new Int2(
					bullet.Velocity.x,
					bullet.Velocity.y + offset * AngleSpeedDelta
				);
				bullet.Y += offset * bullet.Height / bulletCount;
			}

		}

		return result;
	}

}
