using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class CharacterRenderer (Character target) {

	// VAR
	private static readonly int[] BOUNCE_AMOUNTS = [500, 200, 100, 50, 25, 50, 100, 200, 500,];
	private static readonly int[] BOUNCE_AMOUNTS_BIG = [0, -600, -900, -1200, -1400, -1200, -900, -600, 0,];
	public readonly Character TargetCharacter = target;
	public int CurrentAnimationFrame { get; set; } = 0;
	public int CurrentRenderingBounce { get; private set; } = 1000;

	// MSG
	public virtual void OnActivated () { }
	public virtual void BeforeUpdate () { }
	public void UpdateForBounce () {

		var Movement = TargetCharacter.Movement;
		var Attackness = TargetCharacter.Attackness;
		var LastRequireBounceFrame = TargetCharacter.LastRequireBounceFrame;

		int frame = Game.GlobalFrame;
		int bounce = 1000;
		int duration = BOUNCE_AMOUNTS.Length;
		bool reverse = false;
		bool isPounding = Movement.MovementState == CharacterMovementState.Pound;
		bool isSquatting = Movement.MovementState == CharacterMovementState.SquatIdle || Movement.MovementState == CharacterMovementState.SquatMove;
		if (frame < LastRequireBounceFrame + duration) {
			bounce = TargetCharacter.InWater ? BOUNCE_AMOUNTS_BIG[frame - LastRequireBounceFrame] : BOUNCE_AMOUNTS[frame - LastRequireBounceFrame];
			if (Attackness.AttackChargeStartFrame.HasValue && Game.GlobalFrame > Attackness.AttackChargeStartFrame.Value + Attackness.MinimalChargeAttackDuration) {
				bounce += (1000 - bounce) / 2;
			}
		} else if (isPounding) {
			bounce = 1500;
		} else if (TargetCharacter.IsGrounded && frame.InRangeExclude(Movement.LastPoundingFrame, Movement.LastPoundingFrame + duration)) {
			// Gound Pound End
			bounce = BOUNCE_AMOUNTS_BIG[frame - Movement.LastPoundingFrame];
		} else if (isSquatting && frame.InRangeExclude(Movement.LastSquatFrame, Movement.LastSquatFrame + duration)) {
			// Squat Start
			bounce = BOUNCE_AMOUNTS[frame - Movement.LastSquatFrame];
		} else if (TargetCharacter.IsGrounded && frame.InRangeExclude(Movement.LastGroundFrame, Movement.LastGroundFrame + duration)) {
			// Gounded Start
			bounce = BOUNCE_AMOUNTS[frame - Movement.LastGroundFrame];
		} else if (!isSquatting && frame.InRangeExclude(Movement.LastSquattingFrame, Movement.LastSquattingFrame + duration)) {
			// Squat End
			bounce = BOUNCE_AMOUNTS[frame - Movement.LastSquattingFrame];
			reverse = true;
		} else if (Movement.IsCrashing && frame.InRangeExclude(Movement.LastCrashFrame, Movement.LastCrashFrame + duration)) {
			// Crash Start
			bounce = BOUNCE_AMOUNTS_BIG[frame - Movement.LastCrashFrame];
		}
		if (bounce != 1000) {
			bounce = Util.RemapUnclamped(0, 1000, (1000 - TargetCharacter.Bouncy).Clamp(0, 999), 1000, bounce);
		}
		CurrentRenderingBounce = reverse ? -bounce : bounce;
	}
	public virtual void LateUpdate () { }
	public void GrowAnimationFrame () {

		int frame = CurrentAnimationFrame;
		var Movement = TargetCharacter.Movement;

		switch (Movement.MovementState) {

			case CharacterMovementState.Climb:
				int climbVelocity = Movement.IntendedY != 0 ? Movement.IntendedY : Movement.IntendedX;
				if (climbVelocity > 0) {
					frame++;
				} else if (climbVelocity < 0) {
					frame--;
				}
				break;

			case CharacterMovementState.GrabTop:
				if (Movement.IntendedX > 0) {
					frame++;
				} else if (Movement.IntendedX < 0) {
					frame--;
				}
				break;

			case CharacterMovementState.GrabSide:
				if (Movement.IntendedY > 0) {
					frame++;
				} else if (Movement.IntendedY < 0) {
					frame--;
				}
				break;

			case CharacterMovementState.GrabFlip:
				frame += TargetCharacter.VelocityY > 0 ? 1 : -1;
				break;

			case CharacterMovementState.Run:
			case CharacterMovementState.Walk:
				frame += Movement.IntendedX > 0 == Movement.FacingRight ? 1 : -1;
				break;

			case CharacterMovementState.Rush:
				if (TargetCharacter.VelocityX == 0 || TargetCharacter.VelocityX > 0 == Movement.FacingRight) {
					frame++;
				} else {
					frame = 0;
				}
				break;

			default:
				frame++;
				break;

		}

		// Final
		CurrentAnimationFrame = frame;
	}

}
