using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public class eTreeMaple : eTreeLeaf {
		protected override string LeafCode => "Leaf Maple";
	}
	public class eTreePine : eTreeLeaf {
		protected override string LeafCode => "Leaf Pine";
	}
	public class eTreePoplar : eTreeLeaf {
		protected override string LeafCode => "Leaf Poplar";
	}
	public class eTreePalm : eTreeLeaf {


		// Api
		protected override string LeafCode => "Leaf Palm";


		// MSG
		public override void FillPhysics () {
			Physics.FillBlock(YayaConst.LAYER_ENVIRONMENT, Rect, true, Const.ONEWAY_UP_TAG);
		}


		public override void FrameUpdate () {
            AngeliaFramework.Renderer.Draw(LeafArtworkCode, base.Rect.Shift(0, GetLeafShiftY(-24)));
		}


	}
	public class eTreeWillow : eTreeLeaf {


		// Api
		protected override string LeafCode => "Leaf Willow";

		// Data
		public override void FillPhysics () {
			Physics.FillBlock(
				YayaConst.LAYER_ENVIRONMENT,
				Rect.Shrink(0, 0, 0, Height / 2),
				true, YayaConst.CLIMB_TAG
			);
		}


		public override void FrameUpdate () {
            AngeliaFramework.Renderer.Draw(LeafArtworkCode, base.Rect.Shift(GetLeafShiftY(0), 0));
		}


	}


	[EntityAttribute.MapEditorGroup("Vegetation")]
	[EntityAttribute.EntityCapacity(256)]
	[EntityAttribute.EntityBounds(-Const.CELL_SIZE, -Const.CELL_SIZE, Const.CELL_SIZE * 3, Const.CELL_SIZE * 3)]
	[EntityAttribute.DrawBehind]
	public abstract class eTreeLeaf : Entity {




		#region --- VAR ---


		// Const
		private static readonly int[] LEAF_OFFSET_SEEDS = new int[] { 0, 6, 2, 8, 3, 7, 2, 3, 5, 2, 2, 6, 9, 3, 6, 1, 9, 0, 1, 7, 4, 2, 8, 4, 6, 5, 2, 4, 8, 7, };
		private const byte LEAF_HIDE_ALPHA = 42;

		// Virtual
		protected virtual string LeafCode => "Leaf";
		protected virtual int LeafCount => 3;
		protected virtual int LeafExpand => Const.CELL_SIZE / 3;
		protected int LeafArtworkCode { get; private set; } = 0;

		// Data
		private Vector2Int[] LeafOffsets = null;
		private Color32 LeafTint = new(255, 255, 255, 255);
		private bool CharacterNearby = false;


		#endregion




		#region --- MSG ---


		public override void OnInitialize (Game game) {
			base.OnInitialize(game);
			LeafOffsets = new Vector2Int[LeafCount];
		}


		public override void OnActived () {

			base.OnActived();
			Width = Const.CELL_SIZE;
			Height = Const.CELL_SIZE;

            // Leaf
            LeafArtworkCode = AngeliaFramework.Renderer.TryGetSpriteFromGroup(
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


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			CharacterNearby = Physics.HasEntity<eCharacter>(Rect.Expand(Const.CELL_SIZE), YayaConst.MASK_CHARACTER, null);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			// Leaf
			LeafTint.a = (byte)Mathf.Lerp(LeafTint.a, CharacterNearby ? LEAF_HIDE_ALPHA : 255, 0.1f);
			for (int i = 0; i < LeafOffsets.Length; i++) {
				var offset = LeafOffsets[i];
				DrawLeaf(offset, 12 * i, LeafExpand, offset.x % 2 == 0);
			}
			// Func
			void DrawLeaf (Vector2Int offset, int frameOffset, int expand, bool flipX = false) {
				var rect = Rect.Shift(offset.x, GetLeafShiftY(frameOffset) + offset.y).Expand(expand);
				if (flipX) {
					rect.x += rect.width;
					rect.width = -rect.width;
				}
                AngeliaFramework.Renderer.Draw(LeafArtworkCode, rect, LeafTint);
			}
		}


		#endregion




		#region --- API ---


		protected int GetLeafShiftY (int frameOffset = 0, int duration = 60, int amount = 12) {
			return ((Game.GlobalFrame + (X / Const.CELL_SIZE) + frameOffset).PingPong(duration) * amount / duration) - (amount / 2);
		}


		#endregion




	}
}