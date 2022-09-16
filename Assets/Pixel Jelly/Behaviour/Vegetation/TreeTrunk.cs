using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PixelJelly {
	public class TreeTrunk : JellyBehaviour {



		// Api
		public override string DisplayName => "Tree Trunk";
		public override string DisplayGroup => "Vegetation";
		public override int MaxFrameCount => 1;
		public override int MaxSpriteCount => 64;
		public LightDirection2 LightDirection { get => m_LightDirection; set => m_LightDirection = value; }
		public int MinHeight { get => m_MinHeight; set => m_MinHeight = value; }
		public Vector2Int BranchCount { get => m_BranchCount; set => m_BranchCount = value; }
		public Vector2Int PatternCount { get => m_PatternCount; set => m_PatternCount = value; }
		public PixelSprite[] Trunks { get => m_Trunks; set => m_Trunks = value; }
		public PixelSprite[] Branchs { get => m_Branchs; set => m_Branchs = value; }
		public PixelSprite[] Patterns { get => m_Patterns; set => m_Patterns = value; }

		// Ser
		[SerializeField, ArrowNumber(true)] LightDirection2 m_LightDirection = LightDirection2.Right;
		[SerializeField, ArrowNumber(0, 128)] int m_MinHeight = 24;
		[SerializeField, MinMaxSlider(0f, 1f)] Vector2 m_BranchRange = new Vector2(0.1f, 0.9f);
		[SerializeField, MinMaxNumber(0, 128)] Vector2Int m_BranchCount = new Vector2Int(0, 2);
		[SerializeField, MinMaxNumber(0, 128)] Vector2Int m_PatternCount = new Vector2Int(1, 5);
		[SerializeField, PixelEditor] PixelSprite[] m_Trunks = new PixelSprite[0];
		[SerializeField, PixelEditor] PixelSprite[] m_Branchs = new PixelSprite[0];
		[SerializeField, PixelEditor] PixelSprite[] m_Patterns = new PixelSprite[0];


		// MSG
		protected override void OnCreated () {
			Width = 16;
			Height = 32;
			FrameCount = 1;
			SpriteCount = 6;
			m_Trunks = new PixelSprite[] {
				new PixelSprite("5_5_2_0_2_2_2_2_2354464511_3213188351_3213188351_3213188351_3904401407_2354464511_2354464511_3213188351_3904401407_3904401407_0_2354464511_3213188351_3904401407_0_0_3213188351_3213188351_3904401407_0_0_0_3904401407_0_0_"),
				new PixelSprite("5_4_2_0_2_2_1_2_2354464511_3213188351_3213188351_3213188351_3904401407_0_2354464511_3213188351_3213188351_3904401407_0_3213188351_3213188351_3904401407_0_0_0_3904401407_0_0_"),
				new PixelSprite("5_5_2_0_2_2_2_2_3213188351_2354464511_3213188351_3213188351_3904401407_2354464511_3213188351_3213188351_3213188351_3904401407_0_2354464511_3213188351_3904401407_0_0_3213188351_3213188351_3904401407_0_0_0_3904401407_0_0_"),

			};
			m_Branchs = new PixelSprite[] {
				new PixelSprite("6_6_0_0_0_5_0_5_3213188351_2354464511_2354464511_0_0_0_3213188351_3213188351_3213188351_2354464511_0_0_0_0_3904401407_3213188351_3213188351_3213188351_0_0_0_3213188351_3213188351_0_0_0_0_3213188351_0_0_0_0_0_3904401407_0_0_"),
				new PixelSprite("5_3_0_0_0_4_0_2_3213188351_2354464511_2354464511_0_0_3213188351_2354464511_3213188351_3213188351_0_0_0_3904401407_3904401407_3904401407_"),
				new PixelSprite("4_2_0_0_0_3_0_1_3213188351_2354464511_0_0_3213188351_3213188351_3904401407_3904401407_"),
				new PixelSprite("3_2_0_0_0_2_0_1_3213188351_3213188351_0_0_3213188351_3904401407_"),
				new PixelSprite("3_2_0_0_0_2_0_1_3213188351_0_0_3904401407_3213188351_3213188351_"),
				new PixelSprite("3_2_0_0_0_2_0_1_3213188351_2354464511_3213188351_0_0_3904401407_"),

			};
			m_Patterns = new PixelSprite[] {
				new PixelSprite("3_3_0_0_1_1_1_1_2354464511_2354464511_3213188351_2354464511_3213188351_3904401407_3213188351_3904401407_3904401407_"),
				new PixelSprite("2_2_0_0_0_0_0_0_2354464511_3904401407_3904401407_3904401407_"),
				new PixelSprite("2_3_0_0_0_0_0_0_3904401407_2354464511_3213188351_3904401407_2354464511_3213188351_"),
				new PixelSprite("1_3_0_0_0_0_0_0_2354464511_3904401407_3904401407_"),

			};
		}


		protected override void BeforePopulateAllPixels () {
			FrameCount = 1;
		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2(width / 2f, 0);
			border = new RectOffset(0, 0, 0, 0);
			int realHeight = Random.Next(Mathf.Min(m_MinHeight, height), height);
			PopulateTrunk(
				new RectInt(0, 0, width, realHeight),
				m_BranchRange,
				m_BranchCount.RandomBetween(Random),
				m_PatternCount.RandomBetween(Random),
				m_LightDirection == LightDirection2.Right,
				m_Trunks.RandomBetween(Random), m_Branchs, m_Patterns
			);

		}


		// LGC
		private void PopulateTrunk (RectInt rect, Vector2 branchRange, int branchCount, int patternCount, bool lightR, PixelSprite trunk, PixelSprite[] branchs, PixelSprite[] patterns) {
			if (trunk == null) { return; }
			using var _ = new LayerScope();
			// Main
			int trunkSize = trunk.Width;
			var mainRect = new RectInt(rect.x + (rect.width - trunkSize) / 2, rect.y, trunkSize, rect.height);
			DrawTrunk(mainRect, lightR, trunk);
			// Pattern
			if (patternCount > 0 && patterns != null && patterns.Length > 0) {
				using (new LockClearScope()) {
					for (int pIndex = 0; pIndex < patternCount; pIndex++) {
						var pat = patterns.RandomBetween(Random);
						var pRect = new RectInt(
							Random.NextClamped(mainRect.xMin, mainRect.xMax - pat.Width),
							Random.NextClamped(mainRect.yMin, mainRect.yMax - pat.Height),
							pat.Width, pat.Height
						);
						pRect.Clamp(mainRect);
						DrawPattern(pRect, lightR, pat);
					}
				}
			}
			// Branch
			bool allowRight = rect.xMax > mainRect.xMax;
			bool allowLeft = rect.xMin < mainRect.xMin;
			if (branchs != null && branchs.Length > 0 && (allowRight || allowLeft)) {
				for (int bIndex = 0; bIndex < branchCount; bIndex++) {
					var branch = branchs.RandomBetween(Random);
					if (branch == null) { continue; }
					bool right = allowRight && allowLeft ? Random.NextDouble() > 0.5f : allowRight;
					int l = right ? mainRect.xMax - 2 : mainRect.xMin + 1 - branch.Width;
					int r = right ? mainRect.xMax - 2 + branch.Width : mainRect.xMin + 1;
					l = Mathf.Clamp(l, rect.xMin, rect.xMax);
					r = Mathf.Clamp(r, rect.xMin, rect.xMax);
					DrawBranch(new RectInt(
							l,
							Random.NextClamped(
								Util.RemapRounded(0f, 1f, mainRect.yMin, mainRect.yMax, branchRange.x),
								Util.RemapRounded(0f, 1f, mainRect.yMin, mainRect.yMax, branchRange.y)
							),
							r - l + 1,
							branch.Height
						), right, branch
					);
				}
			}
		}


		protected virtual void DrawTrunk (RectInt mainRect, bool lightR, PixelSprite trunk) => DrawSprite(mainRect, trunk, SpriteScaleMode.Tile, BlendMode.OneMinusAlpha, !lightR, false);


		protected virtual void DrawBranch (RectInt rect, bool right, PixelSprite branch) => DrawSprite(rect, branch, SpriteScaleMode.Slice, BlendMode.OneMinusAlpha, false, !right);


		protected virtual void DrawPattern (RectInt rect, bool lightR, PixelSprite pattern) => DrawSprite(rect, pattern, SpriteScaleMode.Slice, BlendMode.OneMinusAlpha, false, !lightR);


	}
}
