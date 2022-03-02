using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eConveyor : Entity {


		// SUB
		protected enum PartType {
			None = 0,
			LeftEdge = 1,
			Middle = 2,
			RightEdge = 3,
			Single = 4,
		}


		// Const
		private const PhysicsMask COL_MASK = PhysicsMask.Character | PhysicsMask.Environment | PhysicsMask.Item;

		// Api
		public override EntityLayer Layer => EntityLayer.Environment;
		public abstract int MovingSpeed { get; }
		protected PartType Part { get; private set; } = PartType.None;

		// Data
		private static readonly HitInfo[] c_CheckPart = new HitInfo[8];
		private static readonly HitInfo[] c_Update = new HitInfo[16];


		// MSG
		public override void OnCreate (int frame) {
			Part = PartType.None;
			base.OnCreate(frame);
		}


		public override void FillPhysics (int frame) {
			base.FillPhysics(frame);
			CellPhysics.FillEntity(PhysicsLayer.Environment, this);
		}


		public override void PhysicsUpdate (int frame) {
			base.PhysicsUpdate(frame);
			Update_Part();
			var rect = Rect;
			rect.y += rect.height;
			rect.height = 1;
			int count = CellPhysics.OverlapAll(c_Update, COL_MASK, rect, this);
			for (int i = 0; i < count; i++) {
				var hit = c_Update[i];
				if (hit.Entity is eRigidbody rig) {
					rig.PerformMove(MovingSpeed, 0, true, false);
					rig.Y = rect.yMax;
					rig.VelocityY = 0;
				}
			}
		}


		private void Update_Part () {
			if (Part != PartType.None) return;
			bool hasLeft = false;
			bool hasRight = false;
			var rect = Rect;
			int width = rect.width;
			rect.width = 1;
			rect.x -= 1;
			int count = CellPhysics.OverlapAll(c_CheckPart, PhysicsLayer.Environment, rect, this);
			for (int i = 0; i < count; i++) {
				if (c_CheckPart[i].Entity is eConveyor) {
					hasLeft = true;
					break;
				}
			}
			rect.x += width + 1;
			count = CellPhysics.OverlapAll(c_CheckPart, PhysicsMask.Environment, rect, this);
			for (int i = 0; i < count; i++) {
				if (c_CheckPart[i].Entity is eConveyor) {
					hasRight = true;
					break;
				}
			}
			Part =
				hasLeft && hasRight ? PartType.Middle :
				hasLeft && !hasRight ? PartType.RightEdge :
				!hasLeft && hasRight ? PartType.LeftEdge :
				PartType.Single;
		}


	}
}
