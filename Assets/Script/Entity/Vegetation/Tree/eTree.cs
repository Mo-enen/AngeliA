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
		protected virtual int LeafCountMin => 2;
		protected virtual int LeafCountMax => 6;

		// Api
		public override RectInt Rect => new(X + Const.CELL_SIZE / 2 - TrunkWidth / 2, Y, Width, Height);
		public override int Layer => (int)EntityLayer.Environment;
		public override bool CorrectPosition => true;
		protected int Tall => Mathf.Abs(Data) + 1;
		protected bool HasBottom => Data >= 0;
		protected int TrunkWidth { get; private set; } = 16;
		protected int LeafSize { get; private set; } = 16;

		// Data
		private Vector2Int LeafShift = default;
		private Vector2Int LeafShiftAdd = default;
		private int LeafCount = 0;
		private int LeafCountAdd = 0;


		#endregion




		#region --- MSG ---



		public override void OnCreate (int frame) {
			base.OnCreate(frame);
			TrunkWidth = CellRenderer.GetUVRect(TrunkBottomCode, out var rect) ? rect.Width : Const.CELL_SIZE;
			LeafSize = CellRenderer.GetUVRect(GetLeafCode(0), out var lRect) ? lRect.Width : Const.CELL_SIZE;
			Width = TrunkWidth;
			Height = Const.CELL_SIZE * Tall;
			LeafShift.x = (X * 081620 / 1534 + Y * 040471 / 7354).AltMod(Const.CELL_SIZE / 2) + Const.CELL_SIZE / 4;
			LeafShift.y = (X * 142568 / 1543 + Y * 9364312 / 7206).AltMod(Const.CELL_SIZE / 2) + Const.CELL_SIZE / 4;
			LeafShiftAdd.x = (X * 081620 / 7321 + Y * 040471 / 1047).AltMod(Const.CELL_SIZE * 7 / 8) + Const.CELL_SIZE / 8;
			LeafShiftAdd.y = (X * 432846 / 1824 + Y * 172890 / 7835).AltMod(Const.CELL_SIZE * 7 / 8) + Const.CELL_SIZE / 8;
			LeafCount = (X * 040471 / 8376 + Y * 081620 / 1835).AltMod(LeafCountMax - LeafCountMin + 1) + LeafCountMin;
			LeafCountAdd = (X * 040471 / 1724 + Y * 081620 / 4842).AltMod(5) - 2;
		}


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			DrawTrunk();
			// Leaf
			int tall = Tall;
			int count = LeafCount;
			var shift = LeafShift;
			int leafCodeIndex = shift.x;
			for (int step = 0; step < tall; step++) {
				if (step == 0 && tall > 1) continue;
				for (int i = 0; i < count; i++) {
					DrawLeaf(frame, GetLeafCode(leafCodeIndex), step, shift);
					leafCodeIndex++;
					shift.x = (shift.x + LeafShiftAdd.x).AltMod(Const.CELL_SIZE);
					shift.y = (shift.y + LeafShiftAdd.y).AltMod(Const.CELL_SIZE);
					if (step == 0) shift.y = shift.y.AltMod(Const.CELL_SIZE / 2) + Const.CELL_SIZE / 2;
				}
				count = (count - 2 + LeafCountAdd).AltMod(LeafCountMax - LeafCountMin + 1) + LeafCountMin;
			}
		}


		private void DrawTrunk () {
			int tall = Tall;
			var rect = new RectInt(X + Const.CELL_SIZE / 2 - TrunkWidth / 2, Y, TrunkWidth, Const.CELL_SIZE);
			for (int step = 0; step < tall; step++) {
				rect.y = Y + step * Const.CELL_SIZE;
				CellRenderer.Draw(step == 0 && HasBottom ? TrunkBottomCode : TrunkMidCode, rect);
			}
		}


		protected virtual void DrawLeaf (int frame, int code, int step, Vector2Int shift) { }


		protected abstract int GetLeafCode (int index);


		#endregion




	}
}