using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class CharacterAttackness {




	#region --- VAR ---


	// Api
	public readonly Character TargetCharacter;
	public bool IsAttacking => Game.GlobalFrame < LastAttackFrame + AttackDuration;
	public int LastAttackFrame { get; private set; } = int.MinValue;
	public int? AttackChargeStartFrame { get; private set; } = null;
	public bool LastAttackCharged { get; private set; } = false;
	public virtual int AttackStyleIndex { get; private set; } = -1;
	public virtual bool IsChargingAttack => false;
	public virtual bool RandomAttackAnimationStyle => true;
	public virtual Direction8 AimingDirection => Direction8.Right;
	public int AttackStyleLoop { get; set; } = 1;
	public bool AttackStartFacingRight { get; set; } = true;
	public int AttackDuration { get; set; } = 12;
	public int AttackCooldown { get; set; } = 2;
	public int MinimalChargeAttackDuration { get; set; } = int.MaxValue;
	public bool RepeatAttackWhenHolding { get; set; } = false;
	public bool LockFacingOnAttack { get; set; } = false;

	// Data
	protected readonly int[] IgnoreAimingDirectionFrames = new int[8].FillWithValue(-1);


	#endregion




	#region --- MSG ---


	public CharacterAttackness (Character character) => TargetCharacter = character;


	public void OnActivated () {
		LastAttackFrame = int.MinValue;
		AttackChargeStartFrame = null;
		LastAttackCharged = false;
	}


	public void PhysicsUpdate_Attack () {

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
				Attack(AttackStartFacingRight, charged: true);
			}
			AttackChargeStartFrame = null;
		}

	}


	#endregion




	#region --- API ---


	public virtual bool Attack (bool facingRight, bool charged = false) {
		if (!TargetCharacter.IsAttackAllowedByMovement()) return false;
		if (!TargetCharacter.IsAttackAllowedByEquipment()) return false;
		LastAttackCharged = charged;
		LastAttackFrame = Game.GlobalFrame;
		AttackStyleIndex += RandomAttackAnimationStyle ? Util.QuickRandom(1, Util.Max(2, AttackStyleLoop)) : 1;
		AttackStartFacingRight = facingRight;
		return true;
	}


	public void CancelAttack () => LastAttackFrame = int.MinValue;


	// Ignore Aim
	public void IgnoreAimingDirection (Direction8 dir, int duration = 1) {
		IgnoreAimingDirectionFrames[(int)dir] = Game.GlobalFrame + duration;
	}


	public bool IsAimingDirectionIgnored (Direction8 dir) => Game.GlobalFrame <= IgnoreAimingDirectionFrames[(int)dir];


	#endregion




}