using System;
using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

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

	[CheatCode("GiveAmmo")]
	internal static bool Cheat_GiveAmmo () {
		var player = PlayerSystem.Selecting;
		if (player == null) return false;
		int id = Inventory.GetEquipment(player.InventoryID, EquipmentType.HandTool, out int eqCount);
		if (id == 0 || eqCount <= 0 || ItemSystem.GetItem(id) is not HandTool weapon) return false;
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
				Inventory.GiveItemToTarget(player, itemID, maxCount);
				performed = true;
			}
		}
		// Fill Weapon
		int eqMaxCount = ItemSystem.GetItemMaxStackCount(weapon.TypeID);
		if (eqMaxCount > eqCount) {
			Inventory.SetEquipment(player.InventoryID, EquipmentType.HandTool, weapon.TypeID, eqMaxCount);
			performed = true;
		}
		return performed;
	}

	public override void OnPoseAnimationUpdate_FromEquipment (PoseCharacterRenderer rendering) {
		base.OnPoseAnimationUpdate_FromEquipment(rendering);
		var character = rendering.TargetCharacter;
		var attackness = character.Attackness;
		switch (ValidDirection) {
			case ProjectileValidDirection.Two:
				attackness.IgnoreAimingDirection(Direction8.Bottom);
				attackness.IgnoreAimingDirection(Direction8.Top);
				attackness.IgnoreAimingDirection(Direction8.TopLeft);
				attackness.IgnoreAimingDirection(Direction8.TopRight);
				attackness.IgnoreAimingDirection(Direction8.BottomLeft);
				attackness.IgnoreAimingDirection(Direction8.BottomRight);
				break;
			case ProjectileValidDirection.Three:
				attackness.IgnoreAimingDirection(Direction8.Bottom);
				attackness.IgnoreAimingDirection(Direction8.TopLeft);
				attackness.IgnoreAimingDirection(Direction8.TopRight);
				attackness.IgnoreAimingDirection(Direction8.BottomLeft);
				attackness.IgnoreAimingDirection(Direction8.BottomRight);
				break;
			case ProjectileValidDirection.Four:
				attackness.IgnoreAimingDirection(Direction8.TopLeft);
				attackness.IgnoreAimingDirection(Direction8.TopRight);
				attackness.IgnoreAimingDirection(Direction8.BottomLeft);
				attackness.IgnoreAimingDirection(Direction8.BottomRight);
				break;
			case ProjectileValidDirection.Five:
				attackness.IgnoreAimingDirection(Direction8.BottomLeft);
				attackness.IgnoreAimingDirection(Direction8.BottomRight);
				attackness.IgnoreAimingDirection(Direction8.Bottom);
				break;
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
