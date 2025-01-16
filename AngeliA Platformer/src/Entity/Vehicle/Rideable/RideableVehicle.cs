using System.Collections;
using System.Collections.Generic;

using AngeliA;

namespace AngeliA.Platformer;

public abstract class RideableVehicle<RM> : Vehicle<RM> where RM : RideableMovement {


	public override int StartDriveCooldown => 42;


	// MSG
	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Driving
		if (Driver != null) {
			// Override Animation
			if (Driver.Rendering is PoseCharacterRenderer poseRenderer) {
				OverrideDriverAnimation(poseRenderer);
			}
			// For Player
			if (Driver == PlayerSystem.Selecting) {
				PlayerSystem.IgnorePlayerMenu();
			}
		}
	}


	protected override bool CheckForStartDrive (out Character driver) {

		driver = null;

		// Check for New Driver Join
		int shrinkX = DeltaPositionX.Abs() + 16;
		var hits = Physics.OverlapAll(
			PhysicsMask.CHARACTER,
			Rect.Shrink(shrinkX, shrinkX, 0, 0).EdgeOutside(Direction4.Up, 32),
			out int count, this
		);
		for (int i = 0; i < count; i++) {
			if (
				hits[i].Entity is Character characterHit &&
				characterHit.Y >= Rect.yMax &&
				characterHit.VelocityY <= VelocityY
			) {
				driver = characterHit;
				break;
			}
		}

		return driver != null;
	}


	protected override bool CheckForStopDrive () {

		if (base.CheckForStopDrive()) return true;

		// For Player
		var player = PlayerSystem.Selecting;
		if (Driver == player) {
			if (Input.GameKeyDown(Gamekey.Select) && !Physics.Overlap(player.CollisionMask, player.Rect.EdgeUp(Const.HALF), player)) {
				Input.UseGameKey(Gamekey.Select);
				Driver.VelocityX = VelocityX;
				Driver.VelocityY = 56;
				Driver.IgnorePhysics.False();
				VelocityY -= Driver.VelocityY / 2;
				VelocityX = -VelocityX;
				return true;
			}
			ControlHintUI.AddHint(Gamekey.Select, BuiltInText.HINT_STOP_DRIVE);
		}

		return false;
	}


	protected virtual void OverrideDriverAnimation (PoseCharacterRenderer renderer) => renderer.ManualPoseAnimate(PoseAnimation_Ride.TYPE_ID);


}
