using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.MapEditorGroup("Contraption")]
public sealed class Track : Entity, IBlockEntity {




	#region --- VAR ---


	// Const
	public static readonly int TYPE_ID = typeof(Track).AngeHash();
	private static readonly SpriteCode BODY_SP = "Track.Body";
	private static readonly SpriteCode BODY_TILT = "Track.Tilt";
	private static readonly SpriteCode BODY_CENTER = "Track.Center";
	private static readonly SpriteCode BODY_HOOK = "Track.Hook";
	private static readonly SpriteCode BODY_GEAR = "Track.Gear";

	// Data
	private readonly bool[] HasTrackArr = [false, false, false, false, false, false, false, false, false];
	private Int4 Shrink;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		OnEntityRefresh();
	}


	public void OnEntityRefresh () {

		var squad = WorldSquad.Front;
		int unitX = (X + 1).ToUnit();
		int unitY = (Y + 1).ToUnit();

		// 7 0 1
		// 6   2
		// 5 4 3

		HasTrackArr[0] = squad.GetBlockAt(unitX, unitY + 1, BlockType.Entity) == TYPE_ID;
		HasTrackArr[2] = squad.GetBlockAt(unitX + 1, unitY, BlockType.Entity) == TYPE_ID;
		HasTrackArr[4] = squad.GetBlockAt(unitX, unitY - 1, BlockType.Entity) == TYPE_ID;
		HasTrackArr[6] = squad.GetBlockAt(unitX - 1, unitY, BlockType.Entity) == TYPE_ID;

		HasTrackArr[1] = squad.GetBlockAt(unitX + 1, unitY + 1, BlockType.Entity) == TYPE_ID;
		HasTrackArr[3] = squad.GetBlockAt(unitX + 1, unitY - 1, BlockType.Entity) == TYPE_ID;
		HasTrackArr[5] = squad.GetBlockAt(unitX - 1, unitY - 1, BlockType.Entity) == TYPE_ID;
		HasTrackArr[7] = squad.GetBlockAt(unitX - 1, unitY + 1, BlockType.Entity) == TYPE_ID;

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
		const int HANG_GAP = 64;
		var hits = Physics.OverlapAll(PhysicsMask.DYNAMIC, Rect.Shift(0, -HANG_GAP).Shrink(Shrink), out int count, this, OperationMode.ColliderAndTrigger);
		for (int i = 0; i < count; i++) {

			var hit = hits[i];
			var entity = hit.Entity;

			if (entity is not IAutoTrackWalker walker) continue;

			// Walked Check
			if (Game.GlobalFrame <= walker.LastWalkingFrame) continue;

			int eOffsetX = 0;
			int eOffsetY = 0;
			if (entity is Rigidbody offsetRig) {
				eOffsetX = offsetRig.OffsetX;
				eOffsetY = offsetRig.OffsetY;
			}

			// Start Walk
			if (Game.GlobalFrame <= entity.SpawnFrame + 1 || Game.GlobalFrame > walker.LastWalkingFrame + 1) {
				if (entity.Y + eOffsetY > Y - HANG_GAP) continue;
				if (entity is Rigidbody _rig) {
					if (_rig.DeltaPositionY > 0) continue;
					walker.CurrentDirection = Util.GetDirection(_rig.DeltaPositionX, _rig.DeltaPositionY);
				} else {
					walker.CurrentDirection =
						HasTrackArr[1] || HasTrackArr[2] || HasTrackArr[3] || HasTrackArr[5] || HasTrackArr[6] || HasTrackArr[7] ?
						Direction8.Right : Direction8.Top;
				}
				if (!walker.CurrentDirection.IsVertical()) entity.Y = Y - HANG_GAP - eOffsetY;
				if (!walker.CurrentDirection.IsHorizontal()) entity.X = X - eOffsetX;
				walker.TargetPosition = new Int2((X + 1).ToUnifyGlobal(), (Y + 1).ToUnifyGlobal());
				walker.WalkStartFrame = Game.GlobalFrame;
			}

			// Walking
			walker.LastWalkingFrame = Game.GlobalFrame;
			entity.X += eOffsetX;
			entity.Y += HANG_GAP + eOffsetY;
			var newPos = IRouteWalker.GetNextRoutePosition(
				walker,
				TYPE_ID,
				walker.TrackWalkSpeed,
				allowTurnBack: true,
				pathType: BlockType.Entity
			);
			if (entity is Rigidbody rig) {
				rig.RequireDodgeOverlap = true;
				rig.FillAsTrigger(1);
				rig.IgnoreGravity.True(1);
				rig.IgnorePhysics.True(1);
				rig.VelocityX = 0;
				rig.VelocityY = 0;
			}
			entity.X = newPos.x - eOffsetX;
			entity.Y = newPos.y - HANG_GAP - eOffsetY;

			// Draw Hook
			if (Renderer.TryGetSprite(BODY_HOOK, out var hookSP)) {
				var eRect = entity.Rect;
				Renderer.Draw(
					hookSP,
					eRect.CenterX(), eRect.CenterY() + HANG_GAP,
					hookSP.PivotX, hookSP.PivotY, 0,
					HANG_GAP, HANG_GAP
				);
			}

			// Draw Gear
			if (Renderer.TryGetSprite(BODY_GEAR, out var gearSP)) {
				var eRect = entity.Rect;
				Renderer.Draw(
					gearSP, eRect.CenterX(), eRect.CenterY() + HANG_GAP + 32,
					500, 500, Game.GlobalFrame * (walker.CurrentDirection.IsPositive() ? 4 : -4),
					86, 86
				);
			}

		}

	}


	public override void LateUpdate () {
		base.LateUpdate();

		if (!Renderer.TryGetSprite(BODY_SP, out var bodySP)) return;
		if (!Renderer.TryGetSprite(BODY_TILT, out var bodyTilt)) return;

		int centerX = X + Width / 2;
		int centerY = Y + Height / 2;

		// Line
		const int SIZE = Const.HALF + 2;
		const int SIZE_TILT = Const.HALF * 141422 / 100000 + 2;
		for (int i = 0; i < 8; i++) {
			if (!HasTrackArr[i]) continue;
			var dir = (Direction8)i;
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
		Renderer.Draw(BODY_CENTER, centerX, centerY, 500, 500, 0, Const.HALF, Const.HALF);

	}


	#endregion




	#region --- API ---



	#endregion




	#region --- LGC ---



	#endregion




}
