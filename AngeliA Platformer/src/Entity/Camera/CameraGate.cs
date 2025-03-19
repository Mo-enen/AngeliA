using System.Collections;
using System.Collections.Generic;


using AngeliA;
namespace AngeliA.Platformer;


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


[EntityAttribute.DontDrawBehind]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.MapEditorGroup("System", -512)]
public abstract class CameraGate : Entity {


	// Api
	private const int PRIORITY = 0;
	protected abstract Direction4 Direction { get; }

	// Data
	private static int LastClampFrame = -1;
	private static int? TargetMinX = null;
	private static int? TargetMinY = null;
	private static int? TargetMaxX = null;
	private static int? TargetMaxY = null;


	// MSG
	public override void BeforeUpdate () {
		base.BeforeUpdate();
		TargetMinX = null;
		TargetMinY = null;
		TargetMaxX = null;
		TargetMaxY = null;
	}


	public override void Update () {
		base.Update();

		var player = PlayerSystem.Selecting;
		if (player == null || !player.Active) return;

		// Player Pos Check
		if (Direction switch {
			Direction4.Down => player.Y > Y,
			Direction4.Up => player.Y < Y + Height,
			Direction4.Left => player.X > X,
			Direction4.Right => player.X < X + Width,
			_ => true,
		}) return;

		var centerX = X + Const.HALF;
		var centerY = Y + Const.HALF;

		// Camera Range Check
		var cameraRect = Renderer.CameraRect;
		if (Direction switch {
			Direction4.Down or Direction4.Up => !centerX.InRangeInclude(cameraRect.xMin, cameraRect.xMax),
			Direction4.Left or Direction4.Right => !centerY.InRangeInclude(cameraRect.yMin, cameraRect.yMax),
			_ => true,
		}) return;

		// Set Min Max Values
		const int GAP = -Const.HALF;
		switch (Direction) {
			case Direction4.Down:
				TargetMaxY = Util.Min(TargetMaxY ?? int.MaxValue, centerY + GAP);
				break;
			case Direction4.Up:
				TargetMinY = Util.Max(TargetMinY ?? int.MinValue, centerY - GAP);
				break;
			case Direction4.Left:
				TargetMaxX = Util.Min(TargetMaxX ?? int.MaxValue, centerX + GAP);
				break;
			case Direction4.Right:
				TargetMinX = Util.Max(TargetMinX ?? int.MinValue, centerX - GAP);
				break;
		}

	}


	public override void LateUpdate () {

		base.LateUpdate();
		if (Game.GlobalFrame <= LastClampFrame) return;

		// Clamp Camera
		var player = PlayerSystem.Selecting;
		var cameraRect = Renderer.CameraRect;
		int viewOffsetX = Stage.ViewRect.x - Renderer.CameraRect.x;
		if (player != null && player.Active) {
			cameraRect.x = PlayerSystem.AimViewX - viewOffsetX;
			cameraRect.y = PlayerSystem.AimViewY;
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
				Stage.SetViewXDelay(cameraRect.x + viewOffsetX, 96, priority: PRIORITY);
			}
		} else if (TargetMinX.HasValue) {
			cameraRect.x = Util.Max(cameraRect.x, TargetMinX.Value);
			if (cameraRect.x != oldX) {
				Stage.SetViewXDelay(cameraRect.x + viewOffsetX, 96, priority: PRIORITY);
			}
		} else if (TargetMaxX.HasValue) {
			cameraRect.x = Util.Min(cameraRect.x, TargetMaxX.Value - cameraRect.width);
			if (cameraRect.x != oldX) {
				Stage.SetViewXDelay(cameraRect.x + viewOffsetX, 96, priority: PRIORITY);
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
				Stage.SetViewYDelay(cameraRect.y, 96, priority: PRIORITY);
			}
		} else if (TargetMinY.HasValue) {
			cameraRect.y = Util.Max(cameraRect.y, TargetMinY.Value);
			if (cameraRect.y != oldY) {
				Stage.SetViewYDelay(cameraRect.y, 96, priority: PRIORITY);
			}
		} else if (TargetMaxY.HasValue) {
			cameraRect.y = Util.Min(cameraRect.y, TargetMaxY.Value - cameraRect.height);
			if (cameraRect.y != oldY) {
				Stage.SetViewYDelay(cameraRect.y, 96, priority: PRIORITY);
			}
		}

		LastClampFrame = Game.GlobalFrame;
	}


}