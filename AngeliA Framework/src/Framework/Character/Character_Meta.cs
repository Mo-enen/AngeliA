using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
public abstract partial class Character {




	#region --- Attack ---


	public readonly BuffInt AttackComboGap = new(12);
	public readonly BuffInt HoldAttackPunish = new(4);
	public readonly BuffBool CancelAttackOnJump = new(false);

	public BuffInt CurrentSpeedLoseOnAttack => Movement.MovementState switch {
		CharacterMovementState.Walk => WalkingSpeedLoseOnAttack,
		CharacterMovementState.Run => RunningSpeedLoseOnAttack,
		CharacterMovementState.JumpDown => AirSpeedLoseOnAttack,
		CharacterMovementState.JumpUp => AirSpeedLoseOnAttack,
		_ => DefaultSpeedLoseOnAttack,
	};
	public readonly BuffInt DefaultSpeedLoseOnAttack = new(0);
	public readonly BuffInt AirSpeedLoseOnAttack = new(1000);
	public readonly BuffInt WalkingSpeedLoseOnAttack = new(0);
	public readonly BuffInt RunningSpeedLoseOnAttack = new(0);

	public readonly BuffBool AttackInAir = new(true);
	public readonly BuffBool AttackInWater = new(true);
	public readonly BuffBool AttackWhenWalking = new(true);
	public readonly BuffBool AttackWhenRunning = new(true);
	public readonly BuffBool AttackWhenClimbing = new(false);
	public readonly BuffBool AttackWhenFlying = new(false);
	public readonly BuffBool AttackWhenRolling = new(false);
	public readonly BuffBool AttackWhenSquatting = new(false);
	public readonly BuffBool AttackWhenDashing = new(false);
	public readonly BuffBool AttackWhenSliding = new(false);
	public readonly BuffBool AttackWhenGrabbing = new(false);
	public readonly BuffBool AttackWhenRush = new(false);
	public readonly BuffBool AttackWhenPounding = new(false);


	#endregion




	#region --- Health ---


	public readonly BuffInt MaxHP = new(1);
	public readonly BuffInt InvincibleDuration = new(120);
	public readonly BuffInt DamageStunDuration = new(24);
	public readonly BuffInt KnockBackSpeed = new(64);
	public readonly BuffInt KnockbackDeceleration = new(16);
	public readonly BuffBool InvincibleOnDash = new(false);
	public readonly BuffBool InvincibleOnRush = new(false);


	#endregion




}