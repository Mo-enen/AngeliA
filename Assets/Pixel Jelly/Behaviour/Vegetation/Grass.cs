using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace PixelJelly {
	public class Grass : VegetationWithFlower {


		// SUB
		public enum GrassStyle {
			Wave = 0,
			Straight = 1,
			Checkered = 2,
			Wheat = 3,

		}




		// Api
		public override string DisplayGroup => "Vegetation";
		public override string DisplayName => "Grass";
		public override FlowerStyles FlowerStyle => FlowerStyles.Horizontal;
		public GrassStyle Style { get => m_Style; set => m_Style = value; }
		public Vector2Int Iteration { get => m_Iteration; set => m_Iteration = value; }
		public Vector2Int IterationRadius { get => m_IterationRadius; set => m_IterationRadius = value; }
		public Vector2 Amount { get => m_Amount; set => m_Amount = value; }
		public Vector2Int MinHeight { get => m_MinHeight; set => m_MinHeight = value; }
		public Vector2Int EdgeSmooth { get => m_EdgeSmooth; set => m_EdgeSmooth = value; }
		public Vector2Int LayerCount { get => m_LayerCount; set => m_LayerCount = value; }
		public LightDirection2 LightDirection { get => m_LightDirection; set => m_LightDirection = value; }
		public Color32 ColorA { get => m_ColorA; set => m_ColorA = value; }
		public Color32 ColorB { get => m_ColorB; set => m_ColorB = value; }
		public Color32 ColorC { get => m_ColorC; set => m_ColorC = value; }

		// Ser
		[SerializeField, ArrowNumber] private GrassStyle m_Style = GrassStyle.Wave;
		[SerializeField, MinMaxNumber(1, 6)] private Vector2Int m_Iteration = new Vector2Int(1, 3);
		[SerializeField, MinMaxNumber(1, 6)] private Vector2Int m_IterationRadius = new Vector2Int(1, 1);
		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] private Vector2 m_Amount = new Vector2(0.3f, 0.8f);
		[SerializeField, MinMaxNumber(int.MinValue)] private Vector2Int m_MinHeight = new Vector2Int(-5, 0);
		[SerializeField, MinMaxNumber(0)] private Vector2Int m_EdgeSmooth = new Vector2Int(0, 2);
		[SerializeField, MinMaxNumber(1)] private Vector2Int m_LayerCount = new Vector2Int(1, 3);
		[SerializeField, ArrowNumber(true)] LightDirection2 m_LightDirection = LightDirection2.Right;
		[SerializeField] Color32 m_ColorA = new Color32(83, 245, 113, 255);
		[SerializeField] Color32 m_ColorB = new Color32(58, 166, 105, 255);
		[SerializeField] Color32 m_ColorC = new Color32(60, 115, 96, 255);


		// MSG
		protected override void OnCreated () {
			base.OnCreated();
			Width = 16;
			Height = 16;
			FrameCount = 1;
			SpriteCount = 12;
		}


		protected override void BeforePopulateAllPixels () {
			FrameCount = 1;
		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2(width / 2f, 0);
			border = new RectOffset(0, 0, 0, 0);
			if (width <= 0 || height <= 0 || frameCount <= 0) { return; }
			if (frame == 0 && sprite == 0) { CheckForMessages(height); }
			float[] values = new float[width];
			float[] valuesAlt = new float[width];
			var random0 = new System.Random(Seed + sprite);
			var random1 = new System.Random(Seed * 2 + sprite);
			switch (Style) {
				default:
				case GrassStyle.Wave:
					int layerCount = m_LayerCount.RandomBetween(Random);
					for (int layer = 0; layer < layerCount; layer++) {
						int iteration = m_Iteration.RandomBetween(Random);
						int iterRadius = m_IterationRadius.RandomBetween(Random);
						AverageIteration(values, iteration, iterRadius, m_EdgeSmooth.RandomBetween(Random), random0);
						AverageIteration(valuesAlt, Mathf.Max(iteration - 1, 1), Mathf.Max(iterRadius - 1, 1), 0, random1);
						GenerateWavePixels(values, valuesAlt, width, height, layer, layerCount, m_MinHeight.RandomBetween(Random));
					}
					break;
				case GrassStyle.Straight:
					AverageIteration(values, 0, 0, 0, random0);
					AverageIteration(valuesAlt, 0, 0, 0, random1);
					GenerateStraightPixels(values, valuesAlt, m_Amount.RandomBetween(Random), width, height, m_MinHeight.RandomBetween(Random));
					break;
				case GrassStyle.Checkered:
					AverageIteration(values, m_Iteration.RandomBetween(Random), m_IterationRadius.RandomBetween(Random), 0, random0);
					AverageIteration(valuesAlt, 0, 0, 0, random1);
					GenerateCheckeredPixels(values, valuesAlt, m_Amount.RandomBetween(Random), width, height, m_MinHeight.RandomBetween(Random));
					break;
				case GrassStyle.Wheat:
					AverageIteration(values, m_Iteration.RandomBetween(Random), m_IterationRadius.RandomBetween(Random), 0, random0);
					AverageIteration(valuesAlt, 0, 0, 0, random1);
					GenerateWheatPixels(values, valuesAlt, m_Amount.RandomBetween(Random), width, height, m_MinHeight.RandomBetween(Random));
					break;
			}
			base.OnPopulatePixels(width, height, frame, frameCount, sprite, spriteCount, out _, out _);
		}


		// Pipeline
		private void GenerateWavePixels (float[] values, float[] lights, int width, int height, int layer, int layerCount, int minHeight) {
			int offsetY = (int)Util.Remap(0, layerCount, 0, height, layer);
			int prevHeight = GetHeight(0);
			bool lightR = m_LightDirection == LightDirection2.Right;
			int lightOffset = height / (layerCount + 1);
			for (int i = 0; i < width; i++) {
				int grassHeight = GetHeight(i);
				int grassLight = GetLight(i);
				int nextHeight = i < width - 1 ? GetHeight(i + 1) : grassHeight;
				int darkHeight = grassHeight;
				int rightID = GetRightID(i, grassHeight, prevHeight, nextHeight);
				if (rightID < 0) {
					darkHeight = grassHeight - 1 - grassLight;
				} else if (rightID == 3) {
					darkHeight = (lightR ? nextHeight : prevHeight) - grassLight;
				} else if (rightID == 2 && lightR) {
					darkHeight = nextHeight - grassLight;
				} else if (rightID == 1 && !lightR) {
					darkHeight = prevHeight - grassLight;
				}
				int lightHeight = (grassHeight + darkHeight) / 2;
				prevHeight = grassHeight;
				for (int j = 0; j < grassHeight; j++) {
					SetColor(
						i, j,
						j >= lightHeight ? m_ColorA :
						j >= darkHeight ? m_ColorC :
						m_ColorB
					);
				}
			}
			// Func
			int GetHeight (int index) => Mathf.RoundToInt(Mathf.Lerp(minHeight, height, values[index])) - offsetY;
			int GetLight (int index) => Mathf.RoundToInt(Mathf.Lerp(0, lightOffset, lights[index]));
			int GetRightID (int _i, int _height, int _pHeight, int _nHeight) {
				int result;
				// L
				bool? leftHigher = null;
				if (_pHeight == _height) {
					for (int j = _i - 2; j >= 0; j--) {
						int h = GetHeight(j);
						if (h == _height) { continue; }
						leftHigher = h > _height;
					}
				} else {
					leftHigher = _pHeight > _height;
				}
				// R
				bool? rightHigher = null;
				if (_nHeight == _height) {
					for (int j = _i + 2; j < width; j++) {
						int h = GetHeight(j);
						if (h == _height) { continue; }
						rightHigher = h > _height;
					}
				} else {
					rightHigher = _nHeight > _height;
				}
				// Result
				if (leftHigher.HasValue) {
					if (leftHigher.Value) {
						result = rightHigher.HasValue && rightHigher.Value ? 4 : 2;
					} else {
						result = rightHigher.HasValue && !rightHigher.Value ? 3 : 1;
					}
				} else {
					result = !rightHigher.HasValue ? 3 : rightHigher.Value ? 1 : 2;
				}
				// Flat Fix
				if (m_LightDirection == LightDirection2.Right && result == 2 && _height == _nHeight) {
					result = -2;
				} else if (m_LightDirection == LightDirection2.Left && result == 1 && _height == _pHeight) {
					result = -1;
				}
				return result;
				// 1: Left, 2: Right, 3:Top, 4:Bottom
			}
		}


		private void GenerateStraightPixels (float[] values, float[] amounts, float amount, int width, int height, int minHeight) {
			for (int i = 0; i < width; i++) {
				if (amounts[i] > amount) { continue; }
				int grassHeight = GetHeight(i);
				for (int j = 0; j < grassHeight; j++) {
					SetColor(
						i, j,
						j >= grassHeight * 0.666f ? m_ColorA :
						j >= grassHeight * 0.333f ? m_ColorB :
						m_ColorC
					);
				}
			}
			// Func
			int GetHeight (int index) => Mathf.RoundToInt(Mathf.Lerp(minHeight, height, values[index]));
		}


		private void GenerateCheckeredPixels (float[] values, float[] amounts, float amount, int width, int height, int minHeight) {
			Color color;
			int colorID = 0;
			bool prevCheckered = false;
			for (int i = 0; i < width; i++) {
				float _a = amounts[i];
				if (_a > amount) { continue; }
				int grassHeight = GetHeight(i);
				int randomID = (int)(_a * 1000f) % 100;
				colorID = (int)Mathf.Repeat((colorID + (randomID < 10 ? 0 : randomID < 45 ? 1 : -1)), 2.9999f);
				color = colorID == 0 ? m_ColorA : colorID == 1 ? m_ColorB : m_ColorC;
				if (prevCheckered && colorID == 0) {
					colorID = 1;
				}
				if (colorID == 0) {
					// Checkered
					for (int j = 0; j < grassHeight; j += 2) {
						SetColor(i, j, color);
					}
					i++;
					if (i < width) {
						for (int j = 1; j < grassHeight; j += 2) {
							SetColor(i, j, color);
						}
					}
					prevCheckered = true;
				} else {
					// Normal
					for (int j = 0; j < grassHeight; j++) {
						SetColor(i, j, color);
					}
					prevCheckered = false;
				}
			}
			// Func
			int GetHeight (int index) => Mathf.RoundToInt(Mathf.Lerp(minHeight, height, values[index]));
		}


		private void GenerateWheatPixels (float[] values, float[] amounts, float amount, int width, int height, int minHeight) {
			Color color;
			int colorID = 0;
			for (int i = 0; i < width; i++) {
				float _a = amounts[i];
				if (_a > amount) { continue; }
				int grassHeight = GetHeight(i);
				int randomID = (int)(_a * 1000f) % 100;
				colorID = (int)Mathf.Repeat((colorID + (randomID < 15 ? 0 : randomID < 60 ? 1 : -1)), 2.9999f);
				color = colorID == 0 ? m_ColorA : colorID == 1 ? m_ColorB : m_ColorC;
				for (int j = 0; j < grassHeight; j++) {
					if (j == grassHeight - 2) { continue; }
					SetColor(i, j, color);
				}
			}
			// Func
			int GetHeight (int index) => Mathf.RoundToInt(Mathf.Lerp(minHeight, height, values[index]));
		}


		// LGC
		private void CheckForMessages (int height) {
			// Min Height
			if (m_MinHeight.x >= height) {
				AddInspectorMessage("Min Height should be smaller than Height", MessageType.Warning, "m_MinHeight");
			}
			// Amount
			if (Style != GrassStyle.Wave && Mathf.Approximately(m_Amount.y, 0f)) {
				AddInspectorMessage("Amount is too small", MessageType.Warning, "m_Amount");
			}
		}


	}
}