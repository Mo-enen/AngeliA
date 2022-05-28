using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	[ExcludeInMapEditor]
	[EntityCapacity(4)]
	public abstract class eItem : Entity, IInitialize {




		#region --- VAR ---


		// Const
		private const int ITEM_PHYSICS_SIZE = Const.CELL_SIZE / 2;
		private const int ITEM_RENDER_SIZE = Const.CELL_SIZE * 2 / 3;

		// Api
		public int VelocityY { get; private set; } = 0;
		protected abstract int ItemCode { get; }

		// Data
		private static readonly HitInfo[] c_MakeRoom = new HitInfo[5];
		private static int Gravity = 5;
		private static int MaxGravitySpeed = 64;
		private bool MakingRoom = false;


		#endregion




		#region --- MSG ---


		public static void InitializeWithGame (Game game) {
			Gravity = game.PhysicsConfig.Gravity;
			MaxGravitySpeed = game.PhysicsConfig.MaxGravitySpeed;
		}


		public override void OnActived () {
			base.OnActived();
			Width = ITEM_PHYSICS_SIZE;
			Height = ITEM_PHYSICS_SIZE;
			MakingRoom = false;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.ITEM, this, true, YayaConst.ITEM_TAG);
		}


		public override void PhysicsUpdate () {
			int frame = Game.GlobalFrame;
			base.PhysicsUpdate();
			// Fall
			bool grounded = !CellPhysics.RoomCheck(YayaConst.MASK_MAP, Rect, this, Direction4.Down);
			if (!grounded) {
				if (VelocityY != 0) {
					var rect = Rect;
					rect.position = CellPhysics.Move(
						YayaConst.MASK_MAP, rect.position, 0, VelocityY, rect.size, this, out _, out bool stopY
					);
					Y = Mathf.Min(rect.y, Y);
					if (stopY) VelocityY = 0;
				}
				VelocityY = Mathf.Clamp(VelocityY - Gravity, -MaxGravitySpeed, 0);
			} else {
				VelocityY = 0;
			}
			// Make Room
			if (MakingRoom = MakingRoom || (
				frame % 30 == 0 &&
				CellPhysics.Overlap(YayaConst.MASK_ITEM, Rect, this, OperationMode.TriggerOnly)
			)) {
				int count = CellPhysics.OverlapAll(c_MakeRoom, YayaConst.MASK_ITEM, Rect, this, OperationMode.TriggerOnly);
				int deltaX = 0;
				for (int i = 0; i < count; i++) {
					deltaX += c_MakeRoom[i].Rect.x - X;
				}
				if (count > 0 && deltaX == 0) {
					deltaX = count % 2 == 0 ? 1 : -1;
				}
				var rect = Rect;
				rect.position = CellPhysics.MoveIgnoreOneway(
					YayaConst.MASK_MAP, rect.position,
					Mathf.Clamp(-deltaX, -6, 6), 0,
					rect.size, this
				);
				X = rect.x;
				Y = Mathf.Min(rect.y, Y);
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(
				ItemCode,
				new(
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
