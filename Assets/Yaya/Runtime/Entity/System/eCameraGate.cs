using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public class eCameraGateDown : eCameraGate {
		protected override Direction4 Direction => Direction4.Down;
	}
	public class eCameraGateUp : eCameraGate {
		protected override Direction4 Direction => Direction4.Up;
	}
	public class eCameraGateLeft : eCameraGate {
		protected override Direction4 Direction => Direction4.Left;
	}
	public class eCameraGateRight : eCameraGate {
		protected override Direction4 Direction => Direction4.Right;
	}


	[EntityAttribute.ForceUpdate]
	[EntityAttribute.Capacity(16)]
	[EntityAttribute.MapEditorGroup("System")]
	public abstract class eCameraGate : Entity {


		// Api
		protected abstract Direction4 Direction { get; }


		// MSG
		public override void FrameUpdate () {

			base.FrameUpdate();

			var player = ePlayer.Current;
			if (player == null || !player.Active) return;

			// Player Pos Check
			if (Direction switch {
				Direction4.Down => player.Y > Y + Const.CEL / 2,
				Direction4.Up => player.Y < Y + Const.CEL / 2,
				Direction4.Left => player.X > X + Const.CEL / 2,
				Direction4.Right => player.X < X + Const.CEL / 2,
				_ => true,
			}) return;

			// Camera Range Check
			var cameraRect = CellRenderer.CameraRect;
			if (Direction switch {
				Direction4.Down or Direction4.Up => !(X + Const.CEL / 2).InRange(cameraRect.xMin, cameraRect.xMax),
				Direction4.Left or Direction4.Right => !(Y + Const.CEL / 2).InRange(cameraRect.yMin, cameraRect.yMax),
				_ => true,
			}) return;

			// Clamp Camera
			const int GAP = Const.CEL;
			var game = Game.Current;
			var yaya = Yaya.Current;
			int viewOffsetX = game.ViewRect.x - CellRenderer.CameraRect.x;
			cameraRect.x = yaya.AimViewX - viewOffsetX;
			cameraRect.y = yaya.AimViewY;
			switch (Direction) {
				case Direction4.Down:
					cameraRect.y = Mathf.Min(Y + Const.CEL / 2 + GAP - cameraRect.height, cameraRect.y);
					game.SetViewYDelay(cameraRect.y, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_SYSTEM);
					break;
				case Direction4.Up:
					cameraRect.y = Mathf.Max(Y + Const.CEL / 2 - GAP, cameraRect.y);
					game.SetViewYDelay(cameraRect.y, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_SYSTEM);
					break;
				case Direction4.Left:
					cameraRect.x = Mathf.Min(X + Const.CEL / 2 + GAP - cameraRect.width, cameraRect.x);
					game.SetViewXDelay(cameraRect.x + viewOffsetX, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_SYSTEM);
					break;
				case Direction4.Right:
					cameraRect.x = Mathf.Max(X + Const.CEL / 2 - GAP, cameraRect.x);
					game.SetViewXDelay(cameraRect.x + viewOffsetX, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_SYSTEM);
					break;
			}

		}


	}
}