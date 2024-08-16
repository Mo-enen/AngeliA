using System;
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


	[CheatCode("giveammo")]
	internal static bool Cheat_GiveAmmo () {
		var player = Player.Selecting;
		if (player == null) return false;
		if (player.GetEquippingItem(EquipmentType.Weapon, out int eqCount) is not Weapon weapon) return false;
		bool performed = false;
		// Fill Bullet
		if (
			Stage.GetEntityType(weapon.BulletID) is Type bulletType &&
			bulletType.IsSubclassOf(typeof(ArrowBullet))
		) {
			var bullet = Activator.CreateInstance(bulletType) as ArrowBullet;
			int itemID = bullet.ArrowItemID;
			if (ItemSystem.HasItem(itemID)) {
				int maxCount = ItemSystem.GetItemMaxStackCount(itemID);
				ItemSystem.GiveItemTo(player.TypeID, itemID, maxCount);
				performed = true;
			}
		}
		// Fill Weapon
		int eqMaxCount = ItemSystem.GetItemMaxStackCount(weapon.TypeID);
		if (eqMaxCount > eqCount) {
			Inventory.SetEquipment(player.TypeID, EquipmentType.Weapon, weapon.TypeID, eqMaxCount);
			performed = true;
		}
		return performed;
	}


	public override void PoseAnimationUpdate_FromEquipment (Entity holder) {
		base.PoseAnimationUpdate_FromEquipment(holder);
		if (holder is Character character) {
			switch (ValidDirection) {
				case ProjectileValidDirection.Two:
					character.IgnoreAimingDirection(Direction8.Bottom);
					character.IgnoreAimingDirection(Direction8.Top);
					character.IgnoreAimingDirection(Direction8.TopLeft);
					character.IgnoreAimingDirection(Direction8.TopRight);
					character.IgnoreAimingDirection(Direction8.BottomLeft);
					character.IgnoreAimingDirection(Direction8.BottomRight);
					break;
				case ProjectileValidDirection.Three:
					character.IgnoreAimingDirection(Direction8.Bottom);
					character.IgnoreAimingDirection(Direction8.TopLeft);
					character.IgnoreAimingDirection(Direction8.TopRight);
					character.IgnoreAimingDirection(Direction8.BottomLeft);
					character.IgnoreAimingDirection(Direction8.BottomRight);
					break;
				case ProjectileValidDirection.Four:
					character.IgnoreAimingDirection(Direction8.TopLeft);
					character.IgnoreAimingDirection(Direction8.TopRight);
					character.IgnoreAimingDirection(Direction8.BottomLeft);
					character.IgnoreAimingDirection(Direction8.BottomRight);
					break;
				case ProjectileValidDirection.Five:
					character.IgnoreAimingDirection(Direction8.BottomLeft);
					character.IgnoreAimingDirection(Direction8.BottomRight);
					character.IgnoreAimingDirection(Direction8.Bottom);
					break;
			}
		}
	}

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
			var aim = sender.AimingDirection;

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
