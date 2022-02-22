using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eOneway : Entity {


		private const int THICKNESS = Const.CELL_SIZE / 3;
		public override EntityLayer Layer => EntityLayer.Environment;


		private static readonly int ONEWAY_CODE = "Oneway".ACode();
		private static readonly PhysicsMask Mask = PhysicsMask.Character | PhysicsMask.Environment | PhysicsMask.Item;



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
			using var overlap = new CellPhysics.OverlapResultScope(Mask, rect, this);
			int count = overlap.Count;
			for (int i = 0; i < count; i++) {
				var hit = overlap.Results[i];
				if (
					hit.Entity is eRigidbody rig &&
					rig.FinalVelocityY < 0 &&
					rig.Rect.y - rig.FinalVelocityY >= rect.yMax &&
					rig.Rect.y < rect.yMax
				) {
					rig.Move(rig.X, rect.yMax, int.MaxValue - 1);
					rig.VelocityY = 0;
				}
			}
		}


		public override void FrameUpdate (int frame) => CellRenderer.Draw(ONEWAY_CODE, Rect);


	}
}
