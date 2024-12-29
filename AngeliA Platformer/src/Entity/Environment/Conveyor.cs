using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Conveyor : Entity, IBlockEntity {


	// Api
	protected FittingPose Pose { get; private set; } = FittingPose.Unknown;
	protected abstract int MoveSpeed { get; }
	protected abstract int ArtCodeLeft { get; }
	protected abstract int ArtCodeMid { get; }
	protected abstract int ArtCodeRight { get; }
	protected abstract int ArtCodeSingle { get; }

	// Data
	private static readonly HashSet<int> ConveyorSet = [];


	// MSG

	[OnGameInitialize]
	internal static void OnGameInitialize () {
		ConveyorSet.Clear();
		foreach (var type in typeof(Conveyor).AllChildClass()) {
			ConveyorSet.Add(type.AngeHash());
		}
	}


	public override void OnActivated () {
		base.OnActivated();
		// Get Pose
		int unitX = X.ToUnit();
		int unitY = Y.ToUnit();
		int idL = WorldSquad.Front.GetBlockAt(unitX - 1, unitY, BlockType.Entity);
		int idR = WorldSquad.Front.GetBlockAt(unitX + 1, unitY, BlockType.Entity);
		bool hasLeft = ConveyorSet.Contains(idL);
		bool hasRight = ConveyorSet.Contains(idR);
		Pose =
			hasLeft && hasRight ? FittingPose.Mid :
			hasLeft && !hasRight ? FittingPose.Right :
			!hasLeft && hasRight ? FittingPose.Left :
			FittingPose.Single;
	}


	void IBlockEntity.OnEntityRefresh () => OnActivated();


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();

		// Carry Objects on Top
		ICarrier.CarryTargetsOnTopHorizontally(this, MoveSpeed, OperationMode.ColliderAndTrigger);

		// Scratch Objects on Bottom
		var hits = Physics.OverlapAll(PhysicsMask.DYNAMIC, Rect.EdgeOutside(Direction4.Down), out int count, this, OperationMode.ColliderAndTrigger);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not Rigidbody rig) continue;
			rig.SetMomentum(-MoveSpeed, 0);


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