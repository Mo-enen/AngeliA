using System.Collections;
using System.Collections.Generic;

using AngeliA;namespace AngeliA.Platformer;

[EntityAttribute.UpdateOutOfRange]
public abstract class RoutinePlatform : StepTriggerPlatform, IRouteWalker {


	// Api
	protected virtual int DefaultSpeed => 24;
	protected virtual int AirDragX => 200;
	protected virtual int Gravity => 1;
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
	private bool RequireSpreadLeadership;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Leader = null;
		LeaderOffset = default;
		MoveSpeed = DefaultSpeed;
		CurrentDirection = default;
		TargetPosition = new(X, Y);
		FreeFalling = false;
		RequireSpreadLeadership = false;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		// Update Leader
		if (Leader == this) {
			UpdateForLeader();
		}
	}


	private void UpdateForLeader () {

		LeadPos.x = X;
		LeadPos.y = Y;

		if (!FreeFalling) {
			// Moving
			var oldTargetPos = TargetPosition;
			IRouteWalker.MoveToRoute(this, PlatformPath.TYPE_ID, MoveSpeed, out int newX, out int newY);
			LeadPos.x = newX;
			LeadPos.y = newY;
			// Freefall Check
			if (
				TargetPosition != oldTargetPos &&
				WorldSquad.Front.GetBlockAt(
					(oldTargetPos.x + Width / 2).ToUnit(),
					(oldTargetPos.y + Height / 2).ToUnit(),
					BlockType.Element
				) != PlatformPath.TYPE_ID
			) {
				// Walking >> Freefalling
				FreeFalling = true;
				var normal = CurrentDirection.Normal();
				int speed = MoveSpeed;
				if (normal.x != 0 && normal.y != 0) {
					speed = speed * 100000 / 141421;
				}
				FreeFallVelocity = normal * speed;
			}
		} else {
			// Freefall
			LeadPos += FreeFallVelocity;
			if (AirDragX >= 1000) {
				FreeFallVelocity.x = FreeFallVelocity.x.MoveTowards(0, AirDragX / 1000);
			} else if ((Game.GlobalFrame - LastTriggerFrame) % (1000 / AirDragX) == 0) {
				FreeFallVelocity.x = FreeFallVelocity.x.MoveTowards(0, 1);
			}
			FreeFallVelocity.y = (FreeFallVelocity.y - Gravity).Clamp(-MaxFallingSpeed, MaxFallingSpeed);
			// Walking Check
			int entranceY = Y.ToUnifyGlobal();
			if (Y > entranceY && LeadPos.y <= entranceY) {
				// Crossing the Line
				int unitX = (X + Width / 2).ToUnit();
				int unitY = (Y + Height / 2).ToUnit();
				if (WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Element) == PlatformPath.TYPE_ID) {
					if (IRouteWalker.GetRouteFromMap(unitX, unitY, CurrentDirection, out var newDir, PlatformPath.TYPE_ID)) {
						CurrentDirection = newDir;
					}
					LeadPos.y = unitY.ToGlobal();
					TargetPosition = new Int2(unitX.ToGlobal(), unitY.ToGlobal());
					FreeFalling = false;
				}
			}
		}

	}


	public override void Update () {
		base.Update();
		// Spread Leadership
		if (RequireSpreadLeadership) {
			RequireSpreadLeadership = false;
			IUnitable.ForAllPartializedEntity<RoutinePlatform>(
				PhysicsMask.ENVIRONMENT, TypeID, Rect,
				OperationMode.ColliderAndTrigger, TriggerMode,
				SetLeader, this
			);
			static void SetLeader (RoutinePlatform other) {
				if (IUnitable.UniteTempParam is not RoutinePlatform leader) return;
				other.Leader = leader;
				other.LeaderOffset.x = other.X - leader.X;
				other.LeaderOffset.y = other.Y - leader.Y;
			}
		}
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
		RequireSpreadLeadership = true;

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

	}


}
