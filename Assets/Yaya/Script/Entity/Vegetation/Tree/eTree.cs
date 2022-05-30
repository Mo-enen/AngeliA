using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[MapEditorGroup("Vegetation")]
	[EntityCapacity(256)]
	[EntityBounds(-Const.CELL_SIZE, -Const.CELL_SIZE, Const.CELL_SIZE * 3, Const.CELL_SIZE * 3)]
	public abstract class eTree : Entity {




		#region --- VAR ---


		// Const
		private static readonly int[] LEAF_OFFSET_SEEDS = new int[] { 0, 6, 2, 8, 3, 7, 2, 3, 5, 2, 2, 6, 9, 3, 6, 1, 9, 0, 1, 7, 4, 2, 8, 4, 6, 5, 2, 4, 8, 7, };

		// Virtual
		protected virtual string TrunkCode => "Trunk";
		protected virtual string LeafCode => "Leaf";
		protected virtual int LeafCount => 3;
		protected virtual int LeafExpand => Const.CELL_SIZE / 3;
		protected Direction3 Direction { get; private set; } = Direction3.None;
		protected int TrunkArtworkCode { get; private set; } = 0;
		protected int LeafArtworkCode { get; private set; } = 0;
		protected bool HasTrunkOnLeft { get; private set; } = false;
		protected bool HasTrunkOnRight { get; private set; } = false;
		protected bool HasTrunkOnBottom { get; private set; } = false;
		protected bool HasTrunkOnTop { get; private set; } = false;

		// Data
		private Vector2Int[] LeafOffsets = null;


		#endregion




		#region --- MSG ---


		public override void OnInitialize (Game game) {
			base.OnInitialize(game);
			LeafOffsets = new Vector2Int[LeafCount];
		}


		public override void OnActived () {

			base.OnActived();
			Direction = Direction3.None;
			Width = Const.CELL_SIZE;
			Height = Const.CELL_SIZE;

			// Trunk
			TrunkArtworkCode = CellRenderer.TryGetSpriteFromGroup(
				TrunkCode.AngeHash(), (X * 3 + Y * 11) / Const.CELL_SIZE, out var tSprite
			) ? tSprite.GlobalID : 0;

			// Leaf
			LeafArtworkCode = CellRenderer.TryGetSpriteFromGroup(
				LeafCode.AngeHash(), (X * 5 + Y * 7) / Const.CELL_SIZE, out var lSprite
			) ? lSprite.GlobalID : 0;

			// Offset
			int sLen = LEAF_OFFSET_SEEDS.Length;
			for (int i = 0; i < LeafOffsets.Length; i++) {
				int seedX = LEAF_OFFSET_SEEDS[(i + X / Const.CELL_SIZE) % sLen];
				int seedY = LEAF_OFFSET_SEEDS[(i + Y / Const.CELL_SIZE) % sLen];
				LeafOffsets[i] = new(
					((X * 137 * seedX + Y * 327 * seedY) / Const.CELL_SIZE).UMod(Const.CELL_SIZE) - Const.CELL_SIZE / 2,
					((X * 149 * seedX + Y * 177 * seedY) / Const.CELL_SIZE).UMod(Const.CELL_SIZE) - Const.CELL_SIZE / 2
				);
			}
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.ENVIRONMENT, this, true, Const.ONEWAY_UP_TAG);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (Direction == Direction3.None) Direction = GetDirection();
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawTrunks();
			DrawLeaf();
		}


		#endregion




		#region --- API ---


		protected virtual void DrawTrunks () {
			bool vertical = Direction == Direction3.Vertical;
			CellRenderer.Draw(
				TrunkArtworkCode,
				X, vertical ? Y : Y + Const.CELL_SIZE, 0, 0,
				vertical ? 0 : 90, Width, Height
			);
		}


		protected virtual void DrawLeaf () {
			// Leaf
			if (Direction != Direction3.Vertical || !HasTrunkOnTop) {
				for (int i = 0; i < LeafOffsets.Length; i++) {
					var offset = LeafOffsets[i];
					DrawLeaf(offset, 12 * i, LeafExpand, offset.x % 2 == 0);
				}
			}
			// Func
			void DrawLeaf (Vector2Int offset, int frameOffset, int expand, bool flipX = false) {
				var rect = Rect.Shift(offset.x, GetLeafShiftY(frameOffset) + offset.y).Expand(expand);
				if (flipX) {
					rect.x += rect.width;
					rect.width = -rect.width;
				}
				CellRenderer.Draw(LeafArtworkCode, rect);
			}
		}


		protected virtual Direction3 GetDirection () {
			HasTrunkOnLeft = false;
			HasTrunkOnRight = false;
			HasTrunkOnBottom = false;
			HasTrunkOnTop = false;
			int h = 0, v = 0;
			if (HasTrunkOnLeft = CellPhysics.HasEntity<eTree>(
				new(X - Const.CELL_SIZE / 2, Y + Const.CELL_SIZE / 2, 1, 1),
				YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly, Const.ONEWAY_UP_TAG
			)) h++;
			if (HasTrunkOnRight = CellPhysics.HasEntity<eTree>(
				new(X + Const.CELL_SIZE + Const.CELL_SIZE / 2, Y + Const.CELL_SIZE / 2, 1, 1),
				YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly, Const.ONEWAY_UP_TAG
			)) h++;
			if (HasTrunkOnBottom = CellPhysics.HasEntity<eTree>(
				new(X + Const.CELL_SIZE / 2, Y - Const.CELL_SIZE / 2, 1, 1),
				YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly, Const.ONEWAY_UP_TAG
			)) v++;
			if (HasTrunkOnTop = CellPhysics.HasEntity<eTree>(
				new(X + Const.CELL_SIZE / 2, Y + Const.CELL_SIZE + Const.CELL_SIZE / 2, 1, 1),
				YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly, Const.ONEWAY_UP_TAG
			)) v++;
			return h > v ? Direction3.Horizontal : Direction3.Vertical;
		}


		protected int GetLeafShiftY (int frameOffset = 0, int duration = 60, int amount = 12) =>
			(Game.GlobalFrame + X / Const.CELL_SIZE + frameOffset).PingPong(duration) * amount / duration - amount / 2;



		#endregion




	}
}