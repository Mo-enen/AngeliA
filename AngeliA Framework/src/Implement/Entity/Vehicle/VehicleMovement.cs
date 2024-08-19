using System.Collections.Generic;
using System.Collections;

namespace AngeliA;

public abstract class VehicleMovement : CharacterMovement {




	#region --- VAR ---


	// Api
	public sealed override bool SyncFromConfigFile => false;
	public Character Driver { get; set; } = null;


	#endregion




	#region --- MSG ---


	public VehicleMovement (Rigidbody rig) : base(rig) {
		InitializeMeta();
	}


	#endregion




	#region --- API ---

	protected virtual void InitializeMeta () {
		WalkAvailable.BaseValue = false;
		SquatAvailable.BaseValue = false;
		PoundAvailable.BaseValue = false;
		DashAvailable.BaseValue = false;
		RushAvailable.BaseValue = false;
		CrashAvailable.BaseValue = false;
		SlideAvailable.BaseValue = false;
		GrabTopAvailable.BaseValue = false;
		GrabSideAvailable.BaseValue = false;
		FlyAvailable.BaseValue = false;
		ClimbAvailable.BaseValue = false;
	}

	// Movement Logic
	public override void Move (Direction3 x, Direction3 y) => MoveLogic((int)x, (int)y);

	public override void Stop () {
		MoveLogic(0, 0);
		VelocityX = 0;
	}

	public override void Jump () {

	}

	public override void HoldJump (bool holding) {


	}

	public override void Crash () { }

	public override void Dash () { }

	public override void Pound () { }

	public override void Rush () { }

	// Misc
	protected override CharacterMovementState GetMovementState () => CharacterMovementState.Idle;


	#endregion




	#region --- LGC ---



	#endregion




}
