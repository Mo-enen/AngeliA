using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
public class OnewayGateDown : Oneway {
	public override Direction4 GateDirection => Direction4.Down;
}


public class OnewayGateLeft : Oneway {
	public override Direction4 GateDirection => Direction4.Left;
}


public class OnewayGateRight : Oneway {
	public override Direction4 GateDirection => Direction4.Right;
}


public class OnewayGateUp : Oneway {
	public override Direction4 GateDirection => Direction4.Up;
}


public abstract class Oneway : EnvironmentEntity {


	// Const
	private const int MASK = PhysicsMask.DYNAMIC;

	// Api
	public abstract Direction4 GateDirection { get; }
	protected int ReboundFrame { get; private set; } = int.MinValue;

	// Data
	private int LastContactFrame = int.MinValue;


	// MSG
	public override void OnActivated () {
		Width = Const.CEL;
		Height = Const.CEL;
	}


	public override void PhysicsUpdate () {
		int frame = Game.GlobalFrame;
		if (ContactReboundUpdate(frame)) {
			if (LastContactFrame < frame - 1) {
				ReboundFrame = frame;
			}
			LastContactFrame = frame;
		}
		base.PhysicsUpdate();
	}


	public override void FillPhysics () {
		CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, Util.GetOnewayTag(GateDirection));
	}


	public override void FrameUpdate () {
		int frame = Game.GlobalFrame;
		var rect = Rect;
		int rotDelta = 0;
		if (frame < ReboundFrame + 4) {
			rect.y += (ReboundFrame - frame + 4) * 8;
			rotDelta = (ReboundFrame - frame + 4) * 2 * (frame % 2 == 0 ? -1 : 1);
		}
		CellRenderer.Draw(
			TypeID,
			rect.x + rect.width / 2,
			rect.y + rect.height / 2,
			500, 500, rotDelta,
			rect.width,
			rect.height
		);
		base.FrameUpdate();
	}


	protected virtual bool ContactReboundUpdate (int frame) {
		var rect = Rect;
		bool contact = false;
		const int GAP = 1;
		IRect edge = GateDirection switch {
			Direction4.Down => new(rect.x, rect.y - GAP, rect.width, GAP),
			Direction4.Up => new(rect.x, rect.yMax, rect.width, GAP),
			Direction4.Left => new(rect.x - GAP, rect.y, GAP, rect.height),
			Direction4.Right => new(rect.xMax, rect.y, GAP, rect.height),
			_ => throw new System.NotImplementedException(),
		};
		var hits = CellPhysics.OverlapAll(MASK, edge, out int count, this);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (
				hit.Entity is Rigidbody rig &&
				!rig.Rect.Overlaps(rect.Shrink(2))
			) {
				contact = true;
				break;
			}
		}
		return contact;
	}



}
