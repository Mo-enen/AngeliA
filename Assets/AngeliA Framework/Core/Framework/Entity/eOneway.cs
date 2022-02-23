using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class eOneway : Entity {


		// Const
		private const PhysicsMask Mask = PhysicsMask.Character | PhysicsMask.Environment | PhysicsMask.Item;

		// Api
		public override EntityLayer Layer => EntityLayer.Environment;
		public abstract Direction4 GateDirection { get; }
		protected int ReboundFrame { get; private set; } = int.MinValue;

		// Data
		private static readonly HitInfo[] c_PhysicsUpdate = new HitInfo[16];
		private static readonly HitInfo[] c_Rebound = new HitInfo[4];
		private int LastContactFrame = int.MinValue;


		// MSG
		public override void OnCreate (int frame) {
			Width = Const.CELL_SIZE;
			Height = Const.CELL_SIZE;
		}


		public override void FillPhysics (int frame) {
			CellPhysics.FillEntity(PhysicsLayer.Environment, this, true, Const.ONEWAY_TAG);
		}


		public override void PhysicsUpdate (int frame) {
			var rect = Rect;
			int count = CellPhysics.OverlapAll(c_PhysicsUpdate, Mask, rect, this);
			bool contact = false;
			for (int i = 0; i < count; i++) {
				var hit = c_PhysicsUpdate[i];
				if (hit.Entity is eRigidbody rig) {
					var rRect = rig.Rect;
					if (!HasVelocityInDirection(rig, GateDirection.Opposite())) continue;
					switch (GateDirection) {
						case Direction4.Left:
							if (
								rRect.xMax - rig.FinalVelocityX <= rect.xMin &&
								rRect.xMax > rect.xMin
							) {
								rig.Move(rect.xMin - rig.Width - rig.OffsetX, rig.Y, int.MaxValue);
								rig.DisableTopCarryUntil(frame + 1);
								rig.VelocityX = 0;
								contact = true;
							}
							break;
						case Direction4.Right:
							if (
								rRect.xMin - rig.FinalVelocityX >= rect.xMax &&
								rRect.xMin < rect.xMax
							) {
								rig.Move(rect.xMax - rig.OffsetX, rig.Y, int.MaxValue);
								rig.DisableTopCarryUntil(frame + 1);
								rig.VelocityX = 0;
								contact = true;
							}
							break;
						case Direction4.Down:
							if (
								rRect.yMax - rig.FinalVelocityY <= rect.yMin &&
								rRect.yMax > rect.yMin
							) {
								rig.Move(rig.X, rect.yMin - rig.Height, int.MaxValue);
								rig.VelocityY = 0;
								contact = true;
							}
							break;
						case Direction4.Up:
							if (
								rRect.yMin - rig.FinalVelocityY >= rect.yMax &&
								rRect.yMin < rect.yMax
							) {
								rig.Move(rig.X, rect.yMax, int.MaxValue);
								rig.VelocityY = 0;
								contact = true;
							}
							break;
					}
				}
			}
			c_PhysicsUpdate.Dispose();
			// Contact Check
			if (!contact) {
				const int GAP = 1;
				RectInt edge = GateDirection switch {
					Direction4.Down => new(rect.x, rect.y - GAP, rect.width, GAP),
					Direction4.Up => new(rect.x, rect.yMax, rect.width, GAP),
					Direction4.Left => new(rect.x - GAP, rect.y, GAP, rect.height),
					Direction4.Right => new(rect.xMax, rect.y, GAP, rect.height),
					_ => throw new System.NotImplementedException(),
				};
				int rCount = CellPhysics.OverlapAll(c_Rebound, Mask, edge, this);
				for (int i = 0; i < rCount; i++) {
					var hit = c_Rebound[i];
					if (
						hit.Entity is eRigidbody rig &&
						!rig.Rect.Overlaps(rect.Shrink(2))
					) {
						contact = true;
						break;
					}
				}
				c_Rebound.Dispose();
			}
			if (contact) {
				if (LastContactFrame < frame - 1) {
					ReboundFrame = frame;
				}
				LastContactFrame = frame;
			}
		}


		private static bool HasVelocityInDirection (eRigidbody rig, Direction4 dir) => dir switch {
			Direction4.Down => rig.FinalVelocityY < 0,
			Direction4.Up => rig.FinalVelocityY > 0,
			Direction4.Left => rig.FinalVelocityX < 0,
			Direction4.Right => rig.FinalVelocityX > 0,
			_ => throw new System.NotImplementedException(),
		};


	}
}
