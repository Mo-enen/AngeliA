namespace AngeliA;

public abstract class Breakable : EnvironmentRigidbody, IDamageReceiver {

	int IDamageReceiver.Team => Const.TEAM_ENVIRONMENT;
	bool IDamageReceiver.TakeDamageFromLevel => false;
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	public override bool DestroyWhenInsideGround => false;
	protected virtual Tag IgnoreDamageType => Tag.None;

	void IDamageReceiver.TakeDamage (Damage damage) {
		if (!Active || damage.Amount <= 0) return;
		if (IgnoreDamageType.HasAll(damage.Type)) return;
		Active = false;
		OnBreak();
	}
	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(TypeID, Rect);
	}
	protected virtual void OnBreak () {
		Stage.MarkAsGlobalAntiSpawn(this);
		GlobalEvent.InvokeObjectBreak(TypeID, Rect);
		ItemSystem.DropItemFor(this);
	}

}