using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {


	public class CameraGateDown : CameraGate {
		protected override Direction4 Direction => Direction4.Down;
	}
	public class CameraGateUp : CameraGate {
		protected override Direction4 Direction => Direction4.Up;
	}
	public class CameraGateLeft : CameraGate {
		protected override Direction4 Direction => Direction4.Left;
	}
	public class CameraGateRight : CameraGate {
		protected override Direction4 Direction => Direction4.Right;
	}


	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.Capacity(16)]
	[EntityAttribute.MapEditorGroup("System")]
	[RequireSprite("{0}")]
	public abstract class CameraGate : Entity {


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

			var player = Player.Selecting;
			if (player == null || !player.Active) return;

			// Player Pos Check
			if (Direction switch {
				Direction4.Down => player.Y > Y + Const.HALF,
				Direction4.Up => player.Y < Y + Const.HALF,
				Direction4.Left => player.X > X + Const.HALF,
				Direction4.Right => player.X < X + Const.HALF,
				_ => true,
			}) return;

			// Camera Range Check
			var cameraRect = CellRenderer.CameraRect;
			if (Direction switch {
				Direction4.Down or Direction4.Up => !(X + Const.HALF).InRange(cameraRect.xMin, cameraRect.xMax),
				Direction4.Left or Direction4.Right => !(Y + Const.HALF).InRange(cameraRect.yMin, cameraRect.yMax),
				_ => true,
			}) return;

			// Set Min Max Values
			const int GAP = Const.CEL;
			switch (Direction) {
				case Direction4.Down:
					TargetMaxY = Util.Min(TargetMaxY ?? int.MaxValue, Y + Const.HALF + GAP);
					break;
				case Direction4.Up:
					TargetMinY = Util.Max(TargetMinY ?? int.MinValue, Y + Const.HALF - GAP);
					break;
				case Direction4.Left:
					TargetMaxX = Util.Min(TargetMaxX ?? int.MaxValue, X + Const.HALF + GAP);
					break;
				case Direction4.Right:
					TargetMinX = Util.Max(TargetMinX ?? int.MinValue, X + Const.HALF - GAP);
					break;
			}

		}


		public override void FrameUpdate () {

			base.FrameUpdate();
			if (Game.GlobalFrame <= LastClampFrame) return;

			// Clamp Camera
			var player = Player.Selecting;
			var cameraRect = CellRenderer.CameraRect;
			int viewOffsetX = Stage.ViewRect.x - CellRenderer.CameraRect.x;
			if (player != null && player.Active) {
				cameraRect.x = player.AimViewX - viewOffsetX;
				cameraRect.y = player.AimViewY;
			}
			int oldX = cameraRect.x;
			int oldY = cameraRect.y;

			// H
			if (TargetMinX.HasValue && TargetMaxX.HasValue) {
				int targetWidth = TargetMaxX.Value - TargetMinX.Value;
				if (targetWidth > cameraRect.width) {
					cameraRect.x = cameraRect.x.Clamp(TargetMinX.Value, TargetMaxX.Value - cameraRect.width);
				} else {
					cameraRect.x = TargetMinX.Value + targetWidth / 2 - cameraRect.width / 2;
				}
				if (cameraRect.x != oldX) {
					Stage.SetViewXDelay(cameraRect.x + viewOffsetX, 96, 0);
				}
			} else if (TargetMinX.HasValue) {
				cameraRect.x = Util.Max(cameraRect.x, TargetMinX.Value);
				if (cameraRect.x != oldX) {
					Stage.SetViewXDelay(cameraRect.x + viewOffsetX, 96, 0);
				}
			} else if (TargetMaxX.HasValue) {
				cameraRect.x = Util.Min(cameraRect.x, TargetMaxX.Value - cameraRect.width);
				if (cameraRect.x != oldX) {
					Stage.SetViewXDelay(cameraRect.x + viewOffsetX, 96, 0);
				}
			}

			// V
			if (TargetMinY.HasValue && TargetMaxY.HasValue) {
				int targetHeight = TargetMaxY.Value - TargetMinY.Value;
				if (targetHeight > cameraRect.height) {
					cameraRect.y = cameraRect.y.Clamp(TargetMinY.Value, TargetMaxY.Value - cameraRect.height);
				} else {
					cameraRect.y = TargetMinY.Value + targetHeight / 2 - cameraRect.height / 2;
				}
				if (cameraRect.y != oldY) {
					Stage.SetViewYDelay(cameraRect.y, 96, 0);
				}
			} else if (TargetMinY.HasValue) {
				cameraRect.y = Util.Max(cameraRect.y, TargetMinY.Value);
				if (cameraRect.y != oldY) {
					Stage.SetViewYDelay(cameraRect.y, 96, 0);
				}
			} else if (TargetMaxY.HasValue) {
				cameraRect.y = Util.Min(cameraRect.y, TargetMaxY.Value - cameraRect.height);
				if (cameraRect.y != oldY) {
					Stage.SetViewYDelay(cameraRect.y, 96, 0);
				}
			}

			LastClampFrame = Game.GlobalFrame;
		}


	}
}