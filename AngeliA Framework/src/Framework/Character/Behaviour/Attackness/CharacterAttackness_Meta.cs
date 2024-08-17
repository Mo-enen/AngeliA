using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class CharacterAttackness {

	public readonly BuffInt AttackComboGap = new(12);
	public readonly BuffInt HoldAttackPunish = new(4);
	public readonly BuffBool CancelAttackOnJump = new(false);

	public readonly BuffInt DefaultSpeedRateOnAttack = new(0);
	public readonly BuffInt AirSpeedRateOnAttack = new(1000);
	public readonly BuffInt WalkingSpeedRateOnAttack = new(0);
	public readonly BuffInt RunningSpeedRateOnAttack = new(0);

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

}
