using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eConveyor : Entity {


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
		private static readonly int[] MID_CODES = new int[8] { "Conveyor Mid 0".AngeHash(), "Conveyor Mid 1".AngeHash(), "Conveyor Mid 2".AngeHash(), "Conveyor Mid 3".AngeHash(), "Conveyor Mid 4".AngeHash(), "Conveyor Mid 5".AngeHash(), "Conveyor Mid 6".AngeHash(), "Conveyor Mid 7".AngeHash(), };
		private static readonly int[] LEFT_CODES = new int[8] { "Conveyor Left 0".AngeHash(), "Conveyor Left 1".AngeHash(), "Conveyor Left 2".AngeHash(), "Conveyor Left 3".AngeHash(), "Conveyor Left 4".AngeHash(), "Conveyor Left 5".AngeHash(), "Conveyor Left 6".AngeHash(), "Conveyor Left 7".AngeHash(), };
		private static readonly int[] RIGHT_CODES = new int[8] { "Conveyor Right 0".AngeHash(), "Conveyor Right 1".AngeHash(), "Conveyor Right 2".AngeHash(), "Conveyor Right 3".AngeHash(), "Conveyor Right 4".AngeHash(), "Conveyor Right 5".AngeHash(), "Conveyor Right 6".AngeHash(), "Conveyor Right 7".AngeHash(), };
		private static readonly int[] SINGLE_CODES = new int[8] { "Conveyor Single 0".AngeHash(), "Conveyor Single 1".AngeHash(), "Conveyor Single 2".AngeHash(), "Conveyor Single 3".AngeHash(), "Conveyor Single 4".AngeHash(), "Conveyor Single 5".AngeHash(), "Conveyor Single 6".AngeHash(), "Conveyor Single 7".AngeHash(), };

		// Api
		public override EntityLayer Layer => EntityLayer.Environment;
		protected PartType Part { get; private set; } = PartType.None;
		public int MoveSpeed => Data;

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
					rig.PerformMove(MoveSpeed, 0, true, false);
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


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			int aFrame = (frame * Mathf.Abs(MoveSpeed) / 16) % 8;
			if (MoveSpeed > 0) aFrame = 7 - aFrame;
			switch (Part) {
				case PartType.LeftEdge:
					CellRenderer.Draw(LEFT_CODES[aFrame], Rect);
					break;
				case PartType.Middle:
					CellRenderer.Draw(MID_CODES[aFrame], Rect);
					break;
				case PartType.RightEdge:
					CellRenderer.Draw(RIGHT_CODES[aFrame], Rect);
					break;
				case PartType.Single:
					CellRenderer.Draw(SINGLE_CODES[aFrame], Rect);
					break;
			}
		}


	}
}
