using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	[ExcludeInMapEditor]
	[EntityCapacity(8)]
	public abstract class eItem : Entity {




		#region --- VAR ---


		// Api
		public int VelocityY { get; private set; } = 0;
		protected abstract int ItemCode { get; }

		// Data
		private static readonly HitInfo[] c_MakeRoom = new HitInfo[5];
		private bool MakingRoom = false;


		#endregion




		#region --- MSG ---


		public override void OnActived (int frame) {
			base.OnActived(frame);
			Width = Const.ITEM_PHYSICS_SIZE;
			Height = Const.ITEM_PHYSICS_SIZE;
		}


		public override void FillPhysics (int frame) {
			base.FillPhysics(frame);
			CellPhysics.FillEntity((int)PhysicsLayer.Item, this, true, YayaConst.ITEM_TAG);
		}


		public override void PhysicsUpdate (int frame) {
			base.PhysicsUpdate(frame);
			// Fall
			bool grounded = !CellPhysics.RoomCheck((int)PhysicsMask.Map, Rect, this, Direction4.Down);
			if (!grounded) {
				if (VelocityY != 0) {
					var rect = Rect;
					rect.position = CellPhysics.Move(
						(int)PhysicsMask.Map, rect.position, 0, VelocityY, rect.size, this, out _, out bool stopY
					);
					Y = Mathf.Min(rect.y, Y);
					if (stopY) VelocityY = 0;
				}
				VelocityY = Mathf.Clamp(VelocityY - Const.GRAVITY, -Const.MAX_GRAVITY_SPEED, 0);
			} else {
				VelocityY = 0;
			}
			// Make Room
			if (MakingRoom = MakingRoom || (
				frame % 30 == 0 &&
				CellPhysics.Overlap((int)PhysicsMask.Item, Rect, this, OperationMode.TriggerOnly)
			)) {
				int count = CellPhysics.OverlapAll(c_MakeRoom, (int)PhysicsMask.Item, Rect, this, OperationMode.TriggerOnly);
				int deltaX = 0;
				for (int i = 0; i < count; i++) {
					deltaX += c_MakeRoom[i].Rect.x - X;
				}
				if (count > 0 && deltaX == 0) {
					deltaX = count % 2 == 0 ? 1 : -1;
				}
				var rect = Rect;
				rect.position = CellPhysics.MoveIgnoreOneway(
					(int)PhysicsMask.Map, rect.position,
					Mathf.Clamp(-deltaX, -6, 6), 0,
					rect.size, this
				);
				X = rect.x;
				Y = Mathf.Min(rect.y, Y);
				c_MakeRoom.Dispose();
			}
		}


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			CellRenderer.Draw(ItemCode, new(X + (Const.ITEM_PHYSICS_SIZE - Const.ITEM_RENDER_SIZE) / 2, Y, Const.ITEM_RENDER_SIZE, Const.ITEM_RENDER_SIZE));
		}


		#endregion




	}
}
