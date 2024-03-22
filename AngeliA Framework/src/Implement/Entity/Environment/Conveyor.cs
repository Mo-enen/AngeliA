using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 


// Wood
public class ConveyorWoodLeft : ConveyorWood { protected override int MoveSpeed => -12; }
public class ConveyorWoodRight : ConveyorWood { protected override int MoveSpeed => 12; }
[RequireSpriteFromField]
public abstract class ConveyorWood : Conveyor {

	private static readonly SpriteCode CODE_L = "ConveyorWood Left";
	private static readonly SpriteCode CODE_M = "ConveyorWood Mid";
	private static readonly SpriteCode CODE_R = "ConveyorWood Right";
	private static readonly SpriteCode CODE_S = "ConveyorWood Single";

	protected override int ArtCodeLeft => CODE_L;
	protected override int ArtCodeMid => CODE_M;
	protected override int ArtCodeRight => CODE_R;
	protected override int ArtCodeSingle => CODE_S;

}


// Iron
public class ConveyorIronLeft : ConveyorIron { protected override int MoveSpeed => -24; }
public class ConveyorIronRight : ConveyorIron { protected override int MoveSpeed => 24; }
[RequireSpriteFromField]
public abstract class ConveyorIron : Conveyor {

	private static readonly SpriteCode CODE_L = "ConveyorIron Left";
	private static readonly SpriteCode CODE_M = "ConveyorIron Mid";
	private static readonly SpriteCode CODE_R = "ConveyorIron Right";
	private static readonly SpriteCode CODE_S = "ConveyorIron Single";

	protected override int ArtCodeLeft => CODE_L;
	protected override int ArtCodeMid => CODE_M;
	protected override int ArtCodeRight => CODE_R;
	protected override int ArtCodeSingle => CODE_S;

}


public abstract class Conveyor : EnvironmentEntity {


	// Api
	protected FittingPose Pose { get; private set; } = FittingPose.Unknown;
	protected abstract int MoveSpeed { get; }
	protected abstract int ArtCodeLeft { get; }
	protected abstract int ArtCodeMid { get; }
	protected abstract int ArtCodeRight { get; }
	protected abstract int ArtCodeSingle { get; }


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		// Get Pose
		int unitX = X.ToUnit();
		int unitY = Y.ToUnit();
		bool hasLeft = WorldSquad.Front.GetBlockAt(unitX - 1, unitY, BlockType.Entity) == TypeID;
		bool hasRight = WorldSquad.Front.GetBlockAt(unitX + 1, unitY, BlockType.Entity) == TypeID;
		Pose =
			hasLeft && hasRight ? FittingPose.Mid :
			hasLeft && !hasRight ? FittingPose.Right :
			!hasLeft && hasRight ? FittingPose.Left :
			FittingPose.Single;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillBlock(PhysicsLayer.LEVEL, TypeID, Rect);
	}


	public override void Update () {
		base.Update();
		var rect = Rect;
		rect.y += rect.height;
		rect.height = 1;
		var hits = Physics.OverlapAll(PhysicsMask.ENTITY, rect, out int count, this);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is Rigidbody rig) {
				rig.PerformMove(MoveSpeed, 0);
				rig.Y = rect.yMax;
				rig.VelocityY = 0;
			}
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();
		int aFrame = (Game.GlobalFrame * Util.Abs(MoveSpeed) / 16).UMod(8);
		if (MoveSpeed > 0) aFrame = 7 - aFrame;
		if (Renderer.TryGetSpriteFromGroup(
			Pose switch {
				FittingPose.Left => ArtCodeLeft,
				FittingPose.Mid => ArtCodeMid,
				FittingPose.Right => ArtCodeRight,
				FittingPose.Single => ArtCodeSingle,
				_ => 0,
			}, aFrame, out var sprite, true, true
		)) {
			Renderer.Draw(sprite, base.Rect);
		}
	}


}