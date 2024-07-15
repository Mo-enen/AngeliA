using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
public abstract partial class Character {




	#region --- VAR ---


	// Api
	public bool IsAttacking => Game.GlobalFrame < LastAttackFrame + AttackDuration;
	public int LastAttackFrame { get; private set; } = int.MinValue;
	public int? AttackChargeStartFrame { get; private set; } = null;
	public bool LastAttackCharged { get; private set; } = false;
	public virtual int AttackStyleIndex { get; private set; } = -1;
	public virtual int AttackTargetTeam => Const.TEAM_ALL;
	public virtual bool IsChargingAttack => false;
	public virtual bool RandomAttackAnimationStyle => true;
	public virtual Direction8 AimingDirection => Direction8.Right;
	public int AttackStyleLoop { get; set; } = 1;
	public bool AttackStartFacingRight { get; set; } = true;
	public int AttackDuration { get; private set; } = 12;
	public int AttackCooldown { get; private set; } = 2;
	public int MinimalChargeAttackDuration { get; private set; } = int.MaxValue;
	public bool RepeatAttackWhenHolding { get; private set; } = false;
	public bool LockFacingOnAttack { get; private set; } = false;


	#endregion




	#region --- MSG ---


	private void OnActivated_Attack () {
		LastAttackFrame = int.MinValue;
		AttackChargeStartFrame = null;
		LastAttackCharged = false;
	}


	private void PhysicsUpdate_Attack () {

		// Combo Break
		if (!RandomAttackAnimationStyle && AttackStyleIndex > -1 && Game.GlobalFrame > LastAttackFrame + AttackDuration + AttackCooldown + AttackComboGap) {
			AttackStyleIndex = -1;
		}

		// Charge Start
		if (IsChargingAttack && !AttackChargeStartFrame.HasValue) {
			AttackChargeStartFrame = Game.GlobalFrame;
		}

		// Charge Release
		if (!IsChargingAttack && AttackChargeStartFrame.HasValue) {
			if (Game.GlobalFrame - AttackChargeStartFrame.Value >= MinimalChargeAttackDuration) {
				Attack(charged: true);
			}
			AttackChargeStartFrame = null;
		}

	}


	#endregion




	#region --- API ---


	public virtual bool Attack (bool charged = false) {
		if (!IsAttackAllowedByMovement() || !IsAttackAllowedByEquipment()) return false;
		LastAttackCharged = charged;
		LastAttackFrame = Game.GlobalFrame;
		AttackStyleIndex += RandomAttackAnimationStyle ? Util.QuickRandom(1, Util.Max(2, AttackStyleLoop)) : 1;
		AttackStartFacingRight = _FacingRight;
		return true;
	}


	protected virtual bool IsAttackAllowedByMovement () =>
		!IsCrashing &&
		(AttackInAir || IsGrounded || InWater || InSand || IsClimbing) &&
		(AttackInWater || !InWater) &&
		(AttackWhenWalking || !IsGrounded || !IsWalking) &&
		(AttackWhenRunning || !IsGrounded || !IsRunning) &&
		(AttackWhenClimbing || !IsClimbing) &&
		(AttackWhenFlying || !IsFlying) &&
		(AttackWhenRolling || !IsRolling) &&
		(AttackWhenSquatting || !IsSquatting) &&
		(AttackWhenDashing || !IsDashing) &&
		(AttackWhenSliding || !IsSliding) &&
		(AttackWhenGrabbing || (!IsGrabbingTop && !IsGrabbingSide)) &&
		(AttackWhenPounding || !IsPounding) &&
		(AttackWhenRush || !IsRushing);


	protected virtual bool IsAttackAllowedByEquipment () =>
		this is not PoseCharacter poseCharacter ||
		(GetEquippingItem(EquipmentType.Weapon) is Weapon weapon && weapon.AllowingAttack(poseCharacter));


	#endregion




}