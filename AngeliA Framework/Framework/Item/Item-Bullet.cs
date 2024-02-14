using System.Collections;
using System.Collections.Generic;

namespace AngeliA.Framework {
	[RequireSprite("{0}.Bullet")]
	public abstract class BulletItem : Item {
		public int BulletArtworkID { get; init; }
		public override int MaxStackCount => 512;
		public BulletItem () => BulletArtworkID = $"{GetType().AngeName()}.Bullet".AngeHash();
	}
}
