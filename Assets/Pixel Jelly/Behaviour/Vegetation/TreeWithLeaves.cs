using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelJelly;



namespace PixelJelly {
	public class TreeWithLeaves : TreeTrunk {



		// VAR
		public override string DisplayName => "Trees";
		public override string DisplayGroup => "Vegetation";
		public override int MaxFrameCount => 1;
		public override int MaxSpriteCount => 64;
		public SpriteScaleMode ScaleMode { get => m_ScaleMode; set => m_ScaleMode = value; }

		// Ser
		[SerializeField, ArrowNumber(true)] SpriteScaleMode m_ScaleMode = SpriteScaleMode.Original;
		[SerializeField, MinMaxNumber(1, 128)] Vector2Int m_LeafCount = new Vector2Int(14, 17);
		[SerializeField, MinMaxNumber(0, 128)] Vector2Int m_LeafWidth = new Vector2Int(4, 6);
		[SerializeField, MinMaxNumber(0, 128)] Vector2Int m_LeafHeight = new Vector2Int(4, 6);
		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] Vector2 m_LeafAmount = new Vector2(1f, 1f);
		[SerializeField] AnimationCurve m_LeafRange = new AnimationCurve();
		[SerializeField, PixelEditor] PixelSprite[] m_Leaves = new PixelSprite[0];

		// Data
		private RectInt TrunkRect = default;


		// MSG
		protected override void OnCreated () {
			base.OnCreated();
			m_LeafRange = AnimationCurve.EaseInOut(0.25f, 1f, 1f, 0f);
			m_Leaves = new PixelSprite[] {
				new PixelSprite("5_5_2_2_1_1_1_1_0_997408255_997408255_997408255_0_997408255_1369848575_997408255_1369848575_2549437439_997408255_997408255_1369848575_2549437439_2549437439_997408255_1369848575_2549437439_1369848575_2549437439_0_2549437439_2549437439_2549437439_0_"),
				new PixelSprite("5_5_2_2_1_1_1_1_0_997408255_1369848575_997408255_0_997408255_997408255_1369848575_1369848575_2549437439_997408255_1369848575_2549437439_1369848575_2549437439_997408255_2549437439_1369848575_2549437439_1369848575_0_2549437439_2549437439_1369848575_0_"),
				new PixelSprite("7_7_3_3_1_1_1_1_0_997408255_997408255_997408255_997408255_997408255_0_997408255_1369848575_997408255_1369848575_997408255_1369848575_2549437439_997408255_997408255_1369848575_997408255_1369848575_2549437439_2549437439_997408255_1369848575_1369848575_1369848575_1369848575_1369848575_2549437439_997408255_997408255_1369848575_2549437439_1369848575_2549437439_2549437439_997408255_1369848575_2549437439_1369848575_2549437439_1369848575_2549437439_0_2549437439_2549437439_2549437439_2549437439_2549437439_0_"),
				new PixelSprite("9_9_4_4_1_1_1_1_0_997408255_997408255_997408255_997408255_997408255_997408255_997408255_0_997408255_1369848575_997408255_1369848575_997408255_1369848575_1369848575_1369848575_2549437439_997408255_997408255_1369848575_997408255_1369848575_997408255_1369848575_1369848575_2549437439_997408255_1369848575_997408255_1369848575_1369848575_1369848575_2549437439_1369848575_2549437439_997408255_997408255_1369848575_1369848575_1369848575_1369848575_1369848575_2549437439_2549437439_997408255_1369848575_1369848575_1369848575_2549437439_1369848575_2549437439_1369848575_2549437439_997408255_1369848575_1369848575_1369848575_1369848575_2549437439_1369848575_2549437439_2549437439_997408255_1369848575_2549437439_1369848575_2549437439_1369848575_2549437439_1369848575_2549437439_0_2549437439_2549437439_2549437439_2549437439_2549437439_2549437439_2549437439_0_"),

			};
		}


