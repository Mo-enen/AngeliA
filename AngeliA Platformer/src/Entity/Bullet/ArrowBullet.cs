using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public class ArrowBullet<I> : ArrowBullet {
	public ArrowBullet () {
		string name = typeof(I).AngeName();
		ArrowItemID = name.AngeHash();
		ArrowArtworkID = $"{name}.Bullet".AngeHash();
		if (!Renderer.HasSprite(ArrowArtworkID) && !Renderer.HasSpriteGroup(ArrowArtworkID)) {
			ArrowArtworkID = ArrowItemID;
		}
	}
}

public class ArrowBullet : MovableBullet {

	public int ArrowItemID { get; init; } = 0;
	public int ArrowArtworkID { get; init; } = 0;
	public override int ArtworkID => ArrowArtworkID;
	protected virtual bool SpawnItemWhenBulletDestroy => true;
	private bool ItemSpawned = false;

	public ArrowBullet () => ArrowArtworkID = TypeID;

	public override void OnActivated () {
		base.OnActivated();
		ItemSpawned = false;
	}

	protected override void BeforeDespawn (IDamageReceiver receiver) {
		if (!ItemSpawned && SpawnItemWhenBulletDestroy && ArrowItemID != 0) {
			ItemSystem.SpawnItem(ArrowItemID, X, Y, 1);
			ItemSpawned = true;
		}
	}

}