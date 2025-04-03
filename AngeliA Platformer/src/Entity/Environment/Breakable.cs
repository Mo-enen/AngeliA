using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Entity that can be break by taking damage
/// </summary>
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Breakable : Rigidbody, IBlockEntity, IDamageReceiver {

	int IDamageReceiver.Team => Const.TEAM_ENVIRONMENT;
	bool IDamageReceiver.TakeDamageFromLevel => false;
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	public override bool DestroyWhenInsideGround => true;
	/// <summary>
	/// True if this entity spawn item from map element overlaps on it
	/// </summary>
	public virtual bool SpawnItemFromMapWhenBreak => false;
	/// <summary>
	/// This entity do not take damage with this tags
	/// </summary>
	public virtual Tag IgnoreDamageType => Tag.None; // ※ From Interface

	// MSG 
	void IDamageReceiver.OnDamaged (Damage damage) {
		Active = false;
		OnBreak();
		IgnoreReposition = true;
	}

	public override void LateUpdate () {
		if (!Active) return;
		base.LateUpdate();
		Draw();
	}

	/// <summary>
	/// This function is called when entity breaks
	/// </summary>
	protected virtual void OnBreak () {
		// Drop Item from Code
		if (!IgnoreReposition) {
			ItemSystem.DropItemFor(this);
		}
		// Drop Item from Map
		if (SpawnItemFromMapWhenBreak && MapUnitPos.HasValue) {
			int itemID = WorldSquad.Front.GetBlockAt(
				MapUnitPos.Value.x, MapUnitPos.Value.y, MapUnitPos.Value.z, BlockType.Element
			);
			if (itemID != 0 && ItemSystem.HasItem(itemID)) {
				ItemSystem.SpawnItem(itemID, Rect.CenterX(), Rect.CenterY());
			}
		}
		// Break
		FrameworkUtil.BreakEntityBlock(this);
	}

	protected override void OnInsideGroundDestroyed () {
		base.OnInsideGroundDestroyed();
		OnBreak();
		IgnoreReposition = true;
	}

}