using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class DroppingPlatform : Platform {


	protected virtual bool DropOnRigidbodyTouch => false;
	protected virtual bool DropOnCharacterTouch => false;
	protected virtual bool DropOnPlayerTouch => true;
	protected virtual int InitialDropSpeed => 0;
	protected virtual int MaxDropSpeed => 96;
	protected virtual int DropAcceleration => -3;

	// Data
	private static readonly Pipe<Int2> DroppingPositions = new(512);
	private bool Dropping = false;
	private int CurrentSpeedY = 0;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Dropping = false;
		CurrentSpeedY = 0;
	}


	protected override void Move () {
		if (!Dropping) return;
		Y += CurrentSpeedY;
		CurrentSpeedY = (CurrentSpeedY + DropAcceleration).Clamp(-MaxDropSpeed, MaxDropSpeed);
	}


	protected override void OnRigidbodyTouched (Rigidbody rig) {
		base.OnRigidbodyTouched(rig);
		if (!DropOnRigidbodyTouch) return;
		StartDrop();
	}
	protected override void OnCharacterTouched (Character character) {
		base.OnCharacterTouched(character);
		if (!DropOnCharacterTouch) return;
		StartDrop();
	}
	protected override void OnPlayerTouched (Player player) {
		base.OnPlayerTouched(player);
		if (!DropOnPlayerTouch) return;
		StartDrop();
	}


	// API
	public void StartDrop () {
		// Drop All Connected Platforms
		DroppingPositions.Reset();
		DroppingPositions.LinkToTail(new Int2(X, Y));
		var rect = new IRect(0, 0, 1, 1);
		while (DroppingPositions.TryPopHead(out var pos)) {
			rect.x = pos.x + Const.HALF;
			rect.y = pos.y + Const.HALF;
			if (Physics.GetEntity(
				TypeID, rect, PhysicsMask.ENVIRONMENT, mode: OperationMode.ColliderAndTrigger
			) is not DroppingPlatform platform) continue;
			if (platform.Dropping) continue;
			platform.DropLogic();
			// Link Connected
			DroppingPositions.LinkToTail(pos.Shift(-Const.CEL, 0));
			DroppingPositions.LinkToTail(pos.Shift(Const.CEL, 0));
			DroppingPositions.LinkToTail(pos.Shift(0, -Const.CEL));
			DroppingPositions.LinkToTail(pos.Shift(0, Const.CEL));
		}
		DroppingPositions.Reset();
	}


	private void DropLogic () {
		CurrentSpeedY = InitialDropSpeed;
		Dropping = true;
	}


}
