using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public abstract class ArrowWeapon<A> : ArrowWeapon where A : Item {
		public ArrowWeapon () => ArrowItemID = typeof(A).AngeHash();
	}
	public abstract class ArrowWeapon : ProjectileWeapon {

		public virtual int ArrowCountInOneShot => 1;
		public virtual int AngleSpeed => 0;
		protected int ArrowItemID { get; init; }

		public override Bullet SpawnBullet (Character sender) {

			// Take Arrow
			int takenCount = ArrowCountInOneShot;
			if (ArrowItemID != 0) {
				// Item Arrow
				takenCount = Inventory.FindAndTakeItem(sender.TypeID, ArrowItemID, ArrowCountInOneShot);
				if (takenCount == 0) {
					// Hint
					InvokeOnItemInsufficient(sender, ArrowItemID);
					return null;
				}
			}

			// Spawn Bullet
			Bullet result = null;
			for (int i = 0; i < takenCount; i++) {

				result = base.SpawnBullet(sender);

				// Arrow Bullet
				if (ArrowItemID != 0) {
					if (result is ArrowBullet aBullet) {
						var item = ItemSystem.GetItem(ArrowItemID);
						aBullet.ArrowItemID = ArrowItemID;
						aBullet.ArrowArtworkID = item is ItemArrow aItem ? aItem.BulletArtworkID : item.TypeID;
					}
				}

				// Movable Bullet
				if (result is MovableBullet mBullet) {
					if (AngleSpeed != 0) {
						int deltaAngle = (int)Util.Atan(mBullet.SpeedX, mBullet.SpeedY);
						int offset = (i % 2 == 0 ? 1 : -1) * ((i + 1) / 2);
						mBullet.CurrentRotation += offset * deltaAngle;
						mBullet.Velocity = new Int2(mBullet.Velocity.x, mBullet.Velocity.y + offset * AngleSpeed);
						mBullet.Y += offset * mBullet.Height / ArrowCountInOneShot;
					}
				}
			}
			return result;
		}

	}
}