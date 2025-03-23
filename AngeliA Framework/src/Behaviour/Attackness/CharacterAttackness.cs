using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Behavour class that handle attack logic for character
/// </summary>
public class CharacterAttackness (Character character) {




	#region --- VAR ---


	// Api
	/// <summary>
	/// Character hosting this attackness
	/// </summary>
	public readonly Character TargetCharacter = character;
	/// <summary>
	/// Which direction does the character attacks
	/// </summary>
	public Direction8 AimingDirection { get; set; } = Direction8.Right;
	/// <summary>
	/// True if the character is attacking at the current frame
	/// </summary>
	public bool IsAttacking => Game.GlobalFrame < LastAttackFrame + AttackDuration;
	/// <summary>
	/// True if the character is not allow to attack at the current frame
	/// </summary>
	public bool IsAttackIgnored => Game.GlobalFrame <= IgnoreAttackFrame;
	/// <summary>
	/// True if the character is charging attack at the current frame
	/// </summary>
	public bool IsChargingAttack { get; set; } = false;
	/// <summary>
	/// The start frame of the last attack
	/// </summary>
	public int LastAttackFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// The start frame of the last attack charging
	/// </summary>
	public int? AttackChargeStartFrame { get; private set; } = null;
	/// <summary>
	/// True if the last performed attack is charged
	/// </summary>
	public bool LastAttackCharged { get; private set; } = false;
	/// <summary>
	/// Attack style index of the current attack, indicate what kind of rendering style should be apply to the current attack
	/// </summary>
	public int AttackStyleIndex { get; set; } = -1;
	/// <summary>
	/// True if character facing right when the current attack start
	/// </summary>
	public bool AttackStartFacingRight { get; set; } = true;
	/// <summary>
	/// How many frames should be the current attack last
	/// </summary>
	public int AttackDuration { get; set; } = 12;
	/// <summary>
	/// How many frames should be wait from the prev attack end to the next attack start
	/// </summary>
	public int AttackCooldown { get; set; } = 2;
	/// <summary>
	/// Charge attack longer than this frame should be count as attack charged
	/// </summary>
	public int MinimalChargeAttackDuration { get; set; } = int.MaxValue;
	/// <summary>
	/// True if the character can hold attack button to keep attacking multiple times
	/// </summary>
	public bool RepeatAttackWhenHolding { get; set; } = false;
	/// <summary>
	/// True if the character is holding attack button
	/// </summary>
	public bool HoldingAttack { get; internal set; } = false;
	/// <summary>
	/// True if the character can not change it's facing direction when attacking
	/// </summary>
	public bool LockFacingOnAttack { get; set; } = false;

	// Data
	private readonly int[] IgnoreAimingDirectionFrames = new int[8].FillWithValue(-1);
	private int IgnoreAttackFrame = -1;

	// Meta
	/// <summary>
	/// Attack happens between this many frames should be combo attacks
	/// </summary>
	public readonly FrameBasedInt AttackComboGap = new(12);
	/// <summary>
	/// If hold attack button to perform multiple attacks, the cooldown will be add this frames longer
	/// </summary>
	public readonly FrameBasedInt HoldAttackPunishFrame = new(4);
	/// <summary>
	/// When character jumps, unfinished attack will be cancel
	/// </summary>
	public readonly FrameBasedBool CancelAttackOnJump = new(false);

	/// <summary>
	/// Moving speed will be mutiply be this rate when character is attacking (0 means 0%, 1000 means 100%)
	/// </summary>
	public readonly FrameBasedInt DefaultSpeedRateOnAttack = new(0);
	/// <summary>
	/// Moving speed will be mutiply be this rate when character is attacking while not grounded (0 means 0%, 1000 means 100%)
	/// </summary>
	public readonly FrameBasedInt AirSpeedRateOnAttack = new(1000);
	/// <summary>
	/// Moving speed will be mutiply be this rate when character is attacking while walking (0 means 0%, 1000 means 100%)
	/// </summary>
	public readonly FrameBasedInt WalkingSpeedRateOnAttack = new(0);
	/// <summary>
	/// Moving speed will be mutiply be this rate when character is attacking while running (0 means 0%, 1000 means 100%)
	/// </summary>
	public readonly FrameBasedInt RunningSpeedRateOnAttack = new(0);

	/// <summary>
	/// Allow character attack when not grounded
	/// </summary>
	public readonly FrameBasedBool AttackInAir = new(true);
	/// <summary>
	/// Allow character attack when inside water
	/// </summary>
	public readonly FrameBasedBool AttackInWater = new(true);
	/// <summary>
	/// Allow character attack when walking
	/// </summary>
	public readonly FrameBasedBool AttackWhenWalking = new(true);
	/// <summary>
	/// Allow character attack when running
	/// </summary>
	public readonly FrameBasedBool AttackWhenRunning = new(true);
	/// <summary>
	/// Allow character attack when climbing
	/// </summary>
	public readonly FrameBasedBool AttackWhenClimbing = new(false);
	/// <summary>
	/// Allow character attack when flying
	/// </summary>
	public readonly FrameBasedBool AttackWhenFlying = new(false);
	/// <summary>
	/// Allow character attack when rolling
	/// </summary>
	public readonly FrameBasedBool AttackWhenRolling = new(false);
	/// <summary>
	/// Allow character attack when squatting
	/// </summary>
	public readonly FrameBasedBool AttackWhenSquatting = new(false);
	/// <summary>
	/// Allow character attack when dashing
	/// </summary>
	public readonly FrameBasedBool AttackWhenDashing = new(false);
	/// <summary>
	/// Allow character attack when sliding
	/// </summary>
	public readonly FrameBasedBool AttackWhenSliding = new(false);
	/// <summary>
	/// Allow character attack when grabbing
	/// </summary>
	public readonly FrameBasedBool AttackWhenGrabbing = new(false);
	/// <summary>
	/// Allow character attack when rushing
	/// </summary>
	public readonly FrameBasedBool AttackWhenRush = new(false);
	/// <summary>
	/// Allow character attack when pounding
	/// </summary>
	public readonly FrameBasedBool AttackWhenPounding = new(false);


	#endregion




	#region --- MSG ---


	/// <summary>
	/// Callback for character entity get activated
	/// </summary>
	public virtual void OnActivated () {
		LastAttackFrame = int.MinValue;
		AttackChargeStartFrame = null;
		LastAttackCharged = false;
		IsChargingAttack = false;
	}


	/// <summary>
	/// Callback for update the attackness
	/// </summary>
	public virtual void PhysicsUpdate_Attack () {

		// Combo Break
		if (AttackStyleIndex > -1 && Game.GlobalFrame > LastAttackFrame + AttackDuration + AttackCooldown + AttackComboGap) {
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


	/// <summary>
	/// Perform an attack
	/// </summary>
	/// <param name="facingRight">True if the attack is performed when character facing right</param>
	/// <param name="charged">True if the attack is charged</param>
	/// <returns></returns>
	public virtual bool Attack (bool facingRight, bool charged = false) {
		if (Game.GlobalFrame <= IgnoreAttackFrame) return false;
		if (!TargetCharacter.IsAttackAllowedByMovement()) return false;
		if (!TargetCharacter.IsAttackAllowedByEquipment()) return false;
		LastAttackCharged = charged;
		LastAttackFrame = Game.GlobalFrame;
		AttackStyleIndex++;
		AttackStartFacingRight = facingRight;
		return true;
	}


	/// <summary>
	/// Cancel current performing attack
	/// </summary>
	public void CancelAttack () => LastAttackFrame = int.MinValue;


	// Ignore
	/// <summary>
	/// Do not allow aiming at given direction for specified frames
	/// </summary>
	public void IgnoreAimingDirection (Direction8 dir, int duration = 1) {
		IgnoreAimingDirectionFrames[(int)dir] = Game.GlobalFrame + duration;
	}


	/// <summary>
	/// True if the given aiming direction is currently ignored
	/// </summary>
	public bool IsAimingDirectionIgnored (Direction8 dir) => Game.GlobalFrame <= IgnoreAimingDirectionFrames[(int)dir];


	/// <summary>
	/// Do not allow attack for specified frames
	/// </summary>
	public void IgnoreAttack (int duration = 1) => IgnoreAttackFrame = Game.GlobalFrame + duration;


	/// <summary>
	/// Allowing attack which ignored by IgnoreAttack function
	/// </summary>
	public void CancelIgnoreAttack () => IgnoreAttackFrame = -1;


	#endregion




}