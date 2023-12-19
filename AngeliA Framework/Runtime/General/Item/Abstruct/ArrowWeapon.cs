using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public abstract class ArrowWeapon<A> : ArrowWeapon where A : Item {
		public ArrowWeapon () {
			ArrowItemID = typeof(A).AngeHash();
		}
	}
	public abstract class ArrowWeapon : ProjectileWeapon {

		public virtual int ArrowCountInOneShot => 1;
		protected int ArrowItemID { get; init; }

		public override bool AllowingAttack (PoseCharacter character) => FindAmmo(character, out _, out _) && base.AllowingAttack(character);

		protected bool FindAmmo (Character character, out int index, out int count) {
			index = -1;
			count = 0;
			if (ArrowItemID == 0) return false;
			int capacity = character.GetInventoryCapacity();
			for (int i = 0; i < capacity; i++) {
				int id = character.GetItemIDFromInventory(i, out count);
				index = i;
				if (id == ArrowItemID) return true;
			}
			index = -1;
			count = 0;
			return false;
		}

		public override Bullet SpawnBullet (Character sender) {
			if (ArrowItemID == 0) return null;
			int takenCount = Inventory.FindAndTakeItem(sender.TypeID, ArrowItemID, ArrowCountInOneShot);
			if (takenCount == 0) return null;
			Bullet result = null;
			for (int i = 0; i < takenCount; i++) {
				result = base.SpawnBullet(sender);
				if (result is ArrowBullet aBullet) {
					aBullet.ArrowItemID = ArrowItemID;
					aBullet.ArrowArtworkID = ItemSystem.GetItem(ArrowItemID) is ItemArrow aItem ? aItem.BulletArtworkID : 0;
				}
			}
			return result;
		}

	}
}