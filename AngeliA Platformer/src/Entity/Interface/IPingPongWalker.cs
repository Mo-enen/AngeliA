using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public interface IPingPongWalker {

	// VAR
	public int WalkSpeed { get; }
	public bool WalkOffEdge { get; }
	public int TurningCooldown => 20;
	public bool OnlyWalkWhenGrounded => true;
	public int TurningCheckMask => PhysicsMask.MAP;
	public int LastTurnFrame { get; set; }
	public bool WalkingRight { get; set; }

	// MSG
	public static void OnActive (IPingPongWalker walker) {
		walker.LastTurnFrame = int.MinValue;
		walker.WalkingRight = true;
	}

	public static void PingPongWalk (IPingPongWalker walker) {

		if (walker is not Entity walkingEntity) return;
		if (walker.WalkSpeed == 0) {
			if (walker is Rigidbody _walkingRig) {
				_walkingRig.VelocityX = 0;
			}
			return;
		}

		// Grounded Check
		if (walker is Rigidbody rig && walker.OnlyWalkWhenGrounded && !rig.IsGrounded) return;

		// Wall Hit Check
		if (Game.GlobalFrame > walker.LastTurnFrame + walker.TurningCooldown) {
			var hitCheckRect = walkingEntity.Rect.Shrink(0, 0, 1, 1).EdgeOutside(
				walker.WalkingRight ? Direction4.Right : Direction4.Left, 1
			);
			if (
				Physics.Overlap(walker.TurningCheckMask, hitCheckRect, walkingEntity) ||
				!Physics.RoomCheckOneway(walker.TurningCheckMask, hitCheckRect, walkingEntity, walker.WalkingRight ? Direction4.Right : Direction4.Left)
			) {
				walker.WalkingRight = !walker.WalkingRight;
				walker.LastTurnFrame = Game.GlobalFrame;
			}

			// Walk Off Edge Check
			if (!walker.WalkOffEdge) {
				hitCheckRect = IRect.Point(walker.WalkingRight ? walkingEntity.X + walkingEntity.Width : walkingEntity.X - 1, walkingEntity.Y - 1);
				if (
					!Physics.Overlap(walker.TurningCheckMask, hitCheckRect, walkingEntity) &&
					Physics.RoomCheckOneway(walker.TurningCheckMask, hitCheckRect, walkingEntity, Direction4.Down)
				) {
					walker.WalkingRight = !walker.WalkingRight;
					walker.LastTurnFrame = Game.GlobalFrame;
				}
			}
		}

		// Perform Walk
		if (walker is Rigidbody walkingRig) {
			walkingRig.VelocityX = walker.WalkingRight ? walker.WalkSpeed : -walker.WalkSpeed;
		} else {
			walkingEntity.X += walker.WalkingRight ? walker.WalkSpeed : -walker.WalkSpeed;
		}

	}

}
