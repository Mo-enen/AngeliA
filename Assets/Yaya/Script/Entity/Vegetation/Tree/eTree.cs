using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eTree : eClimbable {




		#region --- VAR ---


		// Const
		private static readonly int[] LEAF_SHIFT_PATTERN = new int[256] { 125, 35, 244, 61, 180, 216, 86, 204, 39, 4, 214, 71, 34, 118, 19, 173, 246, 19, 151, 117, 70, 207, 21, 115, 34, 110, 34, 238, 157, 164, 212, 243, 62, 55, 179, 125, 184, 67, 126, 64, 70, 19, 231, 159, 169, 98, 86, 75, 221, 52, 242, 65, 83, 241, 111, 174, 14, 128, 27, 70, 181, 104, 47, 130, 48, 226, 9, 235, 194, 149, 245, 179, 149, 87, 46, 50, 232, 117, 202, 191, 23, 215, 16, 104, 178, 146, 160, 76, 199, 5, 110, 55, 39, 56, 139, 222, 228, 100, 111, 199, 89, 107, 136, 72, 62, 62, 172, 252, 195, 60, 198, 153, 181, 92, 46, 221, 87, 198, 208, 157, 65, 188, 35, 189, 39, 190, 140, 93, 203, 80, 78, 131, 6, 3, 101, 172, 79, 200, 42, 115, 230, 164, 137, 139, 62, 213, 129, 202, 9, 173, 135, 29, 148, 209, 133, 156, 72, 203, 32, 128, 178, 79, 48, 114, 238, 66, 146, 178, 247, 130, 142, 142, 156, 92, 183, 157, 51, 152, 126, 81, 60, 193, 83, 30, 201, 48, 239, 52, 126, 200, 99, 132, 167, 169, 192, 151, 115, 22, 156, 251, 66, 207, 211, 134, 30, 248, 128, 55, 26, 232, 105, 176, 76, 207, 68, 241, 251, 18, 169, 189, 83, 94, 51, 46, 30, 10, 230, 243, 155, 32, 41, 28, 251, 130, 14, 108, 237, 204, 255, 208, 175, 183, 25, 149, 95, 179, 55, 216, 101, 207, 155, 97, 108, 222, 168, 228, };

		// Virtual
		protected abstract int TrunkBottomCode { get; }
		protected abstract int TrunkMidCode { get; }
		protected virtual int LeafCountMin => 2;
		protected virtual int LeafCountMax => 6;

		// Api
		public override RectInt Rect => new(X + Const.CELL_SIZE / 2 - TrunkWidth / 2, Y, Width, Height);
		public override int Layer => (int)EntityLayer.Environment;
		public override bool CorrectPosition => true;
		protected int TrunkWidth { get; private set; } = 16;
		protected int LeafSize { get; private set; } = 16;
		protected int TreesOnTop { get; private set; } = -1;
		protected bool HasTreesOnBottom { get; private set; } = false;

		// Data
		private static readonly HitInfo[] c_PoseCheck = new HitInfo[16];
		private Vector2Int LeafShift = default;
		private int LeafCount = 0;


		#endregion




		#region --- MSG ---


		public override void OnCreate (int frame) {
			base.OnCreate(frame);
			TrunkWidth = CellRenderer.GetAngeSprite(TrunkBottomCode, out var rect) ? rect.GlobalWidth : Const.CELL_SIZE;
			LeafSize = CellRenderer.GetAngeSprite(GetLeafCode(0), out var lRect) ? lRect.GlobalWidth : Const.CELL_SIZE;
			Width = TrunkWidth;
			Height = Const.CELL_SIZE;
			LeafShift.x = X.UMod(Const.CELL_SIZE);
			LeafShift.y = Y.UMod(Const.CELL_SIZE);
			LeafCount = (X * 040471 / 8376 + Y * 081620 / 1835).UMod(LeafCountMax - LeafCountMin + 1) + LeafCountMin;
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

			// Trunk
			CellRenderer.Draw(
				HasTreesOnBottom ? TrunkMidCode : TrunkBottomCode,
				new(X + Const.CELL_SIZE / 2 - TrunkWidth / 2, Y, TrunkWidth, Const.CELL_SIZE)
			);

			// Leaf
			int shiftIndexX = LeafShift.x;
			int shiftIndexY = LeafShift.y;
			var shift = new Vector2Int(
				LEAF_SHIFT_PATTERN[shiftIndexX.UMod(LEAF_SHIFT_PATTERN.Length)],
				LEAF_SHIFT_PATTERN[shiftIndexY.UMod(LEAF_SHIFT_PATTERN.Length)]
			);
			int leafCodeIndex = shift.x;

			for (int i = 0; i < LeafCount; i++) {
				DrawLeaf(frame, GetLeafCode(leafCodeIndex), shift);
				leafCodeIndex++;
				shiftIndexX++;
				shiftIndexY++;
				shift.x = (shift.x + LEAF_SHIFT_PATTERN[shiftIndexX.UMod(LEAF_SHIFT_PATTERN.Length)] * 6).UMod(Const.CELL_SIZE);
				shift.y = (shift.y + LEAF_SHIFT_PATTERN[shiftIndexY.UMod(LEAF_SHIFT_PATTERN.Length)]).UMod(Const.CELL_SIZE);
				if (!HasTreesOnBottom) shift.y = shift.y.UMod(Const.CELL_SIZE / 2) + Const.CELL_SIZE / 2;
			}

		}


		protected abstract void DrawLeaf (int frame, int code, Vector2Int shift);


		protected abstract int GetLeafCode (int index);


		#endregion




	}
}