using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.DontSpawnFromWorld]
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.Capacity(64)]
	public class eItemHolder : Entity {




		#region --- VAR ---


		// Const
		private const int ITEM_PHYSICS_SIZE = Const.HALF;
		private const int ITEM_RENDER_SIZE = Const.CEL * 2 / 3;
		private const int GRAVITY = 5;
		private const int MAX_GRAVITY_SPEED = 64;

		// Api
		public int VelocityY { get; private set; } = 0;
		public int ItemID { get; set; } = 0;

		// Data
		private static readonly PhysicsCell[] c_MakeRoom = new PhysicsCell[5];
		private bool MakeRoomToRight = true;
		private bool MakingRoom = false;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			Width = ITEM_PHYSICS_SIZE;
			Height = ITEM_PHYSICS_SIZE;
			MakingRoom = false;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(Const.LAYER_ITEM, this, true);
		}


		public override void PhysicsUpdate () {
			int frame = Game.GlobalFrame;
			base.PhysicsUpdate();
			// Fall
			bool grounded = !CellPhysics.RoomCheck(Const.MASK_MAP, Rect, this, Direction4.Down);
			if (!grounded) {
				if (VelocityY != 0) {
					var rect = Rect;
					rect.position = CellPhysics.Move(
						Const.MASK_MAP, rect.position, 0, VelocityY, rect.size, this, out _, out bool stopY
					);
					Y = Mathf.Min(rect.y, Y);
					if (stopY) VelocityY = 0;
				}
				VelocityY = Mathf.Clamp(VelocityY - GRAVITY, -MAX_GRAVITY_SPEED, 0);
			} else {
				VelocityY = 0;
			}
			// Make Room
			if (MakingRoom = MakingRoom || (
				frame % 30 == 0 &&
				CellPhysics.Overlap(Const.MASK_ITEM, Rect, this, OperationMode.TriggerOnly)
			)) {
				int count = CellPhysics.OverlapAll(c_MakeRoom, Const.MASK_ITEM, Rect, this, OperationMode.TriggerOnly);
				int deltaX = 0;
				int speed = 4;
				for (int i = 0; i < count; i++) {
					var hit = c_MakeRoom[i];
					deltaX += hit.Rect.x - X;
					speed = Mathf.Max(speed, Mathf.Abs(hit.Rect.x - X) / 4);
					if (hit.Entity is eItemHolder hitItem) {
						MakeRoomToRight = !hitItem.MakeRoomToRight;
					}
				}
				if (count > 0 && deltaX == 0) {
					deltaX = MakeRoomToRight ? speed : -speed;
				}
				var rect = Rect;
				rect.position = CellPhysics.MoveIgnoreOneway(
					Const.MASK_MAP, rect.position,
					Mathf.Clamp(-deltaX, -speed, speed), 0,
					rect.size, this
				);
				X = rect.x;
				Y = Mathf.Min(rect.y, Y);
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(
				ItemID,
				new RectInt(
					X + (ITEM_PHYSICS_SIZE - ITEM_RENDER_SIZE) / 2,
					Y,
					ITEM_RENDER_SIZE,
					ITEM_RENDER_SIZE
				)
			);
		}


		#endregion




	}
}