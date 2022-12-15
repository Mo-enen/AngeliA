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
	[EntityAttribute.MapEditorGroup("Camera")]
	public abstract class eCameraGate : Entity {


		// Api
		protected abstract Direction4 Direction { get; }

		// Data
		private static int LastClampFrame = -1;
		private static int? TargetMinX = null;
		private static int? TargetMinY = null;
		private static int? TargetMaxX = null;
		private static int? TargetMaxY = null;


		// MSG
		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			TargetMinX = null;
			TargetMinY = null;
			TargetMaxX = null;
			TargetMaxY = null;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();

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

			// Set Min Max Values
			const int GAP = Const.CEL;
			var game = Game.Current;
			var yaya = YayaGame.Current;
			int viewOffsetX = game.ViewRect.x - CellRenderer.CameraRect.x;
			cameraRect.x = yaya.AimViewX - viewOffsetX;
			cameraRect.y = yaya.AimViewY;
			switch (Direction) {
				case Direction4.Down:
					TargetMaxY = Mathf.Min(TargetMaxY ?? int.MaxValue, Y + Const.CEL / 2 + GAP);
					break;
				case Direction4.Up:
					TargetMinY = Mathf.Max(TargetMinY ?? int.MinValue, Y + Const.CEL / 2 - GAP);
					break;
				case Direction4.Left:
					TargetMaxX = Mathf.Min(TargetMaxX ?? int.MaxValue, X + Const.CEL / 2 + GAP);
					break;
				case Direction4.Right:
					TargetMinX = Mathf.Max(TargetMinX ?? int.MinValue, X + Const.CEL / 2 - GAP);
					break;
			}

		}


		public override void FrameUpdate () {

			base.FrameUpdate();
			if (Game.GlobalFrame <= LastClampFrame) return;

			// Clamp Camera
			var game = Game.Current;
			var yaya = YayaGame.Current;
			var cameraRect = CellRenderer.CameraRect;
			int viewOffsetX = game.ViewRect.x - CellRenderer.CameraRect.x;
			cameraRect.x = yaya.AimViewX - viewOffsetX;
			cameraRect.y = yaya.AimViewY;

			// H
			if (TargetMinX.HasValue && TargetMaxX.HasValue) {
				int targetWidth = TargetMaxX.Value - TargetMinX.Value;
				if (targetWidth > cameraRect.width) {
					cameraRect.x = cameraRect.x.Clamp(TargetMinX.Value, TargetMaxX.Value - cameraRect.width);
				} else {
					cameraRect.x = TargetMinX.Value + targetWidth / 2 - cameraRect.width / 2;
				}
				game.SetViewXDelay(cameraRect.x + viewOffsetX, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_SYSTEM);
			} else if (TargetMinX.HasValue) {
				cameraRect.x = Mathf.Max(cameraRect.x, TargetMinX.Value);
				game.SetViewXDelay(cameraRect.x + viewOffsetX, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_SYSTEM);
			} else if (TargetMaxX.HasValue) {
				cameraRect.x = Mathf.Min(cameraRect.x, TargetMaxX.Value - cameraRect.width);
				game.SetViewXDelay(cameraRect.x + viewOffsetX, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_SYSTEM);
			}

			// V
			if (TargetMinY.HasValue && TargetMaxY.HasValue) {
				int targetHeight = TargetMaxY.Value - TargetMinY.Value;
				if (targetHeight > cameraRect.height) {
					cameraRect.y = cameraRect.y.Clamp(TargetMinY.Value, TargetMaxY.Value - cameraRect.height);
				} else {
					cameraRect.y = TargetMinY.Value + targetHeight / 2 - cameraRect.height / 2;
				}
				game.SetViewYDelay(cameraRect.y, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_SYSTEM);
			} else if (TargetMinY.HasValue) {
				cameraRect.y = Mathf.Max(cameraRect.y, TargetMinY.Value);
				game.SetViewYDelay(cameraRect.y, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_SYSTEM);
			} else if (TargetMaxY.HasValue) {
				cameraRect.y = Mathf.Min(cameraRect.y, TargetMaxY.Value - cameraRect.height);
				game.SetViewYDelay(cameraRect.y, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_SYSTEM);
			}

			LastClampFrame = Game.GlobalFrame;
		}


	}
}