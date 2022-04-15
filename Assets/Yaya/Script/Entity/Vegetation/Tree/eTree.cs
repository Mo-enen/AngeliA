using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eTree : eClimbable {




		#region --- VAR ---


		// Virtual
		protected abstract int TrunkBottomCode { get; }
		protected abstract int TrunkMidCode { get; }

		// Api
		public override RectInt Rect => new(X + Const.CELL_SIZE / 2 - TrunkWidth / 2, Y, Width, Height);
		public override RectInt Bounds => Rect.Expand(Const.CELL_SIZE, Const.CELL_SIZE, 0, Const.CELL_SIZE / 2);
		public override bool CorrectPosition => true;
		protected int TrunkWidth { get; private set; } = 16;
		protected int TreesOnTop { get; private set; } = -1;
		protected bool HasTreesOnBottom { get; private set; } = false;
		protected bool IsBigTree => HasTreesOnBottom || TreesOnTop > 0;
		public override int Capacity => 256;

		// Data
		private static readonly HitInfo[] c_PoseCheck = new HitInfo[16];


		#endregion




		#region --- MSG ---


		public override void OnActived (int frame) {
			base.OnActived(frame);
			TrunkWidth = CellRenderer.GetSprite(TrunkBottomCode, out var rect) ? rect.GlobalWidth : Const.CELL_SIZE;
			Width = TrunkWidth;
			Height = Const.CELL_SIZE;
		}


		public override void PhysicsUpdate (int frame) {
			base.PhysicsUpdate(frame);
			const int MAX_TALL = 7;
			if (TreesOnTop < 0) {
				var rect = new RectInt(Rect.x, 0, Const.CELL_SIZE, Const.CELL_SIZE);
				TreesOnTop = 0;
				// Top
				for (int i = 0; i < MAX_TALL; i++) {
					rect.y = Rect.yMax + rect.height * i;
					int count = CellPhysics.OverlapAll(
						c_PoseCheck, (int)PhysicsMask.Environment,
						rect, this, OperationMode.TriggerOnly, YayaConst.CLIMB_TAG
					);
					for (int j = 0; j < count; j++) {
						if (c_PoseCheck[j].Entity is eTree) {
							TreesOnTop = i + 1;
							break;
						}
					}
					if (count == 0) break;
				}
				// Bottom
				{
					rect.y = Rect.yMin - rect.height;
					int count = CellPhysics.OverlapAll(
						c_PoseCheck, (int)PhysicsMask.Environment,
						rect, this, OperationMode.TriggerOnly, YayaConst.CLIMB_TAG
					);
					HasTreesOnBottom = false;
					for (int j = 0; j < count; j++) {
						if (c_PoseCheck[j].Entity is eTree) {
							HasTreesOnBottom = true;
							break;
						}
					}
				}
				c_PoseCheck.Dispose();
			}
		}


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			CellRenderer.Draw(
				HasTreesOnBottom ? TrunkMidCode : TrunkBottomCode,
				new(X + Const.CELL_SIZE / 2 - TrunkWidth / 2, Y, TrunkWidth, Const.CELL_SIZE)
			);
		}


		#endregion




	}
}