using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class CharacterAttackness {

	public readonly FrameBasedInt AttackComboGap = new(12);
	public readonly FrameBasedInt HoldAttackPunishFrame = new(4);
	public readonly FrameBasedBool CancelAttackOnJump = new(false);

	public readonly FrameBasedInt DefaultSpeedRateOnAttack = new(0);
	public readonly FrameBasedInt AirSpeedRateOnAttack = new(1000);
	public readonly FrameBasedInt WalkingSpeedRateOnAttack = new(0);
	public readonly FrameBasedInt RunningSpeedRateOnAttack = new(0);

	public readonly FrameBasedBool AttackInAir = new(true);
	public readonly FrameBasedBool AttackInWater = new(true);
	public readonly FrameBasedBool AttackWhenWalking = new(true);
	public readonly FrameBasedBool AttackWhenRunning = new(true);
	public readonly FrameBasedBool AttackWhenClimbing = new(false);
	public readonly FrameBasedBool AttackWhenFlying = new(false);
	public readonly FrameBasedBool AttackWhenRolling = new(false);
	public readonly FrameBasedBool AttackWhenSquatting = new(false);
	public readonly FrameBasedBool AttackWhenDashing = new(false);
	public readonly FrameBasedBool AttackWhenSliding = new(false);
	public readonly FrameBasedBool AttackWhenGrabbing = new(false);
	public readonly FrameBasedBool AttackWhenRush = new(false);
	public readonly FrameBasedBool AttackWhenPounding = new(false);

}
