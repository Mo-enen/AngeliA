using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AngeliaFramework;


namespace Yaya {
	public partial class eMapEditor {




		#region --- VAR ---



		#endregion




		#region --- MSG ---


		private void Update_View () {

			var view = Game.Current.ViewRect;

			// Move View
			if (
				FrameInput.MouseMidButton ||
				(FrameInput.CustomKeyPressing(Key.LeftCtrl) && FrameInput.MouseLeftButton)
			) {
				var cameraRect = CellRenderer.CameraRect;
				var uCameraRect = CellRenderer.MainCamera.rect;
				var screenDelta = FrameInput.MouseScreenPositionDelta;
				screenDelta.x = (int)(screenDelta.x * cameraRect.width / uCameraRect.width / Screen.width);
				screenDelta.y = (int)(screenDelta.y * cameraRect.height / uCameraRect.height / Screen.height);
				Game.Current.SetViewPositionImmediately(view.x - screenDelta.x, view.y - screenDelta.y);
			}

			// Pan View
			if (FrameInput.DirectionX != Direction3.None) {
				Game.Current.SetViewPositionDely(
					view.x + (int)FrameInput.DirectionX * Const.CEL, view.y, 500
				);
			}
			if (FrameInput.DirectionY != Direction3.None) {
				Game.Current.SetViewPositionDely(
					view.x, view.y + (int)FrameInput.DirectionY * Const.CEL, 500
				);
			}

			// Zoom View
			int wheel = FrameInput.MouseWheelDelta;
			if (wheel != 0 || (FrameInput.CustomKeyPressing(Key.LeftCtrl) && FrameInput.MouseRightButton)) {
				int delta = wheel != 0 ?
					wheel * -view.height / 16 :
					FrameInput.MouseScreenPositionDelta.y * -view.height / 512;
				var config = Game.Current.ViewConfig;
				int newHeight = (view.height + delta).Clamp(config.DefaultHeight, config.MaxHeight);
				int newWidth = config.ViewRatio * newHeight / 1000;
				int newX, newY;
				if (wheel != 0) {
					var uCameraRect = CellRenderer.MainCamera.rect;
					newX = Util.RemapUnclamped(
						(int)uCameraRect.xMin * Screen.width, (int)uCameraRect.xMax * Screen.width,
						view.x, view.x + (view.width - newWidth),
						FrameInput.MouseScreenPosition.x
					);
					newY = Util.RemapUnclamped(
						(int)uCameraRect.yMin * Screen.height, (int)uCameraRect.yMax * Screen.height,
						view.y, view.y + (view.height - newHeight),
						FrameInput.MouseScreenPosition.y
					);
				} else {
					newX = view.x + (view.width - newWidth) / 2;
					newY = view.y + (view.height - newHeight) / 2;
				}
				Game.Current.SetViewRectImmetiately(newX, newY, newHeight);
			}

		}


		#endregion




		#region --- API ---



		#endregion




		#region --- LGC ---



		#endregion




	}
}