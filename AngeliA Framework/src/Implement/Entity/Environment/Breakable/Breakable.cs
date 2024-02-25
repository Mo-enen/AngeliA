namespace AngeliA.Framework; 
public abstract class Breakable : EnvironmentRigidbody, IDamageReceiver {

	int IDamageReceiver.Team => Const.TEAM_ENVIRONMENT;
	bool IDamageReceiver.TakeDamageFromLevel => false;
	protected override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	protected override bool DestroyWhenInsideGround => false;
	protected override bool PhysicsEnable => false;
	protected virtual bool ReceivePhysicalDamage => true;
	protected virtual bool ReceiveExplosionDamage => true;

	void IDamageReceiver.TakeDamage (Damage damage) {
		if (!Active || damage.Amount <= 0) return;
		if (!ReceivePhysicalDamage && damage.IsPhysical) return;
		if (!ReceiveExplosionDamage && damage.IsExplosive) return;
		Active = false;
		OnBreak();
	}
	public override void FrameUpdate () {
		base.FrameUpdate();
		Renderer.Draw(TypeID, Rect);
	}
	protected virtual void OnBreak () {
		Stage.MarkAsGlobalAntiSpawn(this);
		BreakingParticle.SpawnParticles(TypeID, Rect);
	}

}