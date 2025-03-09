using System.Collections.Generic;
using System.Collections;

using AngeliA;
namespace AngeliA.Platformer;

public abstract class VehicleMovement : CharacterMovement {




	#region --- VAR ---


	// Api
	public sealed override bool SyncFromConfigFile => false;
	public Character Driver { get; set; } = null;


	#endregion




	#region --- MSG ---


	public VehicleMovement (Rigidbody rig) : base(rig) => InitializeMeta();


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

		JumpCount.BaseValue = 0;
		SwimWidthAmount.BaseValue = 1000;
		MovementWidth.BaseValue = Const.CEL;
		MovementHeight.BaseValue = Const.CEL;
		DashHeightAmount.BaseValue = 1000;
		RushHeightAmount.BaseValue = 1000;
		SquatHeightAmount.BaseValue = 1000;
		SwimHeightAmount.BaseValue = 1000;
		FlyHeightAmount.BaseValue = 1000;
		GrabTopHeightAmount.BaseValue = 1000;
		GrabSideHeightAmount.BaseValue = 1000;

	}


	#endregion




	#region --- API ---


	// Movement Logic
	public override void Move (Direction3 x, Direction3 y, bool walk = false) => MoveLogic((int)x, (int)y, walk);

	public override void Stop () {
		MoveLogic(0, 0);
		VelocityX = 0;
	}

	public override void Jump (bool isSquatJump = false) {
		if (JumpCount > 0) base.Jump(isSquatJump);
	}

	public override void HoldJump (bool holding) {
		if (JumpCount > 0) base.HoldJump(holding);
	}

	public override void Crash () {
		if (CrashAvailable) base.Crash();
	}

	public override void Dash () {
		if (DashAvailable) base.Dash();
	}

	public override void Pound () {
		if (PoundAvailable) base.Pound();
	}

	public override void Rush () {
		if (RushAvailable) base.Rush();
	}


	#endregion




}
