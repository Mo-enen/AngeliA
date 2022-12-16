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
	public abstract class eCameraAutoScroll : Entity {




		#region --- VAR ---


		// Const
		private const int PRIORITY = YayaConst.VIEW_PRIORITY_SYSTEM;

		// Api
		protected abstract Direction3 DirectionX { get; }
		protected abstract Direction3 DirectionY { get; }
		protected virtual int Speed => 24;

		// Data
		private static eCameraAutoScroll Current = null;
		private Vector2Int MaxPosition = default;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			MaxPosition.x = (int)DirectionX * Const.MAP * Const.CEL;
			MaxPosition.y = (int)DirectionY * Const.MAP * Const.CEL;
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Current != null) {
				if (Current == this) {
					FrameUpdate_Scroll();
				}
			} else {
				FrameUpdate_Idle();
			}
		}


		private void FrameUpdate_Idle () {

			var player = ePlayer.Current;
			if (player == null || !player.Active) return;

			// Check for Scroll Start
			// Left to Right
			if (DirectionX != Direction3.Left) {


			}

			// Right to Left
			if (DirectionX != Direction3.Right) {


			}


		}


		private void FrameUpdate_Scroll () {

			// End by Max Pos Check
			if (
				(DirectionX == Direction3.Left && X < MaxPosition.x) ||
				(DirectionX == Direction3.Right && X > MaxPosition.x) ||
				(DirectionY == Direction3.Down && Y < MaxPosition.y) ||
				(DirectionY == Direction3.Up && Y > MaxPosition.y)
			) {
				EndScroll();
				return;
			}

			// End by Hit Other Scroll Entity Check




			// Scroll the Camera
			//PRIORITY
			//const int Lerp = 200;





		}


		#endregion




		#region --- API ---


		public void StartScroll () => Current = this;


		public void EndScroll () {
			Current = null;
			Active = false;
		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}
