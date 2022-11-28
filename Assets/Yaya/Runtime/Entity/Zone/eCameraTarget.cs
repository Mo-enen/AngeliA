using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public sealed class eCameraTarget : Zone {


		private static int UpdateFrame = int.MinValue;
		protected override bool UseFirstTarget => false;


		public override void FrameUpdate () {

			base.FrameUpdate();
			if (Game.GlobalFrame <= UpdateFrame) return;
			if (!TargetRect.HasValue) return;
			var player = ePlayer.Current;
			if (player == null || !player.Active) return;
			var targetRect = TargetRect.Value;
			if (!targetRect.Contains(player.X, player.Y)) return;
			UpdateFrame = Game.GlobalFrame;

			// Clamp Camera
			var game = Game.Current;
			var yaya = Yaya.Current;
			var cameraRect = CellRenderer.CameraRect;
			int viewOffsetX = game.ViewRect.x - CellRenderer.CameraRect.x;
			cameraRect.x = yaya.AimViewX - viewOffsetX;
			cameraRect.y = yaya.AimViewY;
			if (targetRect.width > cameraRect.width) {
				cameraRect.x = cameraRect.x.Clamp(targetRect.xMax - cameraRect.width, targetRect.xMin);
			} else {
				cameraRect.x = targetRect.x + targetRect.width / 2 - cameraRect.width / 2;
			}
			if (targetRect.height > cameraRect.height) {
				cameraRect.y = cameraRect.y.Clamp(targetRect.yMax - cameraRect.height, targetRect.yMin);
			} else {
				cameraRect.y = targetRect.y + targetRect.height / 2 - cameraRect.height / 2;
			}

			// Final
			Game.Current.SetViewPositionDely(
				cameraRect.x + viewOffsetX,
				cameraRect.y,
				YayaConst.PLAYER_VIEW_LERP_RATE,
				YayaConst.VIEW_PRIORITY_SYSTEM
			);
		}


	}
}