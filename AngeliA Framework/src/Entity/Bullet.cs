using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.Capacity(128, 0)]
[EntityAttribute.ExcludeInMapEditor]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.DontDespawnOutOfRange]
[EntityAttribute.Layer(EntityLayer.BULLET)]
public abstract class Bullet : Entity {




	#region --- VAR ---


	// Api
	public static event System.Action<Bullet, IDamageReceiver, Tag> OnBulletDealDamage;
	public static event System.Action<Bullet, Tag> OnBulletHitEnvironment;
	public int AttackIndex => Sender is IWithCharacterAttackness attSender ? attSender.CurrentAttackness.AttackStyleIndex : 0;
	public bool AttackCharged => Sender is IWithCharacterAttackness attSender && attSender.CurrentAttackness.LastAttackCharged;

	public readonly FrameBasedInt Damage = new(1);
	public Tag DamageType { get; set; } = Tag.PhysicalDamage;
	public Entity Sender { get; set; } = null;
	protected virtual int EnvironmentMask => PhysicsMask.MAP;
	protected virtual int ReceiverMask => PhysicsMask.ENTITY;
	protected virtual int EnvironmentHitCount => int.MaxValue;
	protected virtual int ReceiverHitCount => int.MaxValue;
	protected virtual bool RoundHitCheck => false;
	public virtual int Duration => 60;

	// Data
	private int CurrentEnvironmentHitCount;
	private int CurrentReceiverHitCount;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		Width = Const.CEL;
		Height = Const.CEL;
		Sender = null;
		CurrentEnvironmentHitCount = EnvironmentHitCount;
		CurrentReceiverHitCount = ReceiverHitCount;
		Damage.ClearOverride();
		DamageType = Tag.PhysicalDamage;
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Life Check
		if (!Active || Game.GlobalFrame > SpawnFrame + Duration) {
			Active = false;
			return;
		}
		// Environment Hit Check
		EnvironmentHitCheck(out _);
	}


	public override void Update () {
		base.Update();
		if (!Active) return;
		ReceiverHitCheck(out _);
	}


	#endregion




	#region --- API ---


	protected virtual void ReceiverHitCheck (out bool requireSelfDestroy) {
		var rect = Rect;
		requireSelfDestroy = false;
		var hits = Physics.OverlapAll(
			ReceiverMask, rect, out int count, Sender, OperationMode.ColliderAndTrigger
		);
		int targetTeam = Sender is Character chSender ? chSender.AttackTargetTeam : Const.TEAM_ALL;
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			// Gate
			if (hit.Entity is not IDamageReceiver receiver) continue;
			if ((receiver.Team & targetTeam) != receiver.Team) continue;
			var fixedDamageType = DamageType & ~receiver.IgnoreDamageType;
			if (fixedDamageType == Tag.None) continue;
			if (receiver is Entity e && !e.Active) continue;
			// Round Shape Gate
			if (RoundHitCheck) {
				int dis = Util.DistanceInt(rect.CenterX(), rect.CenterY(), hit.Rect.CenterX(), hit.Rect.CenterY());
				int rad = (Width.Abs() + Height.Abs()) / 4;
				int hitRad = (hit.Rect.width.Abs() + hit.Rect.height.Abs()) / 4;
				if (dis > rad + hitRad) continue;
			}
			// Perform Damage
			PerformDamage(receiver, fixedDamageType);
			// Destroy Check
			PerformHitReceiver(receiver, out bool _requireSelfDestroy);
			requireSelfDestroy = _requireSelfDestroy || requireSelfDestroy;
		}
	}


	protected virtual void EnvironmentHitCheck (out bool requireSelfDestroy) {
		if (Physics.Overlap(EnvironmentMask, Rect, Sender)) {
			PerformHitEnvironment(out requireSelfDestroy);
		} else {
			requireSelfDestroy = false;
		}
	}


	protected virtual void BeforeDespawn (IDamageReceiver receiver) { }


	protected virtual void PerformDamage (IDamageReceiver receiver, Tag damageType) {
		receiver.TakeDamage(new Damage(Damage, Sender, this, damageType));
		OnBulletDealDamage?.Invoke(this, receiver, damageType);
	}


	protected void PerformHitEnvironment (out bool requireSelfDestroy) {
		CurrentEnvironmentHitCount--;
		if (CurrentEnvironmentHitCount <= 0) {
			Active = false;
			BeforeDespawn(null);
			OnBulletHitEnvironment?.Invoke(this, DamageType);
			requireSelfDestroy = true;
		} else {
			requireSelfDestroy = false;
		}
	}


	protected void PerformHitReceiver (IDamageReceiver receiver, out bool requireSelfDestroy) {
		CurrentReceiverHitCount--;
		if (CurrentReceiverHitCount <= 0) {
			Active = false;
			BeforeDespawn(receiver);
			requireSelfDestroy = true;
		} else {
			requireSelfDestroy = false;
		}
	}


	public bool GroundCheck (out Color32 groundTint) {
		groundTint = Color32.WHITE;
		bool grounded =
			Physics.Overlap(PhysicsMask.MAP, Rect.EdgeOutside(Direction4.Down, 4), out var hit, Sender) ||
			Physics.Overlap(PhysicsMask.MAP, Rect.EdgeOutside(Direction4.Down, 4), out hit, Sender, OperationMode.TriggerOnly, Tag.OnewayUp);
		if (grounded && Renderer.TryGetSprite(hit.SourceID, out var groundSprite, false)) {
			groundTint = groundSprite.SummaryTint;
		}
		return grounded;
	}


	#endregion




}