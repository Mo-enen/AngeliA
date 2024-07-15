using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA;

public enum ProjectileValidDirection { Two = 2, Four = 4, Eight = 8, }

public abstract class ProjectileWeapon<B> : Weapon<B> where B : MovableBullet {

	public virtual int BulletCountInOneShot => 1;
	protected virtual int BulletPivotY => 500;
	protected virtual int AdditionalBulletSpeedX => 0;
	protected virtual int AdditionalBulletSpeedY => 0;
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
				case ProjectileValidDirection.Two:
					bullet.StartMove(
						aim.IsRight() ? Direction8.Right : Direction8.Left,
						bullet.SpeedForward + AdditionalBulletSpeedX,
						bullet.SpeedSide + AdditionalBulletSpeedY
					);
					break;
				case ProjectileValidDirection.Four: {
					int bSpeedY = !sender.FacingRight && (aim.IsTop() || aim.IsBottom()) ? -bullet.SpeedSide : bullet.SpeedSide;
					bullet.StartMove(aim switch {
						Direction8.TopRight or Direction8.TopLeft => Direction8.Top,
						Direction8.BottomRight or Direction8.BottomLeft => Direction8.Bottom,
						_ => aim,
					}, bullet.SpeedForward + AdditionalBulletSpeedX, bSpeedY + AdditionalBulletSpeedY);
					break;
				}
				case ProjectileValidDirection.Eight: {
					int bSpeedY = !sender.FacingRight && (aim.IsTop() || aim.IsBottom()) ? -bullet.SpeedSide : bullet.SpeedSide;
					bullet.StartMove(
						aim,
						bullet.SpeedForward + AdditionalBulletSpeedX,
						bSpeedY + AdditionalBulletSpeedY
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
