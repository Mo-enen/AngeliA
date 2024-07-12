using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA;

public enum ProjectileValidDirection { Two = 2, Four = 4, Eight = 8, }

public abstract class ProjectileWeapon<B> : Weapon<B> where B : MovableBullet {
	protected virtual int BulletPivotY => 500;
	protected virtual int AdditionalBulletSpeedX => 0;
	protected virtual int AdditionalBulletSpeedY => 0;
	protected virtual ProjectileValidDirection ValidDirection => ProjectileValidDirection.Two;
	public override Bullet SpawnBullet (Character sender) {
		if (base.SpawnBullet(sender) is not MovableBullet bullet) return null;
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
					bullet.SpeedX + AdditionalBulletSpeedX,
					bullet.SpeedY + AdditionalBulletSpeedY
				);
				break;
			case ProjectileValidDirection.Four: {
				int bSpeedY = !sender.FacingRight && (aim.IsTop() || aim.IsBottom()) ? -bullet.SpeedY : bullet.SpeedY;
				bullet.StartMove(aim switch {
					Direction8.TopRight or Direction8.TopLeft => Direction8.Top,
					Direction8.BottomRight or Direction8.BottomLeft => Direction8.Bottom,
					_ => aim,
				}, bullet.SpeedX + AdditionalBulletSpeedX, bSpeedY + AdditionalBulletSpeedY);
				break;
			}
			case ProjectileValidDirection.Eight: {
				int bSpeedY = !sender.FacingRight && (aim.IsTop() || aim.IsBottom()) ? -bullet.SpeedY : bullet.SpeedY;
				bullet.StartMove(
					aim,
					bullet.SpeedX + AdditionalBulletSpeedX,
					bSpeedY + AdditionalBulletSpeedY
				);
				break;
			}
		}

		return bullet;
	}
}
