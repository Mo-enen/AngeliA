using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class CharacterAttackness (Character character) {




	#region --- SUB ---


	public enum AttackStyleMode { Combo, Random, Manually, }


	#endregion




	#region --- VAR ---


	// Api
	public readonly Character TargetCharacter = character;
	public virtual bool IsChargingAttack => false;
	public virtual AttackStyleMode AttackStyle => AttackStyleMode.Random;
	public virtual Direction8 AimingDirection => Direction8.Right;
	public bool IsAttacking => Game.GlobalFrame < LastAttackFrame + AttackDuration;
	public int LastAttackFrame { get; private set; } = int.MinValue;
	public int? AttackChargeStartFrame { get; private set; } = null;
	public bool LastAttackCharged { get; private set; } = false;
	public int AttackStyleIndex { get; set; } = -1;
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


	public void OnActivated () {
		LastAttackFrame = int.MinValue;
		AttackChargeStartFrame = null;
		LastAttackCharged = false;
	}


	public void PhysicsUpdate_Attack () {

		// Combo Break
		if (AttackStyle == AttackStyleMode.Combo && AttackStyleIndex > -1 && Game.GlobalFrame > LastAttackFrame + AttackDuration + AttackCooldown + AttackComboGap) {
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
		switch (AttackStyle) {
			case AttackStyleMode.Combo:
				AttackStyleIndex++;
				break;
			case AttackStyleMode.Random:
				AttackStyleIndex += Util.QuickRandom(1, Util.Max(2, AttackStyleLoop));
				break;
		}
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