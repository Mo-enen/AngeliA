using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Movement for rolling on a barrel
/// </summary>
public class BarrelMovement : VehicleMovement {
	public BarrelMovement (Rigidbody rig) : base(rig) { }
	protected override void InitializeMeta () {
		base.InitializeMeta();
		SquatMoveSpeed.BaseValue = -10;
		WalkSpeed.BaseValue = -20;
		RunSpeed.BaseValue = -20;
		PushAvailable.BaseValue = false;
		PushSpeed.BaseValue = 0;
		JumpCount.BaseValue = 0;
	}
	protected override CharacterMovementState GetMovementState () {
		var state = CalculateMovementState(this);
		return state == CharacterMovementState.Run ? CharacterMovementState.Walk : state;
	}
}
