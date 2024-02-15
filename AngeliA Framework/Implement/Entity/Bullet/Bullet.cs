using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
[EntityAttribute.Capacity(128)]
[EntityAttribute.ExcludeInMapEditor]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.DontDestroyOutOfRange]
[EntityAttribute.Layer(EntityLayer.BULLET)]
public abstract class Bullet : Entity {

	// Api
	protected virtual int EnvironmentMask => PhysicsMask.MAP;
	protected virtual int ReceiverMask => PhysicsMask.ENTITY;
	protected virtual int Duration => 60;
	protected virtual int Damage => 1;
	protected virtual int DamageType => 0;
	protected virtual int SpawnWidth => Const.CEL;
	protected virtual int SpawnHeight => Const.CEL;
	protected virtual bool DestroyOnHitEnvironment => false;
	protected virtual bool DestroyOnHitReceiver => false;
	public Entity Sender { get; set; } = null;
	public int AttackIndex { get; set; } = 0;
	public bool AttackCharged { get; set; } = false;
	public int TargetTeam { get; set; } = Const.TEAM_ALL;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Width = SpawnWidth;
		Height = SpawnHeight;
		Sender = null;
	}

	public override void BeforePhysicsUpdate () {
		base.BeforePhysicsUpdate();
		// Life Check
		if (Game.GlobalFrame > SpawnFrame + Duration) {
			Active = false;
			return;
		}
		// Environment Hit Check
		if (CellPhysics.Overlap(EnvironmentMask, Rect, Sender)) {
			if (DestroyOnHitEnvironment) {
				Active = false;
				BeforeDespawn(null);
			}
		}
	}

	public override void PhysicsUpdate () {
		base.PhysicsUpdate();
		ReceiverHitCheck(Rect);
	}

	// Api
	protected void ReceiverHitCheck (IRect rect) {
		var hits = CellPhysics.OverlapAll(
			ReceiverMask, rect, out int count, Sender, OperationMode.ColliderAndTrigger
		);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not IDamageReceiver receiver) continue;
			if ((receiver.Team & TargetTeam) != receiver.Team) continue;
			if (receiver is Entity e && !e.Active) continue;
			receiver.TakeDamage(new Damage(Damage, Sender, DamageType));
			if (DestroyOnHitReceiver) {
				Active = false;
				BeforeDespawn(receiver);
			}
		}
	}

	public bool GroundCheck (out Color32 groundTint) {
		groundTint = Color32.WHITE;
		bool grounded =
			CellPhysics.Overlap(PhysicsMask.MAP, Rect.EdgeOutside(Direction4.Down, 4), out var hit, Sender) ||
			CellPhysics.Overlap(PhysicsMask.MAP, Rect.EdgeOutside(Direction4.Down, 4), out hit, Sender, OperationMode.TriggerOnly, SpriteTag.ONEWAY_UP_TAG);
		if (grounded && CellRenderer.TryGetSprite(hit.SourceID, out var groundSprite)) {
			groundTint = groundSprite.SummaryTint;
		}
		return grounded;
	}

	protected virtual void BeforeDespawn (IDamageReceiver receiver) { }

}