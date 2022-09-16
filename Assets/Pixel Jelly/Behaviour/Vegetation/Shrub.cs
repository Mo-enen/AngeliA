using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PixelJelly {
	public class Shrub : VegetationWithFlower {





		#region --- VAR ---


		// Api
		public override string DisplayGroup => "Vegetation";
		public override string DisplayName => "Shrub";
		public override FlowerStyles FlowerStyle => FlowerStyles.Full;
		
		public override int MaxFrameCount => 1;
		public override int MaxSpriteCount => 128;
		public LightDirection2 LightDirection1 { get => m_LightDirection; set => m_LightDirection = value; }
		public float Light { get => m_Light; set => m_Light = value; }
		public Color32 ShrubLight { get => m_ShrubLight; set => m_ShrubLight = value; }
		public Color32 ShrubNormal { get => m_ShrubNormal; set => m_ShrubNormal = value; }
		public Color32 ShrubDark { get => m_ShrubDark; set => m_ShrubDark = value; }
		public Vector2 Dithering { get => m_Dithering; set => m_Dithering = value; }
		public Vector2Int LayerCount { get => m_LayerCount; set => m_LayerCount = value; }

		// Ser
		[SerializeField, ArrowNumber(true)] private LightDirection2 m_LightDirection = LightDirection2.Right;
		[SerializeField, ArrowNumber(0f, 1f, 0.1f)] private float m_Light = 0.8f;
		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] private Vector2 m_Dithering = new Vector2(0.1f, 0.8f);
		[SerializeField, MinMaxNumber(1)] private Vector2Int m_LayerCount = new Vector2Int(1, 3);
		[SerializeField] Color32 m_ShrubLight = new Color32(83, 245, 113, 255);
		[SerializeField] Color32 m_ShrubNormal = new Color32(58, 166, 105, 255);
		[SerializeField] Color32 m_ShrubDark = new Color32(60, 115, 96, 255);


		#endregion




		#region --- MSG ---


		protected override void OnCreated () {
			base.OnCreated();
			Width = 32;
			Height = 16;
			SpriteCount = 12;
			FrameCount = 1;
		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2(width / 2f, 0);
			border = new RectOffset(0, 0, 0, 0);
			if (width <= 0 || height <= 0 || frameCount <= 0 || spriteCount <= 0) { return; }
			Layout_Shrub(
				new RectInt(0, 0, width, height - 3),
				m_LayerCount.RandomBetween(Random), 1f,
				(_rect) => DrawShrub(_rect, m_Light, m_Dithering.RandomBetween(Random), m_LightDirection == LightDirection2.Right, m_ShrubLight, m_ShrubDark, m_ShrubNormal)
			);
			base.OnPopulatePixels(width, height, frame, frameCount, sprite, spriteCount, out _, out _);
		}


		#endregion




		#region --- LGC ---


		protected void DrawShrub (RectInt rect, float amount, float dithering, bool lightR, Color32 light, Color32 dark, Color32 normal) {
			int diameter = Mathf.Min(rect.width, rect.height);
			var circleA = new Circle(diameter, Vector2Int.zero);
			int r = rect.width - diameter;
			int u = rect.height - diameter;
			var CLEAR = new Color32(0, 0, 0, 0);
			// Back
			for (int i = 0; i <= r; i++) {
				for (int j = 0; j <= u; j++) {
					DrawShape(i, j, dark, CLEAR, false, false, null, 0, 0);
				}
			}
			// Normal
			DrawShape(
				lightR ? r : 0, u,
				normal, dark, true, true, new Circle(diameter * 2, Vector2Int.zero),
				lightR ? 0 : -diameter, diameter / 8
			);
			// Light
			DrawShape(
				lightR ? r : 0, u,
				light, normal, true, true, new Circle(diameter * 3, Vector2Int.zero),
				lightR ? 0 : -2 * diameter, diameter / 4
			);
			// Func
			void DrawShape (int x, int y, Color32 color, Color32 bgColor, bool useAmount, bool _dithering, PixelShape shape, int offsetX, int offsetY) {
				for (int j = 0; j < diameter && j < rect.height; j++) {
					for (int i = 0; i < diameter && i < rect.width; i++) {
						var _color = color;
						if (_dithering && (i + j) % 2 == 0 && Random.NextDouble() < dithering) {
							_color = bgColor;
						}
						if (
							(!useAmount || Random.NextDouble() < amount) &&
							circleA.PixelCheck(i, j) &&
							(shape == null || shape.PixelCheck(i - offsetX, j - offsetY))
						) {
							SetColor(rect.x + x + i, rect.y + y + j, _color, BlendMode.OneMinusAlpha);
						}
					}
				}
			}
		}


		#endregion




	}
}
