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
	[OnBulletHitEnvironment_Bullet] internal static System.Action<Bullet> OnBulletHitEnvironment;
	public int AttackIndex => Sender is IWithCharacterAttackness attSender ? attSender.CurrentAttackness.AttackStyleIndex : 0;
	public bool AttackCharged => Sender is IWithCharacterAttackness attSender && attSender.CurrentAttackness.LastAttackCharged;
	public Tag DamageType { get; set; } = Tag.PhysicalDamage;
	public Entity Sender { get; set; } = null;
	public int FailbackTargetTeam { get; set; } = Const.TEAM_ALL;
	public int TargetTeam => Sender is Character chSender ? chSender.AttackTargetTeam : FailbackTargetTeam;
	protected virtual int BasicDamage => 1;
	protected virtual int EnvironmentMask => PhysicsMask.MAP;
	protected virtual int ReceiverMask => PhysicsMask.ENTITY;
	protected virtual int EnvironmentHitCount => int.MaxValue;
	protected virtual int ReceiverHitCount => int.MaxValue;
	protected virtual bool RoundHitCheck => false;
	public virtual int Duration => 60;
	public readonly FrameBasedInt Damage = new(1);

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
		Damage.BaseValue = BasicDamage;
		Damage.ClearOverride();
		DamageType = Tag.PhysicalDamage;
		FailbackTargetTeam = Const.TEAM_ALL;
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


	protected void ReceiverHitCheck (out bool requireSelfDestroy) {
		var rect = Rect;
		requireSelfDestroy = false;
		var hits = Physics.OverlapAll(
			ReceiverMask, rect, out int count, Sender, OperationMode.ColliderAndTrigger
		);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not IDamageReceiver receiver) continue;

			// Round Shape Gate
			if (RoundHitCheck) {
				int dis = Util.DistanceInt(rect.CenterX(), rect.CenterY(), hit.Rect.CenterX(), hit.Rect.CenterY());
				int rad = (Width.Abs() + Height.Abs()) / 4;
				int hitRad = (hit.Rect.width.Abs() + hit.Rect.height.Abs()) / 4;
				if (dis > rad + hitRad) continue;
			}
			
			// Perform Damage
			PerformHitReceiver(receiver, out bool _requireSelfDestroy);

			// Destroy Check
			requireSelfDestroy = _requireSelfDestroy || requireSelfDestroy;
		}
	}


	protected void EnvironmentHitCheck (out bool requireSelfDestroy) {
		if (Physics.Overlap(EnvironmentMask, Rect, Sender)) {
			PerformHitEnvironment(out requireSelfDestroy);
		} else {
			requireSelfDestroy = false;
		}
	}


	protected virtual void BeforeDespawn (IDamageReceiver receiver) { }


	protected virtual void PerformHitEnvironment (out bool requireSelfDestroy) {
		CurrentEnvironmentHitCount--;
		if (CurrentEnvironmentHitCount <= 0) {
			Active = false;
			BeforeDespawn(null);
			OnBulletHitEnvironment?.Invoke(this);
			requireSelfDestroy = true;
		} else {
			requireSelfDestroy = false;
		}
	}


	protected virtual void PerformHitReceiver (IDamageReceiver receiver, out bool requireSelfDestroy) {

		requireSelfDestroy = false;
		bool damaged = receiver.TakeDamage(GetDamage());
		if (!damaged) return;

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
			Physics.Overlap(PhysicsMask.MAP, Rect.EdgeOutsideDown(4), out var hit, Sender) ||
			Physics.Overlap(PhysicsMask.MAP, Rect.EdgeOutsideDown(4), out hit, Sender, OperationMode.TriggerOnly, Tag.OnewayUp);
		if (grounded && Renderer.TryGetSprite(hit.SourceID, out var groundSprite, false)) {
			groundTint = groundSprite.SummaryTint;
		}
		return grounded;
	}


	public Damage GetDamage () => new(Damage, TargetTeam, this, DamageType);


	#endregion




}