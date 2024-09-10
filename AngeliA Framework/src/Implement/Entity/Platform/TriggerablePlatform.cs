using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class TriggerablePlatform : Platform {

	protected enum PlatformTriggerMode { Horizontal, Vertical, FourDirection, EightDirection, }

	protected virtual PlatformTriggerMode TriggerMode => PlatformTriggerMode.Horizontal;

	private static readonly Pipe<Int2> TriggeringPositions = new(512);
	private int TriggeredFrame = -1;
	protected object TriggeredData = null;

	public override void OnActivated () {
		base.OnActivated();
		TriggeredFrame = -1;
		TriggeredData = null;
	}

	protected abstract void OnTriggered (object data);

	public void Trigger (object data = null) {

		// Trigger All Connected Platforms
		int frame = Game.PauselessFrame;
		TriggeringPositions.Reset();
		TriggeringPositions.LinkToTail(new Int2(X, Y));
		var rect = new IRect(0, 0, 1, 1);
		while (TriggeringPositions.TryPopHead(out var pos)) {
			rect.x = pos.x + Const.HALF;
			rect.y = pos.y + Const.HALF;

			if (Physics.GetEntity(
				TypeID, rect, PhysicsMask.ENVIRONMENT, mode: OperationMode.ColliderAndTrigger
			) is not DroppingPlatform platform) continue;
			if (platform.TriggeredFrame >= 0) continue;

			// Invoke
			platform.TriggeredData = data;
			platform.TriggeredFrame = frame;
			platform.OnTriggered(data);

			// Link Connected
			switch (TriggerMode) {
				case PlatformTriggerMode.Horizontal:
					TriggeringPositions.LinkToTail(pos.Shift(-Const.CEL, 0));
					TriggeringPositions.LinkToTail(pos.Shift(Const.CEL, 0));
					break;
				case PlatformTriggerMode.Vertical:
					TriggeringPositions.LinkToTail(pos.Shift(0, -Const.CEL));
					TriggeringPositions.LinkToTail(pos.Shift(0, Const.CEL));
					break;
				case PlatformTriggerMode.FourDirection:
					TriggeringPositions.LinkToTail(pos.Shift(-Const.CEL, 0));
					TriggeringPositions.LinkToTail(pos.Shift(Const.CEL, 0));
					TriggeringPositions.LinkToTail(pos.Shift(0, -Const.CEL));
					TriggeringPositions.LinkToTail(pos.Shift(0, Const.CEL));
					break;
				case PlatformTriggerMode.EightDirection:
					TriggeringPositions.LinkToTail(pos.Shift(-Const.CEL, -Const.CEL));
					TriggeringPositions.LinkToTail(pos.Shift(-Const.CEL, Const.CEL));
					TriggeringPositions.LinkToTail(pos.Shift(Const.CEL, -Const.CEL));
					TriggeringPositions.LinkToTail(pos.Shift(Const.CEL, Const.CEL));
					TriggeringPositions.LinkToTail(pos.Shift(0, -Const.CEL));
					TriggeringPositions.LinkToTail(pos.Shift(0, Const.CEL));
					TriggeringPositions.LinkToTail(pos.Shift(-Const.CEL, 0));
					TriggeringPositions.LinkToTail(pos.Shift(Const.CEL, 0));
					break;
			}
		}
		TriggeringPositions.Reset();
	}

}
