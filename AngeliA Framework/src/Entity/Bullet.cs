using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// An entity represent bullet from weapons that deal damage to IDamageReceiver
/// </summary>
[EntityAttribute.Capacity(128, 0)]
[EntityAttribute.ExcludeInMapEditor]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.DontDespawnOutOfRange]
[EntityAttribute.Layer(EntityLayer.BULLET)]
public abstract class Bullet : Entity {




	#region --- VAR ---


	// Api
	[OnBulletHitEnvironment_Bullet] internal static System.Action<Bullet> OnBulletHitEnvironment;
	/// <summary>
	/// Index for style of the attack from the sender
	/// </summary>
	public int AttackIndex => Sender is IWithCharacterAttackness attSender ? attSender.CurrentAttackness.AttackStyleIndex : 0;
	/// <summary>
	/// True if the attack is charged
	/// </summary>
	public bool AttackCharged => Sender is IWithCharacterAttackness attSender && attSender.CurrentAttackness.LastAttackCharged;
	/// <summary>
	/// What extra type of damage does this bullet deal
	/// </summary>
	public Tag DamageType { get; set; } = Tag.PhysicalDamage;
	/// <summary>
	/// This entity send the bullet
	/// </summary>
	public Entity Sender { get; set; } = null;
	/// <summary>
	/// Default team for checking attack target
	/// </summary>
	public int FailbackTargetTeam { get; set; } = Const.TEAM_ALL;
	/// <summary>
	/// Team data for checking which group of target should be attack
	/// </summary>
	public int TargetTeam => Sender is Character chSender ? chSender.AttackTargetTeam : FailbackTargetTeam;
	/// <summary>
	/// Intrinsic damage value
	/// </summary>
	protected virtual int BasicDamage => 1;
	/// <summary>
	/// Group of physics layers for checking environment that this bullet can react with
	/// </summary>
	protected virtual int EnvironmentMask => PhysicsMask.MAP;
	/// <summary>
	/// Group of physics layers for checking target that this bullet can react with
	/// </summary>
	protected virtual int ReceiverMask => PhysicsMask.ENTITY;
	/// <summary>
	/// How many environment collider can this bullet hit without getting despawn
	/// </summary>
	protected virtual int EnvironmentHitCount => int.MaxValue;
	/// <summary>
	/// How many target collider can this bullet hit without getting despawn
	/// </summary>
	protected virtual int ReceiverHitCount => int.MaxValue;
	/// <summary>
	/// True if the bullet check for target in a round shaped range
	/// </summary>
	protected virtual bool RoundHitCheck => false;
	/// <summary>
	/// How long can this bullet exists on stage in frame
	/// </summary>
	public virtual int Duration => 60;
	/// <summary>
	/// Final damage value
	/// </summary>
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


	/// <summary>
	/// Check for hitting any IDamageReceiver
	/// </summary>
	/// <param name="requireSelfDestroy">True if this bullet should be despawn</param>
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

	/// <summary>
	/// Check for hitting any environment block
	/// </summary>
	/// <param name="requireSelfDestroy">True if this bullet should be despawn</param>
	protected void EnvironmentHitCheck (out bool requireSelfDestroy) {
		if (Physics.Overlap(EnvironmentMask, Rect, Sender)) {
			PerformHitEnvironment(out requireSelfDestroy);
		} else {
			requireSelfDestroy = false;
		}
	}


	/// <summary>
	/// This function is called before the bullet get despawn by performing damage logic
	/// </summary>
	/// <param name="receiver">The target it hits</param>
	protected virtual void BeforeDespawn (IDamageReceiver receiver) { }


	/// <summary>
	/// This function is called when the bullet hit environment colliders
	/// </summary>
	/// <param name="requireSelfDestroy">True if this bullet should be despawn</param>
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


	/// <summary>
	/// This function is called when the bullet hit IDamageReceiver
	/// </summary>
	/// <param name="receiver">The target it hits</param>
	/// <param name="requireSelfDestroy">True if this bullet should be despawn</param>
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


	/// <summary>
	/// True if the bullet is touching ground
	/// </summary>
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


	/// <summary>
	/// Get the damage data using by this bullet to deal damage
	/// </summary>
	/// <returns></returns>
	public Damage GetDamage () => new(Damage, TargetTeam, this, DamageType);


	#endregion




}