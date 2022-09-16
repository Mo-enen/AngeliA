using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PixelJelly {
	public class Paspalum : JellyBehaviour {



		// Api
		public override string DisplayName => "Paspalum";
		public override string DisplayGroup => "Vegetation";
		public override int MaxFrameCount => 1;

		public override int MaxSpriteCount => 64;
		public LightDirection2 LightDirection { get => m_LightDirection; set => m_LightDirection = value; }
		public float LightAmount { get => m_LightAmount; set => m_LightAmount = value; }
		public Vector2Int LeafSize { get => m_LeafSize; set => m_LeafSize = value; }
		public Vector2Int LeafLength { get => m_LeafLength; set => m_LeafLength = value; }
		public Vector2Int LeafCount { get => m_LeafCount; set => m_LeafCount = value; }
		public Color32 Light { get => m_Light; set => m_Light = value; }
		public Color32 Normal { get => m_Normal; set => m_Normal = value; }
		public Color32 Dark { get => m_Dark; set => m_Dark = value; }
		public Vector2Int EarHeight { get => m_EarHeight; set => m_EarHeight = value; }
		public Vector2Int EarStemWidth { get => m_EarStemWidth; set => m_EarStemWidth = value; }
		public Vector2Int EarOffsetY { get => m_EarOffsetY; set => m_EarOffsetY = value; }
		public Vector2Int EarOffsetX { get => m_EarOffsetX; set => m_EarOffsetX = value; }
		public PixelSprite[] Ear { get => m_Ear; set => m_Ear = value; }

		// Ser
		[SerializeField, ArrowNumber(true)] private LightDirection2 m_LightDirection = LightDirection2.Right;
		[SerializeField, ArrowNumber(0f, 1f, 0.1f)] private float m_LightAmount = 0.5f;
		[SerializeField, MinMaxNumber(1, int.MaxValue)] private Vector2Int m_LeafSize = new Vector2Int(2, 4);
		[SerializeField, MinMaxNumber(1, int.MaxValue)] private Vector2Int m_LeafLength = new Vector2Int(6, 36);
		[SerializeField, MinMaxNumber(1, int.MaxValue)] private Vector2Int m_LeafCount = new Vector2Int(3, 8);
		[SerializeField] Color32 m_Light = new Color32(83, 245, 113, 255);
		[SerializeField] Color32 m_Normal = new Color32(58, 166, 105, 255);
		[SerializeField] Color32 m_Dark = new Color32(60, 115, 96, 255);
		[Space]
		[SerializeField, ArrowNumber(0f, 1f, 0.1f)] float m_EarAmount = 0.6f;
		[SerializeField, MinMaxNumber(0)] Vector2Int m_EarStemWidth = new Vector2Int(1, 1);
		[SerializeField, MinMaxNumber(int.MinValue)] Vector2Int m_EarOffsetX = new Vector2Int(-2, 2);
		[SerializeField, MinMaxNumber(int.MinValue)] Vector2Int m_EarOffsetY = new Vector2Int(-12, 0);
		[SerializeField, MinMaxNumber(0)] Vector2Int m_EarHeight = new Vector2Int(7, 13);
		[SerializeField, PixelEditor] PixelSprite[] m_Ear = new PixelSprite[0];



		// MSG
		protected override void OnCreated () {
			Width = 24;
			Height = 32;
			FrameCount = 1;
			SpriteCount = 20;
			m_Ear = new PixelSprite[] {
				new PixelSprite("3_3_1_0_0_0_0_1_2354464511_3213188351_2354464511_3213188351_2354464511_3213188351_0_3213188351_0_"),
				new PixelSprite("4_3_1_0_0_0_0_1_2354464511_3213188351_2354464511_3213188351_3213188351_2354464511_3213188351_2354464511_0_3213188351_3213188351_0_"),
				new PixelSprite("5_4_1_0_0_0_0_2_2354464511_3213188351_2354464511_3213188351_2354464511_3213188351_2354464511_3213188351_2354464511_3213188351_0_3213188351_3213188351_3213188351_0_0_0_3213188351_0_0_"),
			};
		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2(width / 2f, 0);
			border = new RectOffset(0, 0, 0, 0);
			if (sprite == 0 && frame == 0) {
				if (m_LeafSize.x >= width) {
					AddInspectorMessage("Size too large.", MessageType.Warning, "m_Size");
				}
				if (m_LeafLength.x > height * 2) {
					AddInspectorMessage("Length too large.", MessageType.Warning, "m_Length");
				}
			}
			DrawPaspalum(
				new RectInt(0, 0, width, height),
				m_LightDirection == LightDirection2.Right, m_LightAmount, m_LeafSize, m_LeafLength, m_LeafCount.RandomBetween(Random),
				Random.NextFloat() < m_EarAmount, m_EarHeight.RandomBetween(Random), m_EarStemWidth.RandomBetween(Random),
				m_EarOffsetX.RandomBetween(Random), m_EarOffsetY.RandomBetween(Random),
				m_Light, m_Dark, m_Normal, m_Ear != null && m_Ear.Length > 0 ? m_Ear[Random.Next(0, m_Ear.Length)] : null
			);
		}


		protected void DrawPaspalum (
			RectInt rect, bool lightR, float lightAmount,
			Vector2Int sizeRange, Vector2Int lengthRange, int leafCount,
			bool hasEar, int earHeight, int earStemWidth, int earOffsetX, int earOffsetY,
			Color32 light, Color32 dark, Color32 normal, PixelSprite ear
		) {
			for (int leafIndex = 0; leafIndex < leafCount; leafIndex++) {
				int size = sizeRange.RandomBetween(Random);
				int length = lengthRange.RandomBetween(Random);
				int stepCount = length;
				// Ear
				if (hasEar && ear != null && leafIndex == leafCount / 2) {
					// Stem
					int stemX = rect.x + rect.width / 2 - earStemWidth / 2;
					int stemEndX = stemX + earOffsetX;
					int earY = rect.yMax - earHeight + earOffsetY;
					var stem0 = new Vector2(stemX, rect.y);
					var stem3 = new Vector2(stemEndX, earY);
					var stem1 = Vector2.Lerp(stem0, stem3, 1f / 3f);
					var stem2 = Vector2.Lerp(stem0, stem3, 2f / 3f);
					stem1.x = stemX;
					stem2.x = stemEndX;
					DrawBezier(stem0, stem1, stem2, stem3, stepCount, normal, new Circle(earStemWidth, new Vector2Int(earStemWidth / 2, earStemWidth / 2)), BlendMode.OneMinusAlpha);
					// Ear
					DrawSprite(new RectInt(
						stemEndX - ear.Width / 2, earY, ear.Width, earHeight),
						ear, SpriteScaleMode.Tile
					);
				}
				// Leaf
				int leafHeight = Random.Next(rect.height / 2, rect.height + 1);
				int randomGap = Mathf.Min(rect.width, leafHeight) / 8;
				var p0 = new Vector2(rect.x + rect.width / 2f, rect.y);
				var p3 = new Vector2(Random.NextClamped(rect.xMin + size / 2, rect.xMax - size / 2), rect.y + length);
				var p1 = Vector2.Lerp(p0, p3, 1f / 3f);
				var p2 = Vector2.Lerp(p0, p3, 2f / 3f);
				p1.x = p0.x + Random.Next(-randomGap, randomGap + 1);
				p2.x = (p0.x + p3.x) / 2f + Random.Next(-randomGap, randomGap + 1);
				p1.y += Random.Next(-randomGap, randomGap + 1);
				p2.y += Random.Next(-randomGap, randomGap + 1);
				if (length > leafHeight) {
					p3.y = rect.yMax - length + leafHeight;
				}
				var min = rect.min;
				var max = rect.max - new Vector2Int(1, 1);
				p0.Clamp(min, max);
				p1.Clamp(min, max);
				p2.Clamp(min, max);
				p3.Clamp(min, max);
				float sizeTurning = Random.NextFloat(0.618f, 0.9f);
				// Dark
				Layout_Bezier(p0, p1, p2, p3, stepCount, (_pointA, _pointB, _step) => {
					DrawLine(_pointA, _pointB, dark, new Rectangle(GetSize(_step), 1));
				});
				using var __ = new LockClearScope();
				// Normal
				Layout_Bezier(p0, p1, p2, p3, stepCount, (_pointA, _pointB, _step) => {
					int _size = GetSize(_step);
					int shift = Random.NextClamped(1, _size / 2 + 1);
					if (!lightR) {
						shift *= -1;
					}
					_pointA.x += shift;
					_pointB.x += shift;
					DrawLine(_pointA, _pointB, normal, new Rectangle(_size, 1));
				});
				// Light 
				Layout_Bezier(p0, p1, p2, p3, stepCount, (_pointA, _pointB, _step) => {
					int _size = GetSize(_step);
					int shift = (int)(_size * (1f - lightAmount));
					if (!lightR) {
						shift *= -1;
					}
					_pointA.x += shift;
					_pointB.x += shift;
					DrawLine(_pointA, _pointB, light, new Rectangle((int)(_size * lightAmount), 1));
				});
				// Func
				int GetSize (int _step) => Mathf.Clamp(
					Util.RemapUnclampedRounded(0, stepCount - 1f, size / (1f - sizeTurning), 0, _step),
					0, size
				);
			}
		}



	}
}
