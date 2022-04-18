using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eOneway : Entity {


		// Const
		private const int MASK = (int)(PhysicsMask.Character | PhysicsMask.Environment | PhysicsMask.Item);

		// Api
		public abstract Direction4 GateDirection { get; }
		protected int ReboundFrame { get; private set; } = int.MinValue;

		// Data
		private static readonly HitInfo[] c_Rebound = new HitInfo[4];
		private int LastContactFrame = int.MinValue;


		// MSG
		public override void OnActived (int frame) {
			Width = Const.CELL_SIZE;
			Height = Const.CELL_SIZE;
		}


		public override void PhysicsUpdate (int frame) {
			if (ContactReboundUpdate(frame)) {
				if (LastContactFrame < frame - 1) {
					ReboundFrame = frame;
				}
				LastContactFrame = frame;
			}
			base.PhysicsUpdate(frame);
		}


		protected virtual bool ContactReboundUpdate (int frame) {
			var rect = Rect;
			bool contact = false;
			const int GAP = 1;
			RectInt edge = GateDirection switch {
				Direction4.Down => new(rect.x, rect.y - GAP, rect.width, GAP),
				Direction4.Up => new(rect.x, rect.yMax, rect.width, GAP),
				Direction4.Left => new(rect.x - GAP, rect.y, GAP, rect.height),
				Direction4.Right => new(rect.xMax, rect.y, GAP, rect.height),
				_ => throw new System.NotImplementedException(),
			};
			int rCount = CellPhysics.OverlapAll(c_Rebound, MASK, edge, this);
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
			return contact;
		}



	}
}
