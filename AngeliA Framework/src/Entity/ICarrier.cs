using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public interface ICarrier {

	private const int DETECT_RANGE = 96;
	public bool AllowBeingCarry => true;
	public int CarryLeft { get; set; }
	public int CarryRight { get; set; }
	public int CarryHorizontalFrame { get; set; }

	private static readonly Pipe<(IRect rect, int delta)> CarryBuffer = new(1024);
	private static readonly Pipe<(Entity entity, int delta)> CarryPerformBuffer = new(1024);

	// MSG
	public void OnBeingCarry (int deltaX, int deltaY) { }

	public void PerformCarry (int x, int y);

	// API
	public static void CarryTargetsOnTopHorizontally (Entity self, int _deltaX, OperationMode colMode = OperationMode.ColliderOnly) {
		if (_deltaX == 0) return;
		CarryBuffer.Reset();
		CarryBuffer.LinkToTail((self.Rect, _deltaX));
		for (int safe = 0; CarryBuffer.TryPopHead(out var data) && safe < 1024; safe++) {
			var selfRect = data.rect;
			var deltaX = data.delta;
			var hits = Physics.OverlapAll(
				PhysicsMask.DYNAMIC,
				selfRect.EdgeOutside(Direction4.Up, DETECT_RANGE),
				out int count, null, colMode
			);
			for (int i = 0; i < count; i++) {

				var hit = hits[i];
				var entity = hit.Entity;
				if (entity is not ICarrier carrier) continue;
				if (!carrier.AllowBeingCarry) continue;
				if (entity.Rect.y < selfRect.yMax) continue;

				// Reset Limit Value
				if (carrier.CarryHorizontalFrame != Game.GlobalFrame) {
					carrier.CarryHorizontalFrame = Game.GlobalFrame;
					carrier.CarryLeft = 0;
					carrier.CarryRight = 0;
				}

				// Update Left/Right Limit Value
				int nextDeltaX;
				if (deltaX > 0) {
					if (deltaX <= carrier.CarryRight) continue;
					nextDeltaX = deltaX - carrier.CarryRight;
					carrier.CarryRight = deltaX;
				} else {
					if (deltaX >= carrier.CarryLeft) continue;
					nextDeltaX = deltaX - carrier.CarryLeft;
					carrier.CarryLeft = deltaX;
				}

				// Move
				int oldX = entity.X;
				carrier.PerformCarry(nextDeltaX, 0);
				carrier.OnBeingCarry(nextDeltaX, 0);
				nextDeltaX = entity.X - oldX;

				// Keep Carry
				CarryBuffer.LinkToTail((entity.Rect, nextDeltaX));
			}
		}
	}

	public static void CarryTargetsOnTopVertically (Entity self, int _deltaY, OperationMode colMode = OperationMode.ColliderOnly) {
		if (_deltaY == 0) return;
		CarryBuffer.Reset();
		CarryPerformBuffer.Reset();
		CarryBuffer.LinkToTail((self.Rect, _deltaY));
		for (int safe = 0; CarryBuffer.TryPopHead(out var data) && safe < 1024; safe++) {
			var selfRect = data.rect;
			var deltaY = data.delta;
			var hits = Physics.OverlapAll(
				PhysicsMask.DYNAMIC,
				selfRect.EdgeOutside(Direction4.Up, DETECT_RANGE),
				out int count, null, colMode
			);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				var entity = hit.Entity;
				if (entity is not ICarrier carrier) continue;
				if (!carrier.AllowBeingCarry) continue;
				if (entity.Rect.y < selfRect.yMax - 64) continue;
				// Keep Carry
				CarryPerformBuffer.LinkToTail((entity, deltaY));
				CarryBuffer.LinkToTail((entity.Rect, deltaY));
			}
		}

		// Perform
		for (
			int safe = 0;
			CarryPerformBuffer.TryPopHead(out var data) && safe < 1024;
			safe++
		) {
			// Perform Move
			var entity = data.entity;
			if (entity is not ICarrier carrier) continue;

			var deltaY = data.delta;
			if (entity is IWithCharacterMovement withMovement) {
				var movement = withMovement.CurrentMovement;
				if (
					movement != null &&
					movement.IsFlying ||
					movement.Target.VelocityY > deltaY ||
					Game.GlobalFrame < movement.LastJumpFrame + 2
				) {
					movement.Target.CancelMakeGrounded();
					continue;
				}
			}

			// Move
			carrier.PerformCarry(0, deltaY);
			carrier.OnBeingCarry(0, deltaY);

		}
	}

}
