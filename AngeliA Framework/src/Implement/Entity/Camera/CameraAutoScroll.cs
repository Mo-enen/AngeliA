using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.Capacity(16)]
[EntityAttribute.MapEditorGroup("System")]
[EntityAttribute.UpdateOutOfRange]
public sealed class CameraAutoScroll : Entity {




	#region --- VAR ---


	// Const
	private const int PLAYER_OUT_RANGE_GAP = Const.CEL * 3;
	private static readonly int PATH_ID = typeof(CameraAutoDirection).AngeHash();

	// Data
	private static CameraAutoScroll Current = null;
	private Int2 UpdatedPlayerPos;
	private Int2 StartVelocity;
	private Int2 TargetUnitPos;
	private Direction8 StartDirection;
	private Direction8 MovingDirection;
	private int Speed = 24;
	private int MoveStartFrame;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		MovingDirection = Direction8.Right;
		var startDir = GetDirection(X, Y, MovingDirection, ignoreOpposite: false);
		if (!startDir.HasValue) {
			Active = false;
			return;
		}
		StartDirection = startDir.Value;
		StartVelocity = StartDirection.Normal();
		UpdatedPlayerPos = PlayerSystem.Selecting != null ? new(PlayerSystem.Selecting.X, PlayerSystem.Selecting.Y) : default;
		var squad = WorldSquad.Front as IBlockSquad;
		Speed =
			squad.ReadSystemNumber(X.ToUnit(), Y.ToUnit() + 1, Stage.ViewZ, Direction4.Right, out int speed) ? speed :
			squad.ReadSystemNumber(X.ToUnit(), Y.ToUnit() - 1, Stage.ViewZ, Direction4.Right, out speed) ? speed : 24;
		Speed = Speed.Clamp(1, Const.CEL);
	}


	public override void OnInactivated () {
		base.OnInactivated();
		if (Current == this) Current = null;
	}


	public override void LateUpdate () {
		base.LateUpdate();
#if DEBUG
		Renderer.Draw(TypeID, Rect, Current == this ? Color32.GREEN : Color32.WHITE);
#endif
		if (Current != null && !Current.Active) Current = null;

		// Update
		if (Current == this) {
			Update_Following();
		} else if (Current == null) {
			Update_Idle();
		}

		// Cache
		var player = PlayerSystem.Selecting;
		if (player != null) {
			UpdatedPlayerPos.x = player.X;
			UpdatedPlayerPos.y = player.Y;
		}
	}


	private void Update_Idle () {

		if (Game.GlobalFrame < SpawnFrame + 2) return;
		var player = PlayerSystem.Selecting;
		if (player == null) return;

		if (StartVelocity.x != 0) {
			// Check for X
			int centerX = X + Width / 2;
			if (
				(UpdatedPlayerPos.x - centerX).Sign() != StartVelocity.x.Sign() &&
				(player.X - centerX).Sign() == StartVelocity.x.Sign()
			) {
				goto _TRIGGER_;
			}
		} else {
			// Check for Y
			int centerY = Y + Height / 2;
			if (
				(UpdatedPlayerPos.y - centerY).Sign() != StartVelocity.y.Sign() &&
				(player.Y - centerY).Sign() == StartVelocity.y.Sign()
			) {
				goto _TRIGGER_;
			}
		}

		return;
		_TRIGGER_:;
		Current = this;
		MovingDirection = StartDirection;
		var normal = MovingDirection.Normal();
		TargetUnitPos.x = (X + Const.HALF).ToUnit() + normal.x;
		TargetUnitPos.y = (Y + Const.HALF).ToUnit() + normal.y;
		MoveStartFrame = Game.GlobalFrame;
	}


	private void Update_Following () {
		var targetPos = TargetUnitPos.ToGlobal();
		int dis = Util.DistanceInt(new Int2(X, Y), targetPos);
		var normal = MovingDirection.Normal();
		if (dis > Speed) {
			// Just Move
			int speed = normal.x == 0 || normal.y == 0 ? Speed : (Speed * 10000 / 14142).GreaterOrEquel(1);
			X += normal.x * speed;
			Y += normal.y * speed;
		} else {
			// Reached
			X = targetPos.x;
			Y = targetPos.y;
			// To Next
			var nextDir = GetDirection(X, Y, MovingDirection, ignoreOpposite: true);
			if (!nextDir.HasValue) {
				// Stop
				Active = false;
				Current = null;
				return;
			}
			// Move Remaining
			MovingDirection = nextDir.Value;
			normal = MovingDirection.Normal();
			TargetUnitPos.x = (X + Const.HALF).ToUnit() + normal.x;
			TargetUnitPos.y = (Y + Const.HALF).ToUnit() + normal.y;
			int remaining = Speed - dis;
			int speed = normal.x == 0 || normal.y == 0 ? remaining : (remaining * 10000 / 14142).GreaterOrEquel(1);
			X += normal.x * speed;
			Y += normal.y * speed;
		}
		// Move Camera
		Stage.SetViewPositionDelay(
			X - Stage.ViewRect.width / 2,
			Y - Stage.ViewRect.height / 2,
			((Game.GlobalFrame - MoveStartFrame) * 6).LessOrEquel(1000),
			0
		);
		// Player Interaction
		var player = PlayerSystem.Selecting;
		var viewRect = Stage.ViewRect;
		if (player != null) {
			if (player.Y < viewRect.y - PLAYER_OUT_RANGE_GAP) {
				// Player Passout Outside Camera
				Active = false;
				Current = null;
				player.Health.HP = 0;
				player.SetCharacterState(CharacterState.PassOut);
			} else {
				var cameraRect = Renderer.CameraRect;
				player.X = player.X.Clamp(cameraRect.xMin, cameraRect.xMax);
			}
		}

	}


	#endregion




	#region --- LGC ---


	private static Direction8? GetDirection (int x, int y, Direction8 movingDir, bool ignoreOpposite) {

		x = (x + Const.HALF).ToUnit();
		y = (y + Const.HALF).ToUnit();

		// 0
		var movingNormal = movingDir.Normal();
		int id = WorldSquad.Front.GetBlockAt(x + movingNormal.x, y + movingNormal.y, BlockType.Element);
		if (id == PATH_ID) return movingDir;

		// 45
		var dirA = movingDir.AntiClockwise();
		var dirB = movingDir.Clockwise();
		var normalA = dirA.Normal();
		var normalB = dirB.Normal();
		id = WorldSquad.Front.GetBlockAt(x + normalA.x, y + normalA.y, BlockType.Element);
		if (id == PATH_ID) return dirA;
		id = WorldSquad.Front.GetBlockAt(x + normalB.x, y + normalB.y, BlockType.Element);
		if (id == PATH_ID) return dirB;

		// 90
		dirA = dirA.AntiClockwise();
		dirB = dirB.Clockwise();
		normalA = dirA.Normal();
		normalB = dirB.Normal();
		id = WorldSquad.Front.GetBlockAt(x + normalA.x, y + normalA.y, BlockType.Element);
		if (id == PATH_ID) return dirA;
		id = WorldSquad.Front.GetBlockAt(x + normalB.x, y + normalB.y, BlockType.Element);
		if (id == PATH_ID) return dirB;

		// 135
		dirA = dirA.AntiClockwise();
		dirB = dirB.Clockwise();
		normalA = dirA.Normal();
		normalB = dirB.Normal();
		id = WorldSquad.Front.GetBlockAt(x + normalA.x, y + normalA.y, BlockType.Element);
		if (id == PATH_ID) return dirA;
		id = WorldSquad.Front.GetBlockAt(x + normalB.x, y + normalB.y, BlockType.Element);
		if (id == PATH_ID) return dirB;

		// 180
		if (!ignoreOpposite) {
			id = WorldSquad.Front.GetBlockAt(x - movingNormal.x, y - movingNormal.y, BlockType.Element);
			if (id == PATH_ID) return movingDir.Opposite();
		}

		// None
		return null;
	}


	#endregion




}