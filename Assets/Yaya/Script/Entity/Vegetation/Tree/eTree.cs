using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[MapEditorGroup("Vegetation")]
	[EntityCapacity(256)]
	[EntityBounds(-Const.CELL_SIZE, 0, Const.CELL_SIZE * 3, Const.CELL_SIZE + Const.CELL_SIZE / 2)]
	public abstract class eTree : eClimbable {




		#region --- VAR ---


		// Virtual
		protected abstract int TrunkBottomCode { get; }
		protected abstract int TrunkMidCode { get; }

		// Api
		public override RectInt Rect => new(X + Const.CELL_SIZE / 2 - TrunkWidth / 2, Y, Width, Height);
		public override bool CorrectPosition => true;
		protected int TrunkWidth { get; private set; } = 16;
		protected int TreesOnTop { get; private set; } = -1;
		protected bool HasTreesOnBottom { get; private set; } = false;
		protected bool IsBigTree => HasTreesOnBottom || TreesOnTop > 0;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			TrunkWidth = CellRenderer.TryGetSprite(TrunkBottomCode, out var rect) ? rect.GlobalWidth : Const.CELL_SIZE;
			Width = TrunkWidth;
			Height = Const.CELL_SIZE;
			TreesOnTop = -1;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			const int MAX_TALL = 7;
			if (TreesOnTop < 0) {
				var rect = new RectInt(Rect.x + Width / 2, 0, 1, 1);
				TreesOnTop = 0;
				// Top
				for (int i = 0; i < MAX_TALL; i++) {
					rect.y = Rect.yMax + Const.CELL_SIZE * i + Const.CELL_SIZE / 2;
					if (CellPhysics.HasEntity<eTree>(rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly, YayaConst.CLIMB_TAG)) {
						TreesOnTop = i + 1;
					} else break;
				}
				// Bottom
				rect.y = Rect.yMin - Const.CELL_SIZE / 2;
				HasTreesOnBottom = CellPhysics.HasEntity<eTree>(rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly, YayaConst.CLIMB_TAG);
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(
				HasTreesOnBottom ? TrunkMidCode : TrunkBottomCode,
				new(X + Const.CELL_SIZE / 2 - TrunkWidth / 2, Y, TrunkWidth, Const.CELL_SIZE)
			);
		}


		#endregion




	}
}