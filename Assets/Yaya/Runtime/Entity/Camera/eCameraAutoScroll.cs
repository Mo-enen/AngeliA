using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	// x
	public class eCameraAutoScrollStop : eCameraAutoScroll {
		public override Direction3 DirectionX => Direction3.None;
		public override Direction3 DirectionY => Direction3.None;
	}


	// ←
	public class eCameraAutoScrollLeft : eCameraAutoScroll {
		public override Direction3 DirectionX => Direction3.Left;
		public override Direction3 DirectionY => Direction3.None;
	}

	// →
	public class eCameraAutoScrollRight : eCameraAutoScroll {
		public override Direction3 DirectionX => Direction3.Right;
		public override Direction3 DirectionY => Direction3.None;
	}

	// ↓
	public class eCameraAutoScrollBottom : eCameraAutoScroll {
		public override Direction3 DirectionX => Direction3.None;
		public override Direction3 DirectionY => Direction3.Down;
	}

	// ↑
	public class eCameraAutoScrollTop : eCameraAutoScroll {
		public override Direction3 DirectionX => Direction3.None;
		public override Direction3 DirectionY => Direction3.Up;
	}

	// ↙
	public class eCameraAutoScrollBottomLeft : eCameraAutoScroll {
		public override Direction3 DirectionX => Direction3.Left;
		public override Direction3 DirectionY => Direction3.Down;
	}

	// ↘
	public class eCameraAutoScrollBottomRight : eCameraAutoScroll {
		public override Direction3 DirectionX => Direction3.Right;
		public override Direction3 DirectionY => Direction3.Down;
	}

	// ↖
	public class eCameraAutoScrollTopLeft : eCameraAutoScroll {
		public override Direction3 DirectionX => Direction3.Left;
		public override Direction3 DirectionY => Direction3.Up;
	}

	// ↗
	public class eCameraAutoScrollTopRight : eCameraAutoScroll {
		public override Direction3 DirectionX => Direction3.Right;
		public override Direction3 DirectionY => Direction3.Up;
	}


	[EntityAttribute.Capacity(16)]
	[EntityAttribute.MapEditorGroup("System")]
	[EntityAttribute.UpdateOutOfRange]
	public abstract class eCameraAutoScroll : Entity {




		#region --- VAR ---


		// Const
		public const int MAX_LEN = 64;

		// Api
		public abstract Direction3 DirectionX { get; }
		public abstract Direction3 DirectionY { get; }
		public virtual int Speed => 24;

		// Data
		private static eCameraAutoScroll Current = null;
		private static readonly HashSet<int> AllCameraScrollID = new();
		private Vector2Int MaxPosition = default;
		private bool IsEntrance = true;
		private int PlayerPrevX = 0;
		private int PlayerPrevXUpdateFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		[AfterGameInitialize]
		public static void Initialize () {
			AllCameraScrollID.Clear();
			foreach (var type in typeof(eCameraAutoScroll).AllChildClass()) {
				var ins = System.Activator.CreateInstance(type) as eCameraAutoScroll;
				if (ins.DirectionX != Direction3.None || ins.DirectionY != Direction3.None) {
					AllCameraScrollID.TryAdd(type.AngeHash());
				}
			}
		}


		public override void OnActived () {
			base.OnActived();
			MaxPosition.x = X + (int)DirectionX * MAX_LEN * Const.CEL;
			MaxPosition.y = Y + (int)DirectionY * MAX_LEN * Const.CEL;
			IsEntrance = CheckEntrance();
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (ePlayer.Selecting == null || !ePlayer.Selecting.Active) {
				Current = null;
				return;
			}
			if (ePlayer.Selecting.CharacterState != CharacterState.GamePlay) {
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

			var player = ePlayer.Selecting;
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
			var nextScroll = CellPhysics.GetEntity<eCameraAutoScroll>(
				new RectInt(X + Const.HALF, Y + Const.HALF, 1, 1),
				Const.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly
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

			// Clamp or Passout Player
			const int PASS_OUT_GAP = Const.CEL * 3;
			var player = ePlayer.Selecting;
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
			var view = Game.Current.ViewRect;
			int deltaX = (int)DirectionX * Speed;
			int deltaY = (int)DirectionY * Speed;
			X += deltaX;
			Y += deltaY;
			//Game.Current.SetViewPositionDelay(view.x + deltaX, view.y + deltaY, 1000, 0);
			Game.Current.SetViewPositionDelay(
				X + Const.HALF - view.width / 2,
				Y + Const.HALF - view.height / 2
				, 50, 1
			);
		}


		private bool CheckEntrance () {

			if (DirectionX == Direction3.None && DirectionY == Direction3.None) return false;
			var unitPos = new Vector2Int(X, Y).ToUnit();
			var squad = Game.Current.WorldSquad;

			var dir = new Vector2Int((int)DirectionX, (int)DirectionY);
			if (HasPrevTarget(new(-1, -1))) return false;
			if (HasPrevTarget(new(-1, 0))) return false;
			if (HasPrevTarget(new(-1, 1))) return false;
			if (HasPrevTarget(new(0, -1))) return false;
			if (HasPrevTarget(new(0, 1))) return false;
			if (HasPrevTarget(new(1, -1))) return false;
			if (HasPrevTarget(new(1, 0))) return false;
			if (HasPrevTarget(new(1, 1))) return false;
			return true;

			bool HasPrevTarget (Vector2Int direction) {
				if (direction == dir) return false;
				for (int i = 1; i < MAX_LEN; i++) {
					int x = unitPos.x + direction.x * i;
					int y = unitPos.y + direction.y * i;
					int id = squad.GetBlockAt(x, y, BlockType.Entity);
					if (id != 0 && AllCameraScrollID.Contains(id)) return true;
				}
				return false;
			}
		}


		#endregion




	}
}
