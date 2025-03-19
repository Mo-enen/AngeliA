using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.MapEditorGroup("Entity")]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class CarryingPipe : Entity, IBlockEntity {

	// VAR
	protected abstract SpriteCode EdgeSprite { get; }
	protected abstract SpriteCode MidSprite { get; }
	protected abstract SpriteCode BottomSprite { get; }
	protected abstract SpriteCode InsertSprite { get; }
	protected abstract Direction4 Direction { get; }
	protected virtual int CarryingPoseAnimationID => PoseAnimation_SquatIdle.TYPE_ID;

	private static readonly Dictionary<int, Direction4> PipePool = [];
	private static int LastPlayerInputFrame = int.MinValue;
	private int LastPlayerInsideFrame = int.MinValue;
	private Direction5? NeighborPipeDirL = null;
	private Direction5? NeighborPipeDirR = null;
	private Direction5? NeighborPipeDirD = null;
	private Direction5? NeighborPipeDirU = null;

	// MSG
	[OnGameUpdate]
	internal static void OnGameUpdate () {
		foreach (var type in typeof(CarryingPipe).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not CarryingPipe pipe) continue;
			PipePool.TryAdd(type.AngeHash(), pipe.Direction);
		}
		PipePool.TrimExcess();
	}

	public override void OnActivated () {
		base.OnActivated();
		LastPlayerInsideFrame = int.MinValue;
		(this as IBlockEntity).OnEntityRefresh();
	}

	void IBlockEntity.OnEntityRefresh () {
		NeighborPipeDirL = null;
		NeighborPipeDirR = null;
		NeighborPipeDirD = null;
		NeighborPipeDirU = null;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
	}

	public override void Update () {
		base.Update();
		Update_Cache();
		Update_PlayerEnter();
		Update_PlayerInside();
	}

	private void Update_Cache () {
		if (!NeighborPipeDirL.HasValue) {
			int leftID = WorldSquad.Front.GetBlockAt((X + 1).ToUnit() - 1, (Y + 1).ToUnit(), BlockType.Entity);
			if (PipePool.TryGetValue(leftID, out var leftDir)) {
				NeighborPipeDirL = leftDir.ToDirection5();
			} else {
				var pipe = Physics.GetEntity<CarryingPipe>(IRect.Point(X - Width / 2, Y + Height / 2), PhysicsMask.ENVIRONMENT, this);
				NeighborPipeDirL = pipe != null ? pipe.Direction.ToDirection5() : Direction5.Center;
			}
		}
		if (!NeighborPipeDirR.HasValue) {
			int rightID = WorldSquad.Front.GetBlockAt((X + 1).ToUnit() + 1, (Y + 1).ToUnit(), BlockType.Entity);
			if (PipePool.TryGetValue(rightID, out var rightDir)) {
				NeighborPipeDirR = rightDir.ToDirection5();
			} else {
				var pipe = Physics.GetEntity<CarryingPipe>(IRect.Point(X + Width + Width / 2, Y + Height / 2), PhysicsMask.ENVIRONMENT, this);
				NeighborPipeDirR = pipe != null ? pipe.Direction.ToDirection5() : Direction5.Center;
			}
		}
		if (!NeighborPipeDirD.HasValue) {
			int downID = WorldSquad.Front.GetBlockAt((X + 1).ToUnit(), (Y + 1).ToUnit() - 1, BlockType.Entity);
			if (PipePool.TryGetValue(downID, out var downDir)) {
				NeighborPipeDirD = downDir.ToDirection5();
			} else {
				var pipe = Physics.GetEntity<CarryingPipe>(IRect.Point(X + Width / 2, Y - Height / 2), PhysicsMask.ENVIRONMENT, this);
				NeighborPipeDirD = pipe != null ? pipe.Direction.ToDirection5() : Direction5.Center;
			}
		}
		if (!NeighborPipeDirU.HasValue) {
			int upID = WorldSquad.Front.GetBlockAt((X + 1).ToUnit(), (Y + 1).ToUnit() + 1, BlockType.Entity);
			if (PipePool.TryGetValue(upID, out var upDir)) {
				NeighborPipeDirU = upDir.ToDirection5();
			} else {
				var pipe = Physics.GetEntity<CarryingPipe>(IRect.Point(X + Width / 2, Y + Height + Height / 2), PhysicsMask.ENVIRONMENT, this);
				NeighborPipeDirU = pipe != null ? pipe.Direction.ToDirection5() : Direction5.Center;
			}
		}
	}

	private void Update_PlayerEnter () {

		if (Game.GlobalFrame <= LastPlayerInputFrame + 20) return;

		var player = PlayerSystem.Selecting;
		if (player == null || !player.Active) return;

		// Overlap Check
		if (!Rect.EdgeOutside(
			Direction, Direction == Direction4.Down ? Const.HALF : 1).Overlaps(player.Rect)
		) return;

		// Center Check
		switch (Direction) {
			case Direction4.Left:
			case Direction4.Right:
				if (!player.Rect.CenterY().InRangeInclude(Y, Y + Height)) return;
				break;
			case Direction4.Down:
			case Direction4.Up:
				if (!player.Rect.CenterX().InRangeInclude(X, X + Width)) return;
				break;
		}

		// Input Check
		switch (Direction) {
			case Direction4.Left:
				if (!NeighborPipeDirL.HasValue || NeighborPipeDirL.Value != Direction5.Center || player.Movement.IntendedX <= 0) return;
				break;
			case Direction4.Right:
				if (!NeighborPipeDirR.HasValue || NeighborPipeDirR.Value != Direction5.Center || player.Movement.IntendedX >= 0) return;
				break;
			case Direction4.Down:
				if (!NeighborPipeDirD.HasValue || NeighborPipeDirD.Value != Direction5.Center || player.Movement.IntendedY <= 0) return;
				break;
			case Direction4.Up:
				if (!NeighborPipeDirU.HasValue || NeighborPipeDirU.Value != Direction5.Center || player.Movement.IntendedY >= 0) return;
				break;
		}

		// Enter
		CenterPlayer(player);
		OnPlayerEnter(player);
		player.IgnorePhysics.True(1, 4096);
		player.IgnoreGravity.True(1, 4096);
		player.IgnoreInsideGround.True(1, 4096);
		PlayerSystem.IgnoreInput(1);
		PlayerSystem.IgnoreAction(1);
		LastPlayerInputFrame = Game.GlobalFrame;
	}

	private void Update_PlayerInside () {

		var player = PlayerSystem.Selecting;
		if (player == null || !player.Active) {
			return;
		}

		if (!Rect.Contains(player.Rect.CenterInt())) return;

		// For Current Pipe
		LastPlayerInsideFrame = Game.GlobalFrame;
		player.IgnorePhysics.True(1, 4096);
		player.IgnoreGravity.True(1, 4096);
		player.IgnoreInsideGround.True(1, 4096);
		player.Movement.LockSquat(1);
		player.MakeGrounded(6, TypeID);
		PlayerSystem.IgnoreInput(1);
		PlayerSystem.IgnoreAction(1);
		CenterPlayer(player);

		// Rendering
		if (player.Rendering is PoseCharacterRenderer pRen) {
			pRen.ManualPoseAnimate(CarryingPoseAnimationID, 2);
			pRen.Tint.Override(Color32.BLACK, 1);
			pRen.Scale.Override(618, 1);
		}

		// Player Input
		if (Game.GlobalFrame > LastPlayerInputFrame + 8) {
			if (Input.GameKeyHolding(Gamekey.Left) && IsValidCarryDirection(Direction4.Left, out bool exit)) {
				player.X -= Const.CEL;
				LastPlayerInputFrame = Game.GlobalFrame;
				player.Bounce();
				if (exit) OnPlayerExit(player);
			}
			if (Input.GameKeyHolding(Gamekey.Right) && IsValidCarryDirection(Direction4.Right, out exit)) {
				player.X += Const.CEL;
				LastPlayerInputFrame = Game.GlobalFrame;
				player.Bounce();
				if (exit) OnPlayerExit(player);
			}
			if (Input.GameKeyHolding(Gamekey.Down) && IsValidCarryDirection(Direction4.Down, out exit)) {
				player.Y -= Const.CEL;
				LastPlayerInputFrame = Game.GlobalFrame;
				player.Bounce();
				if (exit) OnPlayerExit(player);
			}
			if (Input.GameKeyHolding(Gamekey.Up) && IsValidCarryDirection(Direction4.Up, out exit)) {
				player.Y += Const.CEL;
				LastPlayerInputFrame = Game.GlobalFrame;
				player.Bounce();
				if (exit) OnPlayerExit(player);
			}
		}
		ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, BuiltInText.HINT_MOVE);
		ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, BuiltInText.HINT_MOVE);

	}

	public override void LateUpdate () {
		base.LateUpdate();
		DrawPipe();
		DrawGizmos();
	}

	private void DrawPipe () {
		switch (Direction) {
			case Direction4.Left:
				// Body
				Renderer.Draw(
					!NeighborPipeDirL.HasValue || NeighborPipeDirL.Value == Direction5.Center || NeighborPipeDirL.Value == Direction5.Right ? EdgeSprite :
					!NeighborPipeDirR.HasValue || NeighborPipeDirR.Value == Direction5.Center ? BottomSprite : MidSprite,
					X + Width / 2, Y + Height / 2, 500, 500, 90, Width, -Height
				);
				// Insert
				if (NeighborPipeDirL.HasValue && NeighborPipeDirL.Value.IsVertical()) {
					Renderer.Draw(InsertSprite, X, Y + Height / 2, 500, 0, 90, Width, Const.ORIGINAL_SIZE_NEGATAVE);
				}
				if (NeighborPipeDirR.HasValue && NeighborPipeDirR.Value.IsVertical()) {
					Renderer.Draw(InsertSprite, X + Width, Y + Height / 2, 500, 0, 90, Width, Const.ORIGINAL_SIZE);
				}
				break;
			case Direction4.Right:
				// Body
				Renderer.Draw(
					!NeighborPipeDirR.HasValue || NeighborPipeDirR.Value == Direction5.Center || NeighborPipeDirR.Value == Direction5.Left ? EdgeSprite :
					!NeighborPipeDirL.HasValue || NeighborPipeDirL.Value == Direction5.Center ? BottomSprite : MidSprite,
					X + Width / 2, Y + Height / 2, 500, 500, 90, Width, Height
				);
				// Insert
				if (NeighborPipeDirL.HasValue && NeighborPipeDirL.Value.IsVertical()) {
					Renderer.Draw(InsertSprite, X, Y + Height / 2, 500, 0, 90, Width, Const.ORIGINAL_SIZE_NEGATAVE);
				}
				if (NeighborPipeDirR.HasValue && NeighborPipeDirR.Value.IsVertical()) {
					Renderer.Draw(InsertSprite, X + Width, Y + Height / 2, 500, 0, 90, Width, Const.ORIGINAL_SIZE);
				}
				break;
			case Direction4.Down:
				// Body
				Renderer.Draw(
					!NeighborPipeDirD.HasValue || NeighborPipeDirD.Value == Direction5.Center || NeighborPipeDirD.Value == Direction5.Up ? EdgeSprite :
					!NeighborPipeDirU.HasValue || NeighborPipeDirU.Value == Direction5.Center ? BottomSprite : MidSprite,
					X + Width / 2, Y + Height / 2, 500, 500, 0, Width, -Height
				);
				// Insert
				if (NeighborPipeDirD.HasValue && NeighborPipeDirD.Value.IsHorizontal()) {
					Renderer.Draw(InsertSprite, X + Width / 2, Y, 500, 0, 0, -Width, Const.ORIGINAL_SIZE_NEGATAVE);
				}
				if (NeighborPipeDirU.HasValue && NeighborPipeDirU.Value.IsHorizontal()) {
					Renderer.Draw(InsertSprite, X + Width / 2, Y + Height, 500, 0, 0, -Width, Const.ORIGINAL_SIZE);
				}
				break;
			case Direction4.Up:
				// Body
				Renderer.Draw(
					!NeighborPipeDirU.HasValue || NeighborPipeDirU.Value == Direction5.Center || NeighborPipeDirU.Value == Direction5.Down ? EdgeSprite :
					!NeighborPipeDirD.HasValue || NeighborPipeDirD.Value == Direction5.Center ? BottomSprite : MidSprite,
					X + Width / 2, Y + Height / 2, 500, 500, 0, Width, Height
				);
				// Insert
				if (NeighborPipeDirD.HasValue && NeighborPipeDirD.Value.IsHorizontal()) {
					Renderer.Draw(InsertSprite, X + Width / 2, Y, 500, 0, 0, -Width, Const.ORIGINAL_SIZE_NEGATAVE);
				}
				if (NeighborPipeDirU.HasValue && NeighborPipeDirU.Value.IsHorizontal()) {
					Renderer.Draw(InsertSprite, X + Width / 2, Y + Height, 500, 0, 0, -Width, Const.ORIGINAL_SIZE);
				}
				break;
		}
	}

	private void DrawGizmos () {

		if (LastPlayerInsideFrame != Game.GlobalFrame) return;

		using var _ = new UILayerScope();

		// Frame
		Renderer.DrawSlice(BuiltInSprite.FRAME_HOLLOW_16, Rect.Expand(Game.GlobalFrame.PingPong(24) - 12));

		// Arrow
		int offset = Game.GlobalFrame.PingPong(24);
		if (IsValidCarryDirection(Direction4.Left, out bool exit)) {
			Renderer.Draw(
				BuiltInSprite.LEFT_ARROW,
				Rect.Shift(-Const.CEL - offset, 0).Shrink(Const.QUARTER / 2),
				exit ? Color32.GREEN : Color32.WHITE
			);
		}
		if (IsValidCarryDirection(Direction4.Right, out exit)) {
			Renderer.Draw(
				BuiltInSprite.RIGHT_ARROW,
				Rect.Shift(Const.CEL + offset, 0).Shrink(Const.QUARTER / 2),
				exit ? Color32.GREEN : Color32.WHITE
			);
		}
		if (IsValidCarryDirection(Direction4.Down, out exit)) {
			Renderer.Draw(
				BuiltInSprite.DOWN_ARROW,
				Rect.Shift(0, -Const.CEL - offset).Shrink(Const.QUARTER / 2),
				exit ? Color32.GREEN : Color32.WHITE
			);
		}
		if (IsValidCarryDirection(Direction4.Up, out exit)) {
			Renderer.Draw(
				BuiltInSprite.UP_ARROW,
				Rect.Shift(0, Const.CEL + offset).Shrink(Const.QUARTER / 2),
				exit ? Color32.GREEN : Color32.WHITE
			);
		}

	}

	// API
	public bool IsEdge (bool requireOpenSpace) => Direction switch {
		Direction4.Left => !NeighborPipeDirL.HasValue || NeighborPipeDirL.Value == Direction5.Center || (!requireOpenSpace && NeighborPipeDirL.Value == Direction5.Right),
		Direction4.Right => !NeighborPipeDirR.HasValue || NeighborPipeDirR.Value == Direction5.Center || (!requireOpenSpace && NeighborPipeDirR.Value == Direction5.Left),
		Direction4.Down => !NeighborPipeDirD.HasValue || NeighborPipeDirD.Value == Direction5.Center || (!requireOpenSpace && NeighborPipeDirD.Value == Direction5.Up),
		Direction4.Up => !NeighborPipeDirU.HasValue || NeighborPipeDirU.Value == Direction5.Center || (!requireOpenSpace && NeighborPipeDirU.Value == Direction5.Down),
		_ => false,
	};

	protected virtual void OnPlayerEnter (Character player) { }
	protected virtual void OnPlayerExit (Character player) { }

	// LGC
	private void CenterPlayer (Entity player) {
		player.X = X + Width / 2;
		player.Y = Y + Height / 2 - player.Height / 2;
	}

	private bool IsValidCarryDirection (Direction4 moveDir, out bool exit) {

		exit = false;

		// Blocked Check
		var hits = Physics.OverlapAll(PhysicsMask.MAP, IRect.Point(Rect.CenterInt() + moveDir.Normal() * Const.CEL), out int count, this);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is CarryingPipe) continue;
			return false;
		}

		// Same Dir
		if (moveDir == Direction) {
			exit = Direction switch {
				Direction4.Up => !NeighborPipeDirU.HasValue || NeighborPipeDirU.Value == Direction5.Center,
				Direction4.Down => !NeighborPipeDirD.HasValue || NeighborPipeDirD.Value == Direction5.Center,
				Direction4.Left => !NeighborPipeDirL.HasValue || NeighborPipeDirL.Value == Direction5.Center,
				Direction4.Right => !NeighborPipeDirR.HasValue || NeighborPipeDirR.Value == Direction5.Center,
				_ => false,
			};
			return true;
		}

		// Opposite Dir
		if (moveDir == Direction.Opposite()) {
			switch (Direction) {
				case Direction4.Up:
					return NeighborPipeDirD.HasValue && NeighborPipeDirD.Value != Direction5.Center;
				case Direction4.Down:
					return NeighborPipeDirU.HasValue && NeighborPipeDirU.Value != Direction5.Center;
				case Direction4.Left:
					return NeighborPipeDirR.HasValue && NeighborPipeDirR.Value != Direction5.Center;
				case Direction4.Right:
					return NeighborPipeDirL.HasValue && NeighborPipeDirL.Value != Direction5.Center;
			}
		}

		// Side
		switch (Direction) {
			case Direction4.Down:
			case Direction4.Up:
				if (moveDir == Direction4.Left) {
					return NeighborPipeDirL.HasValue && NeighborPipeDirL.Value.IsHorizontal();
				} else {
					return NeighborPipeDirR.HasValue && NeighborPipeDirR.Value.IsHorizontal();
				}
			case Direction4.Left:
			case Direction4.Right:
				if (moveDir == Direction4.Down) {
					return NeighborPipeDirD.HasValue && NeighborPipeDirD.Value.IsVertical();
				} else {
					return NeighborPipeDirU.HasValue && NeighborPipeDirU.Value.IsVertical();
				}
		}

		return false;
	}

}
