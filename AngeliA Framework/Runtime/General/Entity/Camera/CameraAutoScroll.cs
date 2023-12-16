using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	// x
	public class CameraAutoScrollStop : CameraAutoScroll {
		public override Direction3 DirectionX => Direction3.None;
		public override Direction3 DirectionY => Direction3.None;
	}


	// ←
	public class CameraAutoScrollLeft : CameraAutoScroll {
		public override Direction3 DirectionX => Direction3.Left;
		public override Direction3 DirectionY => Direction3.None;
	}

	// →
	public class CameraAutoScrollRight : CameraAutoScroll {
		public override Direction3 DirectionX => Direction3.Right;
		public override Direction3 DirectionY => Direction3.None;
	}

	// ↓
	public class CameraAutoScrollBottom : CameraAutoScroll {
		public override Direction3 DirectionX => Direction3.None;
		public override Direction3 DirectionY => Direction3.Down;
	}

	// ↑
	public class CameraAutoScrollTop : CameraAutoScroll {
		public override Direction3 DirectionX => Direction3.None;
		public override Direction3 DirectionY => Direction3.Up;
	}

	// ↙
	public class CameraAutoScrollBottomLeft : CameraAutoScroll {
		public override Direction3 DirectionX => Direction3.Left;
		public override Direction3 DirectionY => Direction3.Down;
	}

	// ↘
	public class CameraAutoScrollBottomRight : CameraAutoScroll {
		public override Direction3 DirectionX => Direction3.Right;
		public override Direction3 DirectionY => Direction3.Down;
	}

	// ↖
	public class CameraAutoScrollTopLeft : CameraAutoScroll {
		public override Direction3 DirectionX => Direction3.Left;
		public override Direction3 DirectionY => Direction3.Up;
	}

	// ↗
	public class CameraAutoScrollTopRight : CameraAutoScroll {
		public override Direction3 DirectionX => Direction3.Right;
		public override Direction3 DirectionY => Direction3.Up;
	}


	[EntityAttribute.Capacity(16)]
	[EntityAttribute.MapEditorGroup("System")]
	[EntityAttribute.UpdateOutOfRange]
	public abstract class CameraAutoScroll : Entity {




		#region --- VAR ---


		// Const
		public const int MAX_LEN = 64;

		// Api
		public static readonly Dictionary<int, Int3> DirectionPool = new() {
			{ typeof(CameraAutoScrollStop).AngeHash(), new Int3(0, 0, 0) },
			{ typeof(CameraAutoScrollLeft).AngeHash(), new Int3(-1, 0, -90) },
			{ typeof(CameraAutoScrollRight).AngeHash(), new Int3(1, 0, 90) },
			{ typeof(CameraAutoScrollBottom).AngeHash(), new Int3(0, -1, 180) },
			{ typeof(CameraAutoScrollTop).AngeHash(), new Int3(0, 1, 0) },
			{ typeof(CameraAutoScrollBottomLeft).AngeHash(), new Int3(-1, -1, -135) },
			{ typeof(CameraAutoScrollBottomRight).AngeHash(), new Int3(1, -1, 135) },
			{ typeof(CameraAutoScrollTopLeft).AngeHash(), new Int3(-1, 1, -45) },
			{ typeof(CameraAutoScrollTopRight).AngeHash(), new Int3(1, 1, 45) },
		};
		public abstract Direction3 DirectionX { get; }
		public abstract Direction3 DirectionY { get; }
		public virtual int Speed => 24;

		// Data
		private static CameraAutoScroll Current = null;
		private Int2 MaxPosition = default;
		private bool IsEntrance = true;
		private int PlayerPrevX = 0;
		private int PlayerPrevXUpdateFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			MaxPosition.x = X + (int)DirectionX * MAX_LEN * Const.CEL;
			MaxPosition.y = Y + (int)DirectionY * MAX_LEN * Const.CEL;
			IsEntrance = CheckEntrance(X, Y, DirectionX, DirectionY);
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Player.Selecting == null || !Player.Selecting.Active) {
				Current = null;
				return;
			}
			if (Player.Selecting.CharacterState != CharacterState.GamePlay) {
				Current = null;
				return;
			}
			if (Current != null) {
				FrameUpdate_Scroll();
			} else if (IsEntrance) {
				FrameUpdate_Idle();
			}
		}


		private void FrameUpdate_Idle () {

			var player = Player.Selecting;
			if (player == null || !player.Active) return;
			int thisX = X + Const.HALF;
			int thisY = Y + Const.HALF;
			var cameraRect = CellRenderer.CameraRect;
			int? prevX = null;

			if (Game.GlobalFrame == PlayerPrevXUpdateFrame + 1) {
				prevX = PlayerPrevX;
			}
			PlayerPrevX = player.Rect.x + player.Rect.width / 2;
			PlayerPrevXUpdateFrame = Game.GlobalFrame;

			// Check Camera in Range
			if (!cameraRect.Contains(thisX, thisY)) return;

			// Left to Right
			if (DirectionX != Direction3.Left && prevX.HasValue && prevX < thisX && player.X >= thisX) {
				Current = this;
			}

			// Right to Left
			if (DirectionX != Direction3.Right && prevX.HasValue && prevX > thisX && player.X <= thisX) {
				Current = this;
			}

		}


		private void FrameUpdate_Scroll () {

			if (Current != this) return;

			// End by Max Pos
			if (
				(DirectionX == Direction3.Left && X < MaxPosition.x) ||
				(DirectionX == Direction3.Right && X > MaxPosition.x) ||
				(DirectionY == Direction3.Down && Y < MaxPosition.y) ||
				(DirectionY == Direction3.Up && Y > MaxPosition.y)
			) {
				Current = null;
				Active = false;
				return;
			}

			// End by Hit Other Scroll Entity
			var nextScroll = CellPhysics.GetEntity<CameraAutoScroll>(
				new IRect(X + Const.HALF, Y + Const.HALF, 1, 1),
				PhysicsMask.ENVIRONMENT, this, OperationMode.TriggerOnly
			);
			if (nextScroll != null && nextScroll.Active) {

				// End Scroll Check
				if (
					(nextScroll.DirectionX == Direction3.None && nextScroll.DirectionY == Direction3.None) ||
					(DirectionX == nextScroll.DirectionX.Opposite() && DirectionY == nextScroll.DirectionY.Opposite())
				) {
					Current = null;
					Active = false;
					return;
				}

				// Trigger Check
				bool xTriggered =
					DirectionX == Direction3.None ||
					(DirectionX == Direction3.Left && X < nextScroll.X) ||
					(DirectionX == Direction3.Right && X > nextScroll.X);
				bool yTriggered =
					DirectionY == Direction3.None ||
					(DirectionY == Direction3.Down && Y < nextScroll.Y) ||
					(DirectionY == Direction3.Up && Y > nextScroll.Y);
				if (xTriggered && yTriggered) {
					Active = false;
					Current = nextScroll;
					nextScroll.Move();
					return;
				}
			}

			// Movement
			Move();

			// Clamp or PassOut Player
			const int PASS_OUT_GAP = Const.CEL * 3;
			var player = Player.Selecting;
			var pRect = player.Rect;
			var cameraRect = CellRenderer.CameraRect;
			if (pRect.yMin < cameraRect.yMin - PASS_OUT_GAP) {
				player.SetHealth(0);
			}
			if (pRect.yMax > cameraRect.yMax) {
				player.Y = cameraRect.yMax - pRect.height;
			}
			if (pRect.xMin < cameraRect.xMin) {
				player.X = cameraRect.xMin + player.Width / 2;
			}
			if (pRect.xMax > cameraRect.xMax) {
				player.X = cameraRect.xMax - player.Width / 2;
			}

		}


		#endregion




		#region --- LGC ---


		private void Move () {
			var view = Stage.ViewRect;
			int deltaX = (int)DirectionX * Speed;
			int deltaY = (int)DirectionY * Speed;
			X += deltaX;
			Y += deltaY;
			Stage.SetViewPositionDelay(
				X + Const.HALF - view.width / 2,
				Y + Const.HALF - view.height / 2,
				50, 1
			);
		}


		public static bool CheckEntrance (int globalX, int globalY, Direction3 directionX, Direction3 directionY) {

			if (directionX == Direction3.None && directionY == Direction3.None) return false;
			var unitPos = new Int2(globalX, globalY).ToUnit();
			var squad = WorldSquad.Front;

			var dir = new Int2((int)directionX, (int)directionY);
			if (HasPrevTarget(new(-1, -1))) return false;
			if (HasPrevTarget(new(-1, 0))) return false;
			if (HasPrevTarget(new(-1, 1))) return false;
			if (HasPrevTarget(new(0, -1))) return false;
			if (HasPrevTarget(new(0, 1))) return false;
			if (HasPrevTarget(new(1, -1))) return false;
			if (HasPrevTarget(new(1, 0))) return false;
			if (HasPrevTarget(new(1, 1))) return false;
			return true;

			bool HasPrevTarget (Int2 direction) {
				if (direction == dir) return false;
				for (int i = 1; i < MAX_LEN; i++) {
					int x = unitPos.x + direction.x * i;
					int y = unitPos.y + direction.y * i;
					int id = squad.GetBlockAt(x, y, BlockType.Entity);
					if (
						id != 0 &&
						DirectionPool.TryGetValue(id, out var dir) &&
						direction.x == -dir.x &&
						direction.y == -dir.y
					) return true;
				}
				return false;
			}
		}


		#endregion




	}


	public class CameraAutoScroll_Gizmos : MapEditorGizmos {
		private static readonly int TRI = "Triangle13".AngeHash();
		private static readonly int FRAME = "Frame16".AngeHash();

		public override System.Type TargetEntity => typeof(CameraAutoScroll);
		public override bool DrawGizmosOutOfRange => true;
		public override void DrawGizmos (IRect entityGlobalRect, int entityID) {

			if (!CameraAutoScroll.DirectionPool.TryGetValue(entityID, out var dir) || (Int2)dir == Int2.zero) return;

			// Entrance
			if (CameraAutoScroll.CheckEntrance(
				entityGlobalRect.x, entityGlobalRect.y,
				(Direction3)dir.x, (Direction3)dir.y
			)) {
				CellRenderer.Draw_9Slice(FRAME, entityGlobalRect, Const.GREEN);
			}

			// Draw Line
			int entityUnitX = entityGlobalRect.x.ToUnit();
			int entityUnitY = entityGlobalRect.y.ToUnit();
			var squad = WorldSquad.Front;
			int size = CellRendererGUI.Unify(10);
			for (int i = 0; i < CameraAutoScroll.MAX_LEN; i++) {
				int uX = entityUnitX + dir.x * i;
				int uY = entityUnitY + dir.y * i;
				int id = squad.GetBlockAt(uX, uY, BlockType.Entity);
				if (i != 0 && id != 0 && CameraAutoScroll.DirectionPool.ContainsKey(id)) break;
				int delta = Game.GlobalFrame.UMod(60) * Const.CEL / 60;
				int x = uX.ToGlobal() + Const.HALF + delta * dir.x;
				int y = uY.ToGlobal() + Const.HALF + delta * dir.y;
				CellRenderer.Draw(TRI, x, y, 500, 500, dir.z, size, size);
			}
		}
	}
}