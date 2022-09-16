using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelJelly;

namespace PixelJelly {
	public class FiddleLeaf : JellyBehaviour {



		// VAR
		public override string DisplayName => "Fiddle Leaf";
		public override string DisplayGroup => "Vegetation";
		public override int MaxFrameCount => 1;

		public override int MaxSpriteCount => 64;
		public int Thickness { get => m_Thickness; set => m_Thickness = value; }
		public int MinWidth { get => m_MinWidth; set => m_MinWidth = value; }
		public int MinHeight { get => m_MinHeight; set => m_MinHeight = value; }
		public Vector2Int LeafSize { get => m_LeafSize; set => m_LeafSize = value; }
		public Vector2Int LeafCount { get => m_LeafCount; set => m_LeafCount = value; }
		public Color32 Light { get => m_Light; set => m_Light = value; }
		public Color32 Normal { get => m_Normal; set => m_Normal = value; }
		public Color32 Dark { get => m_Dark; set => m_Dark = value; }


		// Ser
		[SerializeField, ArrowNumber(1)] int m_MinWidth = 24;
		[SerializeField, ArrowNumber(1)] int m_MinHeight = 12;
		[SerializeField, ArrowNumber(1)] int m_Thickness = 1;
		[SerializeField, MinMaxNumber(1, 128)] Vector2Int m_LeafSize = new Vector2Int(6, 12);
		[SerializeField, MinMaxNumber(1, 64)] Vector2Int m_LeafCount = new Vector2Int(2, 4);
		[SerializeField] Color32 m_Light = new Color32(83, 245, 113, 255);
		[SerializeField] Color32 m_Normal = new Color32(58, 166, 105, 255);
		[SerializeField] Color32 m_Dark = new Color32(60, 115, 96, 255);


		// MSG
		protected override void OnCreated () {
			Width = 32;
			Height = 16;
			SpriteCount = 10;
			FrameCount = 1;
		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2(width / 2f, 0);
			border = new RectOffset(0, 0, 0, 0);
			if (sprite == 0 && frame == 0) {
				if (m_Thickness >= width) {
					AddInspectorMessage("Thickness is too large.", MessageType.Warning, "m_Thickness");
				}
			}
			int realWidth = Random.Next(Mathf.Min(m_MinWidth, width), width + 1);
			int realHeight = Random.Next(Mathf.Min(m_MinHeight, height), height + 1);
			DrawFiddleLeaf(
				new RectInt((width - realWidth) / 2, 0, realWidth, realHeight),
				m_Thickness, m_LeafSize, Random.Next(m_LeafCount.x, m_LeafCount.y + 1),
				m_Light, m_Normal, m_Dark
			);
		}


		// API
		protected void DrawFiddleLeaf (RectInt rect, int thickness, Vector2Int leafSize, int leafCount, Color32 light, Color32 normal, Color32 dark) {
			int startXOffset = 0;
			bool right = Random.NextDouble() > 0.5f;
			int stepCount = Mathf.Max(rect.width / 2, rect.height / 2);
			int colorID = 0;
			float startX = rect.x + rect.width / 2f + Random.Next(-rect.height / 8, rect.height / 8 + 1);
			for (int i = 0; i < leafCount; i++) {
				int leafSizeX = Random.Next(leafSize.x, leafSize.y + 1);
				float leafGapY = Random.Next(2, 5);
				float topGap = Random.Next(0, (int)(rect.height * 0.4f));
				bool stemRight = i == 0 || i != 1 || (i > 1 && Random.NextDouble() > 0.5f);
				// Stem
				var stem0 = new Vector2(startX + (right ? startXOffset : -startXOffset), rect.y);
				var stem1 = new Vector2(stem0.x, rect.y + rect.height - topGap);
				var stem2 = new Vector2(stemRight ? rect.xMax - 1 : rect.x, stem1.y);
				var stem3 = new Vector2(stem2.x, stem0.y + leafGapY);
				colorID = (colorID + Random.Next(1, 3)) % 3;
				DrawBezier(
					stem0, stem1, stem2, stem3,
					stepCount, colorID == 0 ? light : colorID == 1 ? normal : dark,
					new Circle(thickness, new Vector2Int(thickness / 2, thickness / 2)),
					BlendMode.OneMinusAlpha
				);
				// Leaf
				var leafTop = Util.GetBezierPoint(stem0, stem1, stem2, stem3, 0.6666f);
				var leafMid = (leafTop + stem3) / 2f;
				leafMid.x += stemRight ? -leafSizeX : leafSizeX;
				int prevY = Util.GetBezierPoint(stem0, stem1, stem2, stem3, 0.6666f).RoundToInt().y;
				for (int j = 0; j < stepCount; j++) {
					var point = Util.GetBezierPoint(leafTop, leafMid, stem3, j / (stepCount - 1f)).RoundToInt();
					var stemPoint = Util.GetBezierPoint(stem0, stem1, stem2, stem3, Util.Remap(0f, stepCount - 1f, 0.6666f, 1f, j)).RoundToInt();
					int len = Mathf.Abs(stemPoint.x - point.x);
					int yMax = Mathf.Max(prevY, stemPoint.y);
					for (int y = Mathf.Min(prevY, stemPoint.y); y <= yMax; y++) {
						for (int x = 0; x <= len; x++) {
							SetColor(
								stemPoint.x + (stemRight ? -x : x),
								y,
								x <= len / 2 ?
									(colorID == 2 ? normal : light) :
									(colorID == 2 ? dark : normal),
								BlendMode.OneMinusAlpha
							);
						}
					}
					prevY = stemPoint.y;
				}
				// Final
				if (i % 2 == 0) {
					startXOffset += Random.Next(1, 3);
				}
				right = !right;
			}
		}



	}
}
