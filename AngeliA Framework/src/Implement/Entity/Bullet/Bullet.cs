using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

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
	protected virtual Tag DamageType => Tag.GeneralDamage;
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

	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Life Check
		if (Game.GlobalFrame > SpawnFrame + Duration) {
			Active = false;
			return;
		}
		// Environment Hit Check
		if (Physics.Overlap(EnvironmentMask, Rect, Sender)) {
			if (DestroyOnHitEnvironment) {
				Active = false;
				BeforeDespawn(null);
			}
		}
	}

	public override void Update () {
		base.Update();
		ReceiverHitCheck(Rect);
	}

	// Api
	protected void ReceiverHitCheck (IRect rect) {
		var hits = Physics.OverlapAll(
			ReceiverMask, rect, out int count, Sender, OperationMode.ColliderAndTrigger
		);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not IDamageReceiver receiver) continue;
			if ((receiver.Team & TargetTeam) != receiver.Team) continue;
			if (receiver is Entity e && !e.Active) continue;
			receiver.TakeDamage(new Damage(Damage, Sender, Rect, DamageType));
			if (DestroyOnHitReceiver) {
				Active = false;
				BeforeDespawn(receiver);
			}
		}
	}

	public bool GroundCheck (out Color32 groundTint) {
		groundTint = Color32.WHITE;
		bool grounded =
			Physics.Overlap(PhysicsMask.MAP, Rect.EdgeOutside(Direction4.Down, 4), out var hit, Sender) ||
			Physics.Overlap(PhysicsMask.MAP, Rect.EdgeOutside(Direction4.Down, 4), out hit, Sender, OperationMode.TriggerOnly, Tag.OnewayUp);
		if (grounded && Renderer.TryGetSprite(hit.SourceID, out var groundSprite)) {
			groundTint = groundSprite.SummaryTint;
		}
		return grounded;
	}

	protected virtual void BeforeDespawn (IDamageReceiver receiver) { }

}