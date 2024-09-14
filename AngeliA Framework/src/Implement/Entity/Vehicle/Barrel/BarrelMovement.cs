namespace AngeliA;

public class BarrelMovement : VehicleMovement {
	public BarrelMovement (Rigidbody rig) : base(rig) { }
	public override void Jump () {
		if (Driver == null) return;
		Driver.VelocityY = JumpSpeed + Target.DeltaPositionY;
	}
	protected override void InitializeMeta () {
		base.InitializeMeta();
		RunSpeed.BaseValue = -20;
	}
	protected override CharacterMovementState GetMovementState () {
		var state = CharacterMovement.CalculateMovementState(this);
		return state == CharacterMovementState.Run ? CharacterMovementState.Walk : state;
	}
}
