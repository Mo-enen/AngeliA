using System;
using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public static class PlatformerUtil {


	[CheatCode("GiveAmmo")]
	internal static bool Cheat_GiveAmmo () {
		var player = PlayerSystem.Selecting;
		if (player == null) return false;
		int id = Inventory.GetEquipment(player.InventoryID, EquipmentType.HandTool, out int eqCount);
		if (id == 0 || eqCount <= 0 || ItemSystem.GetItem(id) is not Weapon weapon) return false;
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


	public static Int2 NavigationFreeWandering (Int2 aimPosition, Entity target, out bool grounded, int frequency, int maxDistance, int randomShift = 0) {

		int insIndex = target.InstanceOrder;
		int freeShiftX = Util.QuickRandomWithSeed(
			target.TypeID + (insIndex + (Game.GlobalFrame / frequency) + randomShift) * target.TypeID
		) % maxDistance;

		// Find Available Ground
		int offsetX = freeShiftX + Const.CEL * ((insIndex % 12) / 2 + 2) * (insIndex % 2 == 0 ? -1 : 1);
		if (Navigation.ExpandTo(
			Game.GlobalFrame, Stage.ViewRect,
			aimPosition.x, aimPosition.y,
			aimPosition.x + offsetX, aimPosition.y + Const.HALF,
			maxIteration: 12,
			out int groundX, out int groundY
		)) {
			aimPosition.x = groundX;
			aimPosition.y = groundY;
			grounded = true;
		} else {
			aimPosition.x += offsetX;
			grounded = false;
		}

		return aimPosition;
	}


}
