using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Breakable : Rigidbody, IBlockEntity, IDamageReceiver {

	int IDamageReceiver.Team => Const.TEAM_ENVIRONMENT;
	bool IDamageReceiver.TakeDamageFromLevel => false;
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	public override bool DestroyWhenInsideGround => true;
	public virtual bool SpawnItemFromMapWhenBreak => false;
	public virtual Tag IgnoreDamageType => Tag.None; // ※ Interface

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

	protected virtual void OnBreak () {
		// Drop Item from Code
		if (!IgnoreReposition) {
			ItemSystem.DropItemFor(this);
		}
		// Drop Item from Map
		if (SpawnItemFromMapWhenBreak && MapUnitPos.HasValue) {
			int itemID = WorldSquad.Stream.GetBlockAt(
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