using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	// ←
	public class eCameraAutoScrollLeft : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.Left;
		protected override Direction3 DirectionY => Direction3.None;
	}

	// →
	public class eCameraAutoScrollRight : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.Right;
		protected override Direction3 DirectionY => Direction3.None;
	}

	// ↓
	public class eCameraAutoScrollBottom : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.None;
		protected override Direction3 DirectionY => Direction3.Down;
	}

	// ↑
	public class eCameraAutoScrollTop : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.None;
		protected override Direction3 DirectionY => Direction3.Up;
	}

	// ↙
	public class eCameraAutoScrollBottomLeft : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.Left;
		protected override Direction3 DirectionY => Direction3.Down;
	}

	// ↘
	public class eCameraAutoScrollBottomRight : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.Right;
		protected override Direction3 DirectionY => Direction3.Down;
	}

	// ↖
	public class eCameraAutoScrollTopLeft : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.Left;
		protected override Direction3 DirectionY => Direction3.Up;
	}

	// ↗
	public class eCameraAutoScrollTopRight : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.Right;
		protected override Direction3 DirectionY => Direction3.Up;
	}


	[EntityAttribute.Capacity(16)]
	[EntityAttribute.MapEditorGroup("Camera")]
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.DontDestroyOutOfRange]
	public abstract class eCameraAutoScroll : Entity {




		#region --- VAR ---


		// Const
		private const int PRIORITY = YayaConst.VIEW_PRIORITY_SYSTEM + 1;

		// Api
		protected abstract Direction3 DirectionX { get; }
		protected abstract Direction3 DirectionY { get; }
		protected virtual int Speed => 24;

		// Data
		private static eCameraAutoScroll Current = null;
		private Vector2Int MaxPosition = default;
		private bool CameraReady = false;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			MaxPosition.x = X + (int)DirectionX * Const.MAP * Const.CEL;
			MaxPosition.y = Y + (int)DirectionY * Const.MAP * Const.CEL;
			CameraReady = false;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Current != null) {
				FrameUpdate_Scroll();
			} else {
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
				CameraReady = false;
				StartScroll();
			}

			// Right to Left
			if (DirectionX != Direction3.Right && playerPrevX > thisX && player.X <= thisX) {
				CameraReady = false;
				StartScroll();
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
				EndScroll();
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
					EndScroll();
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
					nextScroll.StartScroll();
					nextScroll.CameraReady = true;
					nextScroll.Move();
					return;
				}
			}

			// Movement
			Move();

		}


		#endregion




		#region --- API ---


		protected void StartScroll () => Current = this;


		protected void EndScroll () {
			Current = null;
			Active = false;
		}


		#endregion




		#region --- LGC ---


		private void Move () {
			var view = Game.Current.ViewRect;
			if (CameraReady) {
				X += (int)DirectionX * Speed;
				Y += (int)DirectionY * Speed;
			}
			int targetX = X + Const.CEL / 2 - view.width / 2;
			int targetY = Y + Const.CEL / 2 - view.height / 2;
			if (CameraReady) {
				//Game.Current.SetViewPositionImmediately(targetX, targetY);
				Game.Current.SetViewPositionDelay(targetX, targetY, 1000, PRIORITY);
			} else {
				Game.Current.SetViewPositionDelay(targetX, targetY, 100, PRIORITY);
			}
			CameraReady = CameraReady || Mathf.Abs(view.x - targetX) < Speed && Mathf.Abs(view.y - targetY) < Speed;
		}


		#endregion




	}
}
