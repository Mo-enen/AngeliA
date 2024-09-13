using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.UpdateOutOfRange]
public abstract class RoutinePlatform : StepTriggerPlatform, IRouteWalker {


	// Api
	protected virtual int DefaultSpeed => 24;
	protected virtual int AirDragX => 1;
	protected virtual int Gravity => 2;
	protected virtual int MaxFallingSpeed => 42;
	public Direction8 CurrentDirection { get; set; }
	public Int2 TargetPosition { get; set; }

	// Data
	private RoutinePlatform Leader = null;
	private Int2 LeaderOffset;
	private Int2 LeadPos;
	private int MoveSpeed;
	private bool FreeFalling;
	private Int2 FreeFallVelocity;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Leader = null;
		LeaderOffset = default;
		MoveSpeed = DefaultSpeed;
		CurrentDirection = default;
		TargetPosition = new(X, Y);
		FreeFalling = false;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		if (Leader == this) {
			UpdateForLeader();
		}
	}


	private void UpdateForLeader () {

		LeadPos.x = X;
		LeadPos.y = Y;

		if (!FreeFalling) {
			// Moving
			IRouteWalker.MoveToRoute(this, PlatformPath.TYPE_ID, MoveSpeed, out int newX, out int newY);
			LeadPos.x = newX;
			LeadPos.y = newY;
		} else {
			// Freefall
			LeadPos += FreeFallVelocity;
			FreeFallVelocity.x = FreeFallVelocity.x.MoveTowards(0, AirDragX);
			FreeFallVelocity.y = (FreeFallVelocity.y - Gravity).Clamp(-MaxFallingSpeed, MaxFallingSpeed);
		}

		// Freefall Check
		int unitX = (LeadPos.x + Width / 2).ToUnit();
		int unitY = (LeadPos.y + Height / 2).ToUnit();
		bool hasIndicator = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Element) == PlatformPath.TYPE_ID;
		if (hasIndicator == FreeFalling) {
			FreeFalling = !hasIndicator;
			if (FreeFalling) {
				// Walking >> Freefalling
				var normal = CurrentDirection.Normal();
				int speed = MoveSpeed;
				if (normal.x != 0 && normal.y != 0) {
					speed = speed * 100000 / 141421;
				}
				FreeFallVelocity = normal * speed;
			} else {
				// Freefalling >> Walking
				if (IRouteWalker.GetRouteFromMap(unitX, unitY, CurrentDirection, out var newDir, PlatformPath.TYPE_ID)) {
					CurrentDirection = newDir;
					TargetPosition = new Int2(X, Y);
				}
			}
		}

	}


	public override void Update () {
		base.Update();
		// Active Check
		if (Leader == this) {
			if (FreeFalling && !Stage.SpawnRect.Overlaps(Rect)) {
				Active = false;
			}
		} else if (Leader != null && !Leader.Active) {
			Active = false;
		}
	}


	protected override void Move () {
		if (Leader == null) return;
		if (Leader == this) {
			// Move Leader
			X = LeadPos.x;
			Y = LeadPos.y;
		} else {
			// Following Leader
			X = Leader.LeadPos.x + LeaderOffset.x;
			Y = Leader.LeadPos.y + LeaderOffset.y;
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

		// Get Initial Direction/Pos
		if (IRouteWalker.GetRouteFromMap(unitX, unitY, CurrentDirection, out var newDir, PlatformPath.TYPE_ID)) {
			CurrentDirection = newDir;
			TargetPosition = new Int2(X, Y);
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
