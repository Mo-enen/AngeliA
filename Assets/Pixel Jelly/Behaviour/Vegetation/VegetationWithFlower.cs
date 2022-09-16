using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PixelJelly {
	public abstract class VegetationWithFlower : JellyBehaviour {



		// SUB
		public enum FlowerStyles {
			Horizontal = 0,
			Full = 1,

		}



		// Api
		public virtual FlowerStyles FlowerStyle => FlowerStyles.Full;
		public Vector2 FlowerRangeX { get => m_FlowerRangeX; set => m_FlowerRangeX = value; }
		public Vector2 FlowerRangeY { get => m_FlowerRangeY; set => m_FlowerRangeY = value; }
		public Vector2Int FlowerCount { get => m_FlowerCount; set => m_FlowerCount = value; }

		// Ser
		[SerializeField, MinMaxNumber(0, 1024)] private Vector2Int m_FlowerCount = new Vector2Int(2, 5);
		[SerializeField, MinMaxSlider(0, 1)] private Vector2 m_FlowerRangeX = new Vector2(0f, 1f);
		[SerializeField, MinMaxSlider(0, 1)] private Vector2 m_FlowerRangeY = new Vector2(0.618f, 1f);
		[SerializeField, PixelEditor] private PixelSprite[] m_Flowers = new PixelSprite[1];



		// MSG
		protected override void OnCreated () {
			m_Flowers = new PixelSprite[] {
				new PixelSprite("3_3_1_1_0_0_0_0_0_4294967295_0_4294967295_3342486271_4294967295_0_4294967295_0_"),
				new PixelSprite("3_3_1_1_0_0_0_0_0_4294967295_0_4294967295_4289016575_4294967295_0_4294967295_0_"),
				new PixelSprite("3_3_1_1_0_0_0_0_0_4294967295_0_4294967295_1933091327_4294967295_0_4294967295_0_"),
			};
		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2(width / 2f, 0);
			border = new RectOffset(0, 0, 0, 0);
			if (sprite == 0 && frame == 0) {
				if (FlowerStyle == FlowerStyles.Horizontal && m_FlowerCount.x > width) {
					AddInspectorMessage("Flower Count too large.", MessageType.Warning, "m_FlowerCount");
				}
				if (m_Flowers == null || m_Flowers.Length == 0) {
					AddInspectorMessage("Flowers is empty", MessageType.Warning, "m_Flowers");
				}
			}
			if (m_Flowers == null || m_Flowers.Length == 0) { return; }
			switch (FlowerStyle) {
				case FlowerStyles.Horizontal:
					AddHorizontalFlowers(width, height, m_FlowerCount.RandomBetween(Random), m_FlowerRangeY, m_Flowers.MushroomPick(sprite, spriteCount));
					break;
				case FlowerStyles.Full:
					AddFullFlowers(width, height, m_FlowerCount.RandomBetween(Random), m_FlowerRangeX, m_FlowerRangeY, m_Flowers.MushroomPick(sprite, spriteCount));
					break;
			}
		}


		protected void AddFullFlowers (int width, int height, int flowerCount, Vector2 flowerRangeX, Vector2 flowerRangeY, PixelSprite flower) {
			if (flower == null) { return; }
			int fWidth = flower.Width;
			int fHeight = flower.Height;
			int fPivotX = flower.PivotX;
			int fPivotY = flower.PivotY;
			if (width < fWidth || height < fHeight) { return; }
			int xMin = (int)(flowerRangeX.x * width) + fPivotX;
			int xMax = Mathf.Max((int)(flowerRangeX.y * width) - fWidth + fPivotX, xMin);
			int yMin = (int)(flowerRangeY.x * height) + fPivotY;
			int yMax = Mathf.Max((int)(flowerRangeY.y * height) - fHeight + fPivotY - 1, yMin);
			for (int fIndex = 0, safe = 0; fIndex < flowerCount && safe < width * height; fIndex++, safe++) {
				int x = Random.Next(xMin, xMax + 1);
				int y = Random.Next(yMin, yMax + 1);
				x -= x % 2;
				while (y > 0 && GetColor(x, y).a <= 0) { y--; }
				if (y < fPivotY) {
					fIndex--;
					continue;
				}
				DrawFlower(
					Mathf.Clamp(x, fPivotX, width - fWidth + fPivotX),
					Mathf.Clamp(y - y % 2, fPivotY, height - fHeight + fPivotY),
					flower
				);
			}
		}


		protected void AddHorizontalFlowers (int width, int height, int flowerCount, Vector2 flowerRangeY, PixelSprite flower) {
			if (flower == null) { return; }
			int fWidth = flower.Width;
			int fHeight = flower.Height;
			int fPivotX = flower.PivotX;
			int fPivotY = flower.PivotY;
			if (width < fWidth || height < fHeight) { return; }
			// Height
			var gHeights = new int[width];
			for (int i = 0; i < width; i++) {
				gHeights[i] = 0;
				for (int j = height - 1; j >= 0; j--) {
					if (GetColor(i, j).a > 0) {
						gHeights[i] = j + 1;
						break;
					}
				}
			}
			// Add Flower
			int l = fPivotX;
			int r = Mathf.Clamp(width - fWidth + fPivotX, l, width - 1);
			for (int flowerIndex = 0; flowerIndex < flowerCount; flowerIndex++) {
				int xOffset = Random.Next(0, width);
				for (int x = l; x <= r; x++) {
					int _x = ((x - l + xOffset) % (r - l + 1)) + l;
					int grassHeight = gHeights[_x];
					if (grassHeight > 0) {
						// Get Y01
						float y01 = Mathf.Lerp(flowerRangeY.x, flowerRangeY.y, Random.NextFloat());
						// Draw Flower
						DrawFlower(
							_x, Mathf.Clamp(Mathf.RoundToInt(grassHeight * y01), fPivotY, height - fHeight + fPivotY), flower
						);
						// Clear Height
						gHeights[_x] = 0;
						break;
					}
					// No Flower
					if (x == r) {
						flowerIndex = flowerCount;
					}
				}
			}
		}


		// LGC
		private void DrawFlower (int _x, int _y, PixelSprite flower) => DrawSprite(new RectInt(_x, _y, flower.Width, flower.Height), flower, SpriteScaleMode.Original, BlendMode.OneMinusAlpha);


	}
}
