using System.Collections;
using System.Collections.Generic;

using AngeliA;

namespace AngeliA.Platformer;

public abstract class RideableVehicle<RM> : Vehicle<RM> where RM : RideableMovement {


	public override int StartDriveCooldown => 42;
	protected virtual bool FreeWandering => _FreeWandering;
	protected readonly RigidbodyNavigation Navigation;
	private Int2 SettledPosition = default;
	private bool _FreeWandering = true;


	// MSG
	public RideableVehicle () => Navigation = new(this);


	public override void OnActivated () {
		base.OnActivated();
		_FreeWandering = true;
		SettledPosition.x = X;
		SettledPosition.y = Y;
		Navigation.OnActivated();
	}


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


	public override void Update () {
		base.Update();

		if (Driver != null || !Navigation.NavigationEnable) return;
		if (Game.GlobalFrame < LastDriveChangedFrame + 60) return;

		bool firstFrameWandering = false;
		if (!_FreeWandering) {
			if (!IsGrounded) return;
			firstFrameWandering = true;
			_FreeWandering = true;
			SettledPosition.x = X;
			SettledPosition.y = Y;
			Navigation.ResetNavigation();
		}

		// Free Time
		IgnorePhysics.True(1);
		FillAsTrigger(1);
		if (Movement.LastStartMoveFrame >= 0) {
			Movement.RunSpeed.Override(
				Util.Min(Movement.RunSpeed.BaseValue, Game.GlobalFrame - Movement.LastStartMoveFrame), 1
			);
		} else {
			Movement.RunSpeed.Override(6, 1);
		}

		// Free Wandering
		if (Game.GlobalFrame % 60 == 30 || firstFrameWandering) {
			var aimPosition = PlatformerUtil.NavigationFreeWandering(
				new Int2(SettledPosition.x + Const.HALF, SettledPosition.y + Const.HALF),
				this, out bool grounded,
				frequency: 60 * 30,
				maxDistance: Const.CEL * 6
			);
			Navigation.NavigationAim = aimPosition;
			Navigation.NavigationAimGrounded = grounded;
		}

		// Update
		if (!firstFrameWandering) {
			Navigation.PhysicsUpdate();
		}
	}


	public override void StopDrive () {
		base.StopDrive();
		_FreeWandering = false;
	}


	protected override bool CheckForStartDrive (out Character driver) {

		driver = null;

		// Check for New Driver Join
		var hits = Physics.OverlapAll(
			PhysicsMask.CHARACTER,
			Rect.EdgeOutside(Direction4.Up, 32),
			out int count, this
		);
		for (int i = 0; i < count; i++) {
			if (
				hits[i].Entity is Character characterHit &&
				characterHit.Y >= Rect.CenterY() &&
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
