namespace Yaya {


	public enum MovementState {
		Idle = 0,
		Walk, Run, JumpUp, JumpDown,
		SwimIdle, SwimMove, SwimDash,
		SquatIdle, SquatMove,
		Dash, Roll, Pound, Climb, Fly,
	}


	public enum CharacterState {
		GamePlay = 0,
		Sleep = 1,
		Passout = 2,
	}


	public enum FittingPose {
		Unknown = 0,
		Left = 1,
		Down = 1,
		Mid = 2,
		Right = 3,
		Up = 3,
		Single = 4,
	}


}