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
			int sideSpeedOffset = AdditionalBulletSpeedSide;
			int rotOffset = 0;
			int sidePosOffset = 0;

			// Get Bullet Angle Offset
			if (bulletCount > 1 && AngleSpeedDelta != 0) {
				int deltaAngle = (int)Util.Atan(bullet.SpeedForward, bullet.SpeedSide);
				int offset = (i % 2 == 0 ? 1 : -1) * ((i + 1) / 2);
				rotOffset = offset * deltaAngle;
				sideSpeedOffset += offset * AngleSpeedDelta;
				sidePosOffset = offset * bullet.Height / bulletCount;
			}

			// Move
			Direction8 dir8;
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

				default:
				case ProjectileValidDirection.Two: {
					bullet.StartMove(
						dir8 = sender.FacingRight ? Direction8.Right : Direction8.Left,
						bullet.SpeedForward + AdditionalBulletSpeedForward,
						bullet.SpeedSide + sideSpeedOffset
					);
					break;
				}

				case ProjectileValidDirection.Three: {
					if (aim.IsTop()) {
						int bSpeedY = !sender.FacingRight ? -bullet.SpeedSide : bullet.SpeedSide;
						bullet.StartMove(
							dir8 = Direction8.Top,
							bullet.SpeedForward + AdditionalBulletSpeedForward,
							bSpeedY + sideSpeedOffset
						);
					} else {
						bullet.StartMove(
							dir8 = sender.FacingRight ? Direction8.Right : Direction8.Left,
							bullet.SpeedForward + AdditionalBulletSpeedForward,
							bullet.SpeedSide + sideSpeedOffset
						);
					}
					break;
				}

				case ProjectileValidDirection.Four: {
					int bSpeedY = !sender.FacingRight && (aim.IsTop() || aim.IsBottom()) ? (-bullet.SpeedSide - sideSpeedOffset) : (bullet.SpeedSide + sideSpeedOffset);
					bullet.StartMove(dir8 = aim switch {
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
						int bSpeedY = !sender.FacingRight ? (-bullet.SpeedSide - sideSpeedOffset) : (bullet.SpeedSide + sideSpeedOffset);
						bullet.StartMove(
							dir8 = sender.FacingRight ? Direction8.Right : Direction8.Left,
							bullet.SpeedForward + AdditionalBulletSpeedForward,
							bSpeedY
						);
					} else {
						int bSpeedY = !sender.FacingRight && aim.IsTop() ? (-bullet.SpeedSide - sideSpeedOffset) : (bullet.SpeedSide + sideSpeedOffset);
						bullet.StartMove(
							dir8 = aim,
							bullet.SpeedForward + AdditionalBulletSpeedForward,
							bSpeedY
						);
					}
					break;
				}

				case ProjectileValidDirection.Eight: {
					int bSpeedY = !sender.FacingRight && (aim.IsTop() || aim.IsBottom()) ? (-bullet.SpeedSide - sideSpeedOffset) : (bullet.SpeedSide + sideSpeedOffset);
					bullet.StartMove(
						dir8 = aim,
						bullet.SpeedForward + AdditionalBulletSpeedForward,
						bSpeedY
					);
					break;
				}
			}

			// Apply Offset
			var sideNormal = dir8.Clockwise().Clockwise().Normal();
			bullet.CurrentRotation += rotOffset;
			bullet.X += sideNormal.x * sidePosOffset;
			bullet.Y += sideNormal.y * sidePosOffset;

		}

		return result;
	}

}