		protected override void OnLoaded () {
			base.OnLoaded();
		}


		protected override void BeforePopulateAllPixels () {
			base.BeforePopulateAllPixels();

		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			TrunkRect = new RectInt(0, 0, width, height);
			// Trunk
			base.OnPopulatePixels(width, height, frame, frameCount, sprite, spriteCount, out pivot, out border);
			// Leaf
			DrawLeaves(
				new RectInt(0, 0, width, TrunkRect.height),
				m_LeafCount.RandomBetween(Random),
				m_LeafWidth, m_LeafHeight, m_LeafAmount,
				LightDirection == LightDirection2.Right,
				m_LeafRange, m_ScaleMode, m_Leaves
			);
		}


		protected virtual void DrawLeaves (RectInt rect, int leafCount, Vector2Int leafWidth, Vector2Int leafHeight, Vector2 leafAmount, bool lightR, AnimationCurve leafRange, SpriteScaleMode scaleMode, PixelSprite[] leaves) {
			if (leaves == null || leaves.Length == 0 || leafRange == null || leafRange.length == 0) { return; }
			float leafBottom01 = leafRange[0].time;
			float leafTop01 = leafRange[leafRange.length - 1].time;
			for (int leafIndex = 0; leafIndex < leafCount; leafIndex++) {
				var leaf = leaves.RandomBetween(Random);
				if (leaf == null || leaf.Width * leaf.Height == 0) { continue; }
				float y01 = leafIndex == 0 ? 1f : Mathf.Lerp(leafBottom01, leafTop01, Random.NextFloat());
				float hWidth01 = leafRange.Evaluate(y01) / 2f;
				float x01 = Mathf.Lerp(0.5f - hWidth01, 0.5f + hWidth01, Random.NextFloat());
				RectInt lRect;
				if (scaleMode == SpriteScaleMode.Original) {
					// Point
					lRect = new RectInt(
						Util.RemapRounded(0f, 1f, rect.xMin + leaf.PivotX, rect.xMax - leaf.Width + leaf.PivotX, x01),
						Util.RemapRounded(0f, 1f, rect.yMin + leaf.PivotY, rect.yMax - leaf.Height + leaf.PivotY, y01),
						leaf.Width, leaf.Height
					);
				} else {
					// Rect
					int lWidth = leafWidth.RandomBetween(Random);
					int lHeight = leafHeight.RandomBetween(Random);
					lRect = new RectInt(
						Util.RemapRounded(0f, 1f, rect.xMin, rect.xMax - lWidth, x01),
						Util.RemapRounded(0f, 1f, rect.yMin, rect.yMax - lHeight, y01),
						lWidth, lHeight
					);
				}
				float amount = leafAmount.RandomBetween(Random);
				if (amount.Almost(1f) || amount > 1f) {
					DrawLeafBlock();
				} else if (amount.NotAlmostZero() && amount > 0f) {
					using var _ = new LayerScope();
					DrawLeafBlock();
					int offsetX = scaleMode == SpriteScaleMode.Original ? -leaf.PivotX : 0;
					int offsetY = scaleMode == SpriteScaleMode.Original ? -leaf.PivotY : 0;
					for (int j = lRect.yMin; j < lRect.yMax; j++) {
						for (int i = lRect.xMin; i < lRect.xMax; i++) {
							if (Random.NextDouble() > amount) {
								SetColor(i + offsetX, j + offsetY, Color.clear);
							}
						}
					}

				}
				// Func
				void DrawLeafBlock () => DrawSprite(lRect, leaf, scaleMode, BlendMode.OneMinusAlpha, false, !lightR);
			}
		}


		protected override void DrawTrunk (RectInt mainRect, bool lightR, PixelSprite trunk) {
			base.DrawTrunk(mainRect, lightR, trunk);
			TrunkRect = mainRect;
		}


	}
}
