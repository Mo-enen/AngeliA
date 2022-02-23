using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class eOneway : Entity {


		// Api
		public override EntityLayer Layer => EntityLayer.Environment;
		protected virtual PhysicsMask Mask => PhysicsMask.Character | PhysicsMask.Environment | PhysicsMask.Item;
		public Direction4 GateDirection { get; set; } = Direction4.Up;


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
			foreach (var hit in CellPhysics.ForAllOverlaps(Mask, rect, this)) {
				if (hit.Entity is eRigidbody rig) {

					var rRect = rig.Rect;

					switch (GateDirection) {

						case Direction4.Left:
							if (
								rig.FinalVelocityX > 0 &&
								rRect.xMax - rig.FinalVelocityX <= rect.xMin &&
								rRect.xMax > rect.xMin
							) {
								rig.Move(rect.xMin - rig.Width - rig.OffsetX, rig.Y, int.MaxValue - 1);
								rig.VelocityX = 0;
							}
							break;

						case Direction4.Right:
							if (
								rig.FinalVelocityX < 0 &&
								rRect.xMin - rig.FinalVelocityX >= rect.xMax &&
								rRect.xMin < rect.xMax
							) {
								rig.Move(rect.xMax - rig.OffsetX, rig.Y, int.MaxValue - 1);
								rig.VelocityX = 0;
							}
							break;

						case Direction4.Down:
							if (
								rig.FinalVelocityY > 0 &&
								rRect.yMax - rig.FinalVelocityY <= rect.yMin &&
								rRect.yMax > rect.yMin
							) {
								rig.Move(rig.X, rect.yMin - rig.Height, int.MaxValue - 1);
								rig.VelocityY = 0;
							}
							break;

						case Direction4.Up:
							if (
								rig.FinalVelocityY < 0 &&
								rRect.yMin - rig.FinalVelocityY >= rect.yMax &&
								rRect.yMin < rect.yMax
							) {
								rig.Move(rig.X, rect.yMax, int.MaxValue - 1);
								rig.VelocityY = 0;
							}
							break;
					}
				}
			}
		}




	}
}
