using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Entity that do not allow rigidbody pass through on given direction
/// </summary>
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Contraption")]
public abstract class Oneway : Entity, IBlockEntity {


	// Const
	private const int MASK = PhysicsMask.DYNAMIC;

	// Api
	/// <summary>
	/// Target moving opposite with this direction get blocked
	/// </summary>
	public abstract Direction4 GateDirection { get; }
	protected int LastReboundFrame { get; private set; } = int.MinValue;

	// Data
	private int LastContactFrame = int.MinValue;


	// MSG
	public override void OnActivated () {
		Width = Const.CEL;
		Height = Const.CEL;
	}


	public override void Update () {
		int frame = Game.GlobalFrame;
		if (ContactReboundUpdate(frame)) {
			if (LastContactFrame < frame - 1) {
				LastReboundFrame = frame;
			}
			LastContactFrame = frame;
		}
		base.Update();
	}


	public override void FirstUpdate () {
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, FrameworkUtil.GetOnewayTag(GateDirection));
	}


	public override void LateUpdate () {
		int frame = Game.GlobalFrame;
		var rect = Rect;
		int rotDelta = 0;
		if (frame < LastReboundFrame + 4) {
			rect.y += (LastReboundFrame - frame + 4) * 8;
			rotDelta = (LastReboundFrame - frame + 4) * 2 * (frame % 2 == 0 ? -1 : 1);
		}
		Renderer.Draw(
			TypeID,
			rect.x + rect.width / 2,
			rect.y + rect.height / 2,
			500, 500, rotDelta,
			rect.width,
			rect.height
		);
		base.LateUpdate();
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
		var hits = Physics.OverlapAll(MASK, edge, out int count, this);
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
