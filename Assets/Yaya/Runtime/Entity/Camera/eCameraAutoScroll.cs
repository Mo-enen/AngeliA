using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


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
	[EntityAttribute.MapEditorGroup("Camera")]
	[EntityAttribute.ForceUpdate]
	public abstract class eCameraAutoScroll : Entity {




		#region --- SUB ---


		[System.Serializable]
		public class CameraScrollMeta {
			public Vector3Int[] EntrancePositions = new Vector3Int[0];
		}


		#endregion




		#region --- VAR ---


		// Const
		public const int MAX_LEN = 64;
		private const int PRIORITY = YayaConst.VIEW_PRIORITY_SYSTEM + 1;

		// Api
		public abstract Direction3 DirectionX { get; }
		public abstract Direction3 DirectionY { get; }
		public virtual int Speed => 24;

		// Data
		private static eCameraAutoScroll Current = null;
		private static readonly HashSet<Vector3Int> EntrancePool = new();
		private Vector2Int MaxPosition = default;
		private bool IsEntrance = true;


		#endregion




		#region --- MSG ---


		[AfterGameInitialize]
		public static void Init () {
			EntrancePool.Clear();
			// Get Meta
			var meta = AngeUtil.LoadJson<CameraScrollMeta>(Const.MetaRoot);
			// Meta >> Pool
			if (meta != null) {
				foreach (var pos in meta.EntrancePositions) {
					EntrancePool.TryAdd(pos);
				}
			}
		}


		public override void OnActived () {
			base.OnActived();
			MaxPosition.x = X + (int)DirectionX * MAX_LEN * Const.CEL;
			MaxPosition.y = Y + (int)DirectionY * MAX_LEN * Const.CEL;
			IsEntrance = EntrancePool.Contains(new Vector3Int(
				X.UDivide(Const.CEL),
				Y.UDivide(Const.CEL),
				Game.Current.ViewZ)
			);
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (ePlayer.Current == null || !ePlayer.Current.Active) {
				Current = null;
				return;
			}
			if (ePlayer.Current.CharacterState != CharacterState.GamePlay) {
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

			var player = ePlayer.Current;
			if (player == null || !player.Active) return;
			int thisX = X + Const.CEL / 2;
			int thisY = Y + Const.CEL / 2;
			var cameraRect = CellRenderer.CameraRect;
			int playerPrevX = player.PrevRect.x + player.PrevRect.width / 2;

			// Check Camera in Range
			if (!cameraRect.Contains(thisX, thisY)) return;

			// Left to Right
			if (DirectionX != Direction3.Left && playerPrevX < thisX && player.X >= thisX) {
				Current = this;
			}

			// Right to Left
			if (DirectionX != Direction3.Right && playerPrevX > thisX && player.X <= thisX) {
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
				new RectInt(X + Const.CEL / 2, Y + Const.CEL / 2, 1, 1),
				YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly
			);
			if (nextScroll != null && nextScroll.Active) {

				// End Scroll Check
				if (
					(int)DirectionX == -(int)nextScroll.DirectionX &&
					(int)DirectionY == -(int)nextScroll.DirectionY
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
			var player = ePlayer.Current;
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
			Game.Current.SetViewPositionImmediately(view.x + deltaX, view.y + deltaY);
			Game.Current.SetViewPositionDelay(
				X + Const.CEL / 2 - view.width / 2,
				Y + Const.CEL / 2 - view.height / 2
				, 50, PRIORITY
			);
		}


		#endregion




	}
}
