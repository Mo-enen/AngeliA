using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.MapEditorGroup("Contraption")]
public abstract class Track : Entity, IBlockEntity {

	// Api
	protected virtual int Speed => 12;
	protected virtual int HangGap => 0;
	protected virtual bool AllowStraightConnection => true;
	protected virtual bool AllowTiltConnection => true;
	protected virtual bool TurnBackWhenReachEnd => true;
	protected virtual bool SlowDownWhenWalkerOverlap => true;
	protected virtual bool LoadItemFromMapElement => true;
	protected abstract SpriteCode BodySprite { get; }
	protected abstract SpriteCode BodyTiltSprite { get; }
	protected abstract SpriteCode CenterSprite { get; }
	bool IBlockEntity.AllowBeingEmbedAsElement => false;

	// Data
	private static readonly HashSet<int> TrackSet = new(typeof(Track).AllChildClassID());
	private readonly bool[] HasTrackArr = [false, false, false, false, false, false, false, false, false];
	private Int4 Shrink;
	private int Burden;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		OnEntityRefresh();
		Burden = 0;
		if (LoadItemFromMapElement) {
			int id = WorldSquad.Front.GetBlockAt((X + 1).ToUnit(), (Y + 1).ToUnit(), Stage.ViewZ, BlockType.Element);
			if (id != 0 && IAutoTrackWalker.IsTypeAutoTrackWalker(id) && Stage.SpawnEntity(id, X, Y) is Entity entity) {
				entity.IgnoreReposition = true;
			}
		}
	}

	public void OnEntityRefresh () {

		var squad = WorldSquad.Front;
		int unitX = (X + 1).ToUnit();
		int unitY = (Y + 1).ToUnit();

		// 7 0 1
		// 6   2
		// 5 4 3

		HasTrackArr[0] = AllowStraightConnection && TrackSet.Contains(squad.GetBlockAt(unitX, unitY + 1, BlockType.Entity));
		HasTrackArr[2] = AllowStraightConnection && TrackSet.Contains(squad.GetBlockAt(unitX + 1, unitY, BlockType.Entity));
		HasTrackArr[4] = AllowStraightConnection && TrackSet.Contains(squad.GetBlockAt(unitX, unitY - 1, BlockType.Entity));
		HasTrackArr[6] = AllowStraightConnection && TrackSet.Contains(squad.GetBlockAt(unitX - 1, unitY, BlockType.Entity));

		HasTrackArr[1] = AllowTiltConnection && TrackSet.Contains(squad.GetBlockAt(unitX + 1, unitY + 1, BlockType.Entity));
		HasTrackArr[3] = AllowTiltConnection && TrackSet.Contains(squad.GetBlockAt(unitX + 1, unitY - 1, BlockType.Entity));
		HasTrackArr[5] = AllowTiltConnection && TrackSet.Contains(squad.GetBlockAt(unitX - 1, unitY - 1, BlockType.Entity));
		HasTrackArr[7] = AllowTiltConnection && TrackSet.Contains(squad.GetBlockAt(unitX - 1, unitY + 1, BlockType.Entity));

		HasTrackArr[8] =
			HasTrackArr[0] || HasTrackArr[1] || HasTrackArr[2] || HasTrackArr[3] ||
			HasTrackArr[4] || HasTrackArr[5] || HasTrackArr[6] || HasTrackArr[7];

		Shrink.left = !HasTrackArr[5] && !HasTrackArr[6] && !HasTrackArr[7] ? Const.QUARTER : HasTrackArr[5] || HasTrackArr[7] ? -Const.QUARTER : 0;
		Shrink.right = !HasTrackArr[1] && !HasTrackArr[2] && !HasTrackArr[3] ? Const.QUARTER : HasTrackArr[1] || HasTrackArr[3] ? -Const.QUARTER : 0;
		Shrink.down = !HasTrackArr[3] && !HasTrackArr[4] && !HasTrackArr[5] ? Const.QUARTER : HasTrackArr[3] || HasTrackArr[5] ? -Const.QUARTER : 0;
		Shrink.up = !HasTrackArr[0] && !HasTrackArr[1] && !HasTrackArr[7] ? Const.QUARTER : HasTrackArr[1] || HasTrackArr[7] ? -Const.QUARTER : 0;

	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();

		if (!HasTrackArr[8]) return;

		// Link TrackWalker
		var hits = Physics.OverlapAll(
			PhysicsMask.DYNAMIC, Rect.Shift(0, -HangGap).Shrink(Shrink),
			out int count, this, OperationMode.ColliderAndTrigger
		);
		int burden = 0;
		for (int i = 0; i < count; i++) {

			var hit = hits[i];
			var entity = hit.Entity;

			if (entity is not IAutoTrackWalker walker) continue;

			// Walked Check
			if (Game.GlobalFrame <= walker.LastWalkingFrame) continue;

			burden++;
			int eOffsetX = 0;
			int eOffsetY = 0;
			if (entity is Rigidbody offsetRig) {
				eOffsetX = offsetRig.OffsetX;
				eOffsetY = offsetRig.OffsetY;
			}

			// Start Walk
			if (Game.GlobalFrame <= entity.SpawnFrame + 1 || Game.GlobalFrame > walker.LastWalkingFrame + 1) {
				if (entity.Y + eOffsetY > Y - HangGap) continue;
				bool movingRight = entity is not Rigidbody _rig || _rig.DeltaPositionX >= 0;
				walker.CurrentDirection = movingRight ? Direction8.Right : Direction8.Left;
				for (int dIndex = 0; dIndex < 8; dIndex++) {
					int dir = movingRight ? dIndex : (8 - dIndex) % 8;
					if (HasTrackArr[dir]) {
						walker.CurrentDirection = (Direction8)dir;
						break;
					}
				}
				if (!walker.CurrentDirection.IsVertical()) entity.Y = Y - HangGap - eOffsetY;
				if (!walker.CurrentDirection.IsHorizontal()) entity.X = X - eOffsetX;
				walker.TargetPosition = new Int2((X + 1).ToUnifyGlobal(), (Y + 1).ToUnifyGlobal());
				walker.WalkStartFrame = Game.GlobalFrame;
			}

			// Walking
			walker.LastWalkingFrame = Game.GlobalFrame;
			entity.X += eOffsetX;
			entity.Y += HangGap + eOffsetY;
			int speed = Speed * walker.TrackWalkSpeedRate / 1000;
			var newPos = IRouteWalker.GetNextRoutePosition(
				walker,
				TypeID,
				Burden > 1 && SlowDownWhenWalkerOverlap ? speed.MoveTowards(
					0, (entity.InstanceID.GetHashCode().Abs() % Burden) + 1
				) : speed,
				allowTurnBack: TurnBackWhenReachEnd,
				pathType: BlockType.Entity,
				allowTilt: AllowTiltConnection && AllowStraightConnection,
				TrackSet
			);
			if (entity is Rigidbody rig) {
				rig.RequireDodgeOverlap = true;
				rig.FillAsTrigger(1);
				rig.IgnoreGravity.True(1);
				rig.IgnorePhysics.True(1);
				rig.VelocityX = newPos.x - eOffsetX - entity.X;
				rig.VelocityY = newPos.y - HangGap - eOffsetY - entity.Y;
			}
			entity.X = newPos.x - eOffsetX;
			entity.Y = newPos.y - HangGap - eOffsetY;

			// Callback
			OnWalking(walker);

		}
		Burden = burden;

	}

	public override void LateUpdate () {

		base.LateUpdate();

		if (!Renderer.TryGetSprite(BodySprite, out var bodySP)) return;
		if (!Renderer.TryGetSprite(BodyTiltSprite, out var bodyTilt)) return;

		int centerX = X + Width / 2;
		int centerY = Y + Height / 2;

		// Line
		const int SIZE = Const.HALF + 2;
		const int SIZE_TILT = Const.HALF * 141422 / 100000 + 2;
		for (int i = 0; i < 8; i++) {
			var dir = (Direction8)i;
			if (!IsConnected(dir)) continue;
			bool positive = dir.IsPositive();
			int rot = dir.GetRotation();
			bool tilt = dir.IsTilted();
			int size = tilt ? SIZE_TILT : SIZE;
			Renderer.Draw(
				tilt ? bodyTilt : bodySP,
				centerX, centerY,
				500, positive ? 0 : 1000,
				positive ? rot : rot + 180,
				size, size
			);
		}

		// Center
		Renderer.Draw(CenterSprite, centerX, centerY, 500, 500, 0, Const.HALF, Const.HALF);

	}

	protected virtual void OnWalking (IAutoTrackWalker walker) { }

	// API
	public bool IsConnected (Direction8 direction) => HasTrackArr[(int)direction];

}
