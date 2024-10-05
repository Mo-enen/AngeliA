using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public interface ICarrier {

	public bool AllowBeingCarry => true;
	public int CarryLeft { get; set; }
	public int CarryRight { get; set; }
	public int CarryHorizontalFrame { get; set; }

	private static readonly Pipe<(IRect rect, int delta)> CarryBuffer = new(1024);
	private static readonly Pipe<(Entity entity, int delta)> CarryPerformBuffer = new(1024);

	// MSG
	public void OnBeingCarry (int deltaX, int deltaY) { }

	// API
	public static void CarryTargetsOnTopHorizontally (Entity self, int _deltaX) {
		if (_deltaX == 0) return;
		CarryBuffer.Reset();
		CarryBuffer.LinkToTail((self.Rect, _deltaX));
		for (int safe = 0; CarryBuffer.TryPopHead(out var data) && safe < 1024; safe++) {
			var selfRect = data.rect;
			var deltaX = data.delta;
			var hits = Physics.OverlapAll(PhysicsMask.DYNAMIC, selfRect.EdgeOutside(Direction4.Up), out int count);
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
				if (entity is Rigidbody rig) {
					int oldX = rig.X;
					rig.PerformMove(nextDeltaX, 0, ignoreCarry: true);
					rig.MakeGrounded(1, self.TypeID);
					nextDeltaX = rig.X - oldX;
				} else {
					entity.X += nextDeltaX;
				}

				// Keep Carry
				CarryBuffer.LinkToTail((entity.Rect, nextDeltaX));
			}

		}

	}

	public static void CarryTargetsOnTopVertically (Entity self, int _deltaY, bool fromOneway = false) {
		if (_deltaY == 0) return;
		CarryBuffer.Reset();
		CarryPerformBuffer.Reset();
		CarryBuffer.LinkToTail((self.Rect, _deltaY));
		for (int safe = 0; CarryBuffer.TryPopHead(out var data) && safe < 1024; safe++) {
			var selfRect = data.rect;
			var deltaY = data.delta;
			var hits = Physics.OverlapAll(
				PhysicsMask.DYNAMIC,
				selfRect.EdgeOutside(Direction4.Up, 32),
				out int count
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
			var deltaY = data.delta;
			if (entity is Character character) {
				if (
					character.Movement.IsFlying ||
					character.VelocityY > deltaY ||
					Game.GlobalFrame < character.Movement.LastJumpFrame + 2
				) {
					character.CancelMakeGrounded();
					continue;
				}
			}
			if (entity is Rigidbody rig) {

				if (fromOneway && rig.IgnoringOneway) continue;

				rig.MakeGrounded(1);
				rig.IgnoreGravity(1);
				rig.VelocityY = Util.Max(deltaY, self.Rect.yMax - entity.Rect.y);

			} else {
				entity.Y += deltaY;
			}
		}
	}

}
