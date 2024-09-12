using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class RoutinePlatform : StepTriggerPlatform, IRouteWalker {


	// Api
	protected virtual int DefaultSpeed => 24;
	public Direction8 CurrentDirection { get; set; }
	public Int2 TargetPosition { get; set; }

	// Data
	private RoutinePlatform Leader = null;
	private Int2 LeaderOffset;
	private int MoveSpeed;
	private Int2 LeadingPos;
	private bool FreeFalling;
	private Int2 FreeFallVelocity;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Leader = null;
		LeaderOffset = default;
		MoveSpeed = DefaultSpeed;
		CurrentDirection = default;
		TargetPosition = default;
		FreeFalling = false;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		UpdateForLeader();
	}


	private void UpdateForLeader () {
		if (Leader != this) return;

		LeadingPos.x = X;
		LeadingPos.y = Y;

		if (!FreeFalling) {
			// Moving



		} else {
			// Freefall



		}
	}


	protected override void Move () {
		if (Leader == null) return;
		if (Leader != this) {
			// Following Leader
			X = Leader.LeadingPos.x + LeaderOffset.x;
			Y = Leader.LeadingPos.y + LeaderOffset.y;
		} else {
			// Move Leader
			X = LeadingPos.x;
			Y = LeadingPos.y;
		}
	}


	// API
	protected override void OnTriggered (object data) {
		base.OnTriggered(data);
		if (Leader != null) return;

		var squad = WorldSquad.Front as IBlockSquad;
		int unitX = (X + Width / 2).ToUnit();
		int unitY = (Y + Height / 2).ToUnit();

		// Self Leader Check
		if (squad.GetBlockAt(unitX, unitY, Stage.ViewZ, BlockType.Element) != PlatformPath.TYPE_ID) return;

		// Get Speed from Map
		MoveSpeed = DefaultSpeed;
		if (
			squad.ReadSystemNumber(unitX, unitY + 1, Stage.ViewZ, Direction4.Down, out int number) ||
			squad.ReadSystemNumber(unitX, unitY - 1, Stage.ViewZ, Direction4.Down, out number)
		) {
			MoveSpeed = number;
		}

		// Set Leader for Other Platforms
		IPartializable.ForAllPartializedEntity<RoutinePlatform>(
			PhysicsMask.ENVIRONMENT, TypeID, Rect,
			OperationMode.ColliderAndTrigger, TriggerMode,
			SetLeader, this
		);
		static void SetLeader (RoutinePlatform other) {
			if (IPartializable.PartializeTempParam is not RoutinePlatform leader) return;
			other.Leader = leader;
			other.LeaderOffset.x = other.X - leader.X;
			other.LeaderOffset.y = other.Y - leader.Y;
		}
	}


}
