using System;
using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace AngeliA.Platformer;

/// <summary>
/// Weapon that shoot projectile to attack
/// </summary>
/// <typeparam name="B">Type of the bullet</typeparam>
public abstract class ProjectileWeapon<B> : Weapon<B> where B : MovableBullet {

	// VAR
	/// <summary>
	/// How many bullet does it shoot at once
	/// </summary>
	public virtual int BulletCountInOneShot => 1;
	/// <summary>
	/// Bullet start position Y. (0 means bottom of character hitbox. 1000 means top of character hitbox)
	/// </summary>
	protected virtual int BulletPivotY => 500;
	/// <summary>
	/// Extra speed apply to forward direction
	/// </summary>
	protected virtual int AdditionalBulletSpeedForward => 0;
	/// <summary>
	/// Extra speed apply to side direction
	/// </summary>
	protected virtual int AdditionalBulletSpeedSide => 0;
	/// <summary>
	/// How multiple bullets spread when spawned at once
	/// </summary>
	public virtual int AngleSpeedDelta => 0;
	/// <summary>
	/// Set this value to make "BulletCountInOneShot" different.
	/// </summary>
	protected int ForceBulletCountNextShot { get; set; } = -1;

	// MSG
	public override Bullet SpawnBullet (Character sender) {

		Bullet result = null;

		bool facingRight = sender.Movement.FacingRight;
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
			int bulletSpeedForware = bullet.SpeedForward + AdditionalBulletSpeedForward;
			int bulletSpeedSide = bullet.SpeedSide + sideSpeedOffset;

			// Move
			bullet.X = sender.Rect.CenterX();
			bullet.Y = sender.Rect.yMin;
			int senderW = sender.Width;
			int senderH = sender.Height;
			var aim = sender.Attackness.AimingDirection;

			if (aim.IsRight()) bullet.X += senderW / 2;
			if (aim.IsLeft()) bullet.X -= senderW / 2;
			if (aim.IsTop()) bullet.Y += senderH / 2;
			if (aim.IsBottom()) bullet.Y -= senderH / 2;

			if (!facingRight) bullet.X -= bullet.Width;
			if (aim.IsVertical()) {
				bulletSpeedSide = facingRight ? bulletSpeedSide : -bulletSpeedSide;
			} else {
				bullet.Y += senderH * BulletPivotY / 1000 - bullet.Height / 2;
			}
			bullet.Y = bullet.Y.Clamp(sender.Y + 1, sender.Rect.yMax - 1);

			bullet.CurrentRotation = facingRight ? bullet.StartRotation : -bullet.StartRotation;
			bullet.StartMove(aim, bulletSpeedForware, bulletSpeedSide);

			// Apply Offset
			var sideNormal = aim.Clockwise().Clockwise().Normal();
			bullet.CurrentRotation += rotOffset;
			bullet.X += sideNormal.x * sidePosOffset;
			bullet.Y += sideNormal.y * sidePosOffset;

		}

		return result;
	}

}
