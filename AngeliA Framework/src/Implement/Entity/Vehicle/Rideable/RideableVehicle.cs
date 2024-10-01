using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

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
			Rect.Shrink(shrinkX, shrinkX, 0, 0).EdgeOutside(Direction4.Up, 1),
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
		if (Driver == null || !Driver.Active) return true;
		// For Player
		if (Driver == PlayerSystem.Selecting && Input.GameKeyDown(Gamekey.Select)) {
			Input.UseGameKey(Gamekey.Select);
			Driver.VelocityY = 56;
			Driver.CancelIgnorePhysics();
			return true;
		}
		ControlHintUI.AddHint(Gamekey.Select, BuiltInText.HINT_STOP_DRIVE);
		return false;
	}


	protected virtual void OverrideDriverAnimation (PoseCharacterRenderer renderer) => renderer.ManualPoseAnimate(PoseAnimation_Ride.TYPE_ID);


}
