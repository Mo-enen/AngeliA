using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Interface that makes object auto walks and turn around when collide on a solid block. (like Goomba in Mario games)
/// </summary>
public interface IPingPongWalker {

	// VAR
	public int WalkSpeed { get; }
	/// <summary>
	/// True if the target fall off edge
	/// </summary>
	public bool WalkOffEdge { get; }
	/// <summary>
	/// Frames it takes to turn around again
	/// </summary>
	public int TurningCooldown => 6;
	/// <summary>
	/// True if object don't walk when not touching ground
	/// </summary>
	public bool OnlyWalkWhenGrounded => true;
	/// <summary>
	/// Which physics layers is included for solid block checking
	/// </summary>
	public int TurningCheckMask => PhysicsMask.MAP;
	public int LastTurnFrame { get; set; }
	/// <summary>
	/// True if the object should walk to right 
	/// </summary>
	public bool WalkingRight { get; set; }

	// MSG
	/// <summary>
	/// Call this method when entity activated
	/// </summary>
	public static void OnActivated (IPingPongWalker walker) {
		walker.LastTurnFrame = int.MinValue;
		walker.WalkingRight = true;
	}

	/// <summary>
	/// Call this method every frame
	/// </summary>
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
