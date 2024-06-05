using System.Collections;
using System.Collections.Generic;

namespace AngeliA; 
public abstract class BulletItem : Item {
	public int BulletArtworkID { get; init; }
	public override int MaxStackCount => 512;
	public BulletItem () => BulletArtworkID = $"{GetType().AngeName()}.Bullet".AngeHash();
}
