using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Breakable : Rigidbody, IBlockEntity, IDamageReceiver {

	int IDamageReceiver.Team => Const.TEAM_ENVIRONMENT;
	bool IDamageReceiver.TakeDamageFromLevel => false;
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	public override bool DestroyWhenInsideGround => true;
	protected virtual Tag IgnoreDamageType => Tag.None;


	// MSG
	void IDamageReceiver.TakeDamage (Damage damage) {
		if (!Active || damage.Amount <= 0) return;
		if (IgnoreDamageType.HasAll(damage.Type)) return;
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
		if (!IgnoreReposition) {
			ItemSystem.DropItemFor(this);
		}
		FrameworkUtil.BreakEntityBlock(this);
	}

}