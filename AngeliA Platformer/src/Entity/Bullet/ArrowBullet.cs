using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


/// <summary>
/// Bullet entity that spawns an item when destroy
/// </summary>
/// <typeparam name="I">Type of the item as arrow</typeparam>
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


/// <summary>
/// Bullet entity that spawns an item when destroy
/// </summary>
public class ArrowBullet : MovableBullet {

	/// <summary>
	/// Target item to spawn when destroy
	/// </summary>
	public int ArrowItemID { get; init; } = 0;
	
	/// <summary>
	/// Artwork sprite ID to render this bullet
	/// </summary>
	public int ArrowArtworkID { get; init; } = 0;
	public override int ArtworkID => ArrowArtworkID;
	
	/// <summary>
	/// True if the arrow item should be spawn when bullet destroy
	/// </summary>
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