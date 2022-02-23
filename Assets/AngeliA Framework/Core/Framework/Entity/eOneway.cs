using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class eOneway : Entity {


		// Api
		public override EntityLayer Layer => EntityLayer.Environment;
		protected virtual PhysicsMask Mask => PhysicsMask.Character | PhysicsMask.Environment | PhysicsMask.Item;
		public abstract Direction4 GateDirection { get; }
		protected int LastReboundFrame { get; private set; } = int.MinValue;

		// Data
		private static readonly HitInfo[] c_PhysicsUpdate = new HitInfo[16];
		private bool PrevFrameRebound = false;


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
			using var overlap = new CellPhysics.OverlapAllScope(c_PhysicsUpdate, Mask, rect, this);
			bool rebound = false;
			for (int i = 0; i < overlap.Count; i++) {
				var hit = c_PhysicsUpdate[i];
				if (hit.Entity is eRigidbody rig) {
					var rRect = rig.Rect;
					switch (GateDirection) {
						case Direction4.Left:
							if (
								rig.FinalVelocityX > 0 &&
								rRect.xMax - rig.FinalVelocityX <= rect.xMin &&
								rRect.xMax > rect.xMin
							) {
								rig.Move(rect.xMin - rig.Width - rig.OffsetX, rig.Y, int.MaxValue);
								rig.DisableTopCarryUntil(frame + 1);
								rig.VelocityX = 0;
								if (!PrevFrameRebound) {
									LastReboundFrame = frame;
								}
								rebound = true;
							}
							break;
						case Direction4.Right:
							if (
								rig.FinalVelocityX < 0 &&
								rRect.xMin - rig.FinalVelocityX >= rect.xMax &&
								rRect.xMin < rect.xMax
							) {
								rig.Move(rect.xMax - rig.OffsetX, rig.Y, int.MaxValue);
								rig.DisableTopCarryUntil(frame + 1);
								rig.VelocityX = 0;
								if (!PrevFrameRebound) {
									LastReboundFrame = frame;
								}
								rebound = true;
							}
							break;
						case Direction4.Down:
							if (
								rig.FinalVelocityY > 0 &&
								rRect.yMax - rig.FinalVelocityY <= rect.yMin &&
								rRect.yMax > rect.yMin
							) {
								rig.Move(rig.X, rect.yMin - rig.Height, int.MaxValue);
								rig.VelocityY = 0;
								if (!PrevFrameRebound) {
									LastReboundFrame = frame;
								}
								rebound = true;
							}
							break;
						case Direction4.Up:
							if (
								rig.FinalVelocityY < 0 &&
								rRect.yMin - rig.FinalVelocityY >= rect.yMax &&
								rRect.yMin < rect.yMax
							) {
								rig.Move(rig.X, rect.yMax, int.MaxValue);
								rig.VelocityY = 0;
								if (!PrevFrameRebound) {
									LastReboundFrame = frame;
								}
								rebound = true;
							}
							break;
					}
				}
			}
			PrevFrameRebound = rebound;
		}


	}
}
