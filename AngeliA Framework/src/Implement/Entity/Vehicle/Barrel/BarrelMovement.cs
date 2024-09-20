namespace AngeliA;

public class BarrelMovement : VehicleMovement {
	public BarrelMovement (Rigidbody rig) : base(rig) { }
	protected override void InitializeMeta () {
		base.InitializeMeta();
		SquatSpeed.BaseValue = -10;
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
