using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelJelly;

namespace PixelJelly {
	public class Mushroom : JellyBehaviour {



		// Api
		public override string DisplayName => "Mushroom";
		public override string DisplayGroup => "Vegetation";
		public override int MaxFrameCount => 1;

		public override int MaxSpriteCount => 64;
		public LightDirection2 LightDirection { get => m_Light; set => m_Light = value; }
		public Vector2Int StemTop { get => m_StemTop; set => m_StemTop = value; }
		public Vector2Int StemBottom { get => m_StemBottom; set => m_StemBottom = value; }
		public Vector2Int CapOffsetX { get => m_CapOffsetX; set => m_CapOffsetX = value; }
		public Vector2Int CapOffsetY { get => m_CapOffsetY; set => m_CapOffsetY = value; }
		public Color32 StemLight { get => m_StemLight; set => m_StemLight = value; }
		public ColorGradient StemNormal { get => m_StemNormal; set => m_StemNormal = value; }
		public Color32 StemDark { get => m_StemDark; set => m_StemDark = value; }
		public PixelSprite[] Caps { get => m_Caps; set => m_Caps = value; }
		public Vector2Int StemOffsetY { get => m_StemOffsetY; set => m_StemOffsetY = value; }

		// Ser
		[SerializeField, ArrowNumber(true)] private LightDirection2 m_Light = LightDirection2.Right;
		[SerializeField, MinMaxNumber(-128, 128)] private Vector2Int m_StemOffsetY = new Vector2Int(-5, 0);
		[SerializeField, MinMaxNumber(1, 128)] private Vector2Int m_StemTop = new Vector2Int(1, 3);
		[SerializeField, MinMaxNumber(1, 128)] private Vector2Int m_StemBottom = new Vector2Int(3, 7);
		[SerializeField, MinMaxNumber(-128, 128)] private Vector2Int m_CapOffsetX = new Vector2Int(0, 4);
		[SerializeField, MinMaxNumber(-128, 128)] private Vector2Int m_CapOffsetY = new Vector2Int(-2, 2);
		[SerializeField] Color32 m_StemLight = new Color32(239, 194, 160, 255);
		[SerializeField, ColorGradient] private ColorGradient m_StemNormal = new ColorGradient();
		[SerializeField] Color32 m_StemDark = new Color32(177, 122, 102, 255);
		[SerializeField, PixelEditor] private PixelSprite[] m_Caps = new PixelSprite[0];


		// MSG
		protected override void OnCreated () {
			Width = 16;
			Height = 16;
			FrameCount = 1;
			SpriteCount = 24;
			m_StemNormal = new ColorGradient(
				new Color32(208, 158, 131, 255), new Color32(194, 148, 114, 255)
			);
			m_Caps = GetDefaultCaps();
		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2(width / 2f, 0);
			border = new RectOffset(0, 0, 0, 0);
			if (sprite == 0 && frame == 0) {
				if (Mathf.Abs(m_CapOffsetY.x) > height || Mathf.Abs(m_CapOffsetY.y) > height) {
					AddInspectorMessage("Cap Offset is too large.", MessageType.Warning, "m_CapOffset");
				}
			}
			height = Mathf.Clamp(height + m_StemOffsetY.RandomBetween(Random), 0, height);
			var cap = m_Caps.MushroomPick(sprite, spriteCount);
			int capHeight = cap != null ? cap.Height + m_CapOffsetY.RandomBetween(Random) : 0;
			capHeight = Mathf.Clamp(capHeight, 0, height - 1);
			DrawMushroomStem(
				new RectInt(0, 0, width, height),
				m_StemBottom.RandomBetween(Random), m_StemTop.RandomBetween(Random),
				capHeight,
				m_Light == LightDirection2.Right,
				m_StemLight, m_StemDark, m_StemNormal
			);
			if (cap != null) {
				int offsetX = m_CapOffsetX.RandomBetween(Random);
				DrawMushroomCap(
					new RectInt(
						Mathf.Clamp(offsetX, 0, width - 1),
						height - capHeight,
						Mathf.Clamp(width - offsetX * 2, 0, width),
						capHeight
					),
					m_Light == LightDirection2.Right, cap
				);
			}
		}


		// API
		protected void DrawMushroomStem (RectInt rect, int bottomWidth, int topWidth, int shadowHeight, bool lightR, Color32 light, Color32 dark, ColorGradient normal) {
			using var _mask = new MaskScope(rect);
			int stemShapeHeight = rect.height / 3;
			int bottomRadius = Random.Next(1, bottomWidth / 3 + 1);
			int topRadius = Random.Next(1, topWidth / 3 + 1);
			int topNormalWidth = topWidth;
			int topNormalY = rect.yMax - shadowHeight - 1;
			int topLightY = rect.yMax - shadowHeight - 1;
			bool topNormalSetted = false;
			// Dark
			Layout_Stack(
				new RectInt(rect.x + (rect.width - bottomWidth) / 2, rect.y, bottomWidth, stemShapeHeight),
				new RectInt(rect.x + (rect.width - topWidth) / 2, rect.yMax - shadowHeight, topWidth, stemShapeHeight),
				(_rect, _step, _stepCount) => {
					int deltaY = _rect.y - rect.y;
					int radiusD = deltaY < bottomRadius ? bottomRadius : 0;
					int radiusU = deltaY > _stepCount - topRadius ? topRadius : 0;
					DrawShape(
						new RoundedRect(
							_rect.width, _rect.height, radiusD, radiusD, radiusU, radiusU
						), _rect.x, _rect.y, dark
					);
					if (!topNormalSetted && _rect.yMax >= topNormalY) {
						topNormalWidth = _rect.width;
						topNormalSetted = true;
					}
				}
			);
			// Normal
			int normalShiftX = Mathf.CeilToInt(bottomWidth / 6f) - Random.Next(0, bottomWidth / 6 + 1);
			int lightShiftX = bottomWidth - Random.Next(1, bottomWidth / 6 + 2);
			if (!lightR) {
				normalShiftX *= -1;
				normalShiftX--;
				lightShiftX *= -1;
				lightShiftX--;
			}
			using (new LockClearScope()) {
				Layout_Stack(
					new RectInt(rect.x + (rect.width - bottomWidth) / 2 + normalShiftX, rect.y, bottomWidth, stemShapeHeight),
					new RectInt(rect.x + (rect.width - topWidth) / 2 + normalShiftX, topNormalY - stemShapeHeight, topNormalWidth, stemShapeHeight),
					(_rect, _step, _stepCount) => {
						DrawShape(
							new RoundedRect(
								_rect.width,
								_rect.height,
								_step <= bottomRadius ? bottomRadius :
								_step >= _stepCount - topRadius ? topRadius : 0
							), _rect.x, _rect.y, normal, BlendMode.OneMinusAlpha
						);
					}
				);
				// Light
				Layout_Stack(
					new RectInt(rect.x + (rect.width - bottomWidth) / 2 + lightShiftX, rect.y, bottomWidth, stemShapeHeight),
					new RectInt(rect.x + (rect.width - topWidth) / 2 + lightShiftX, topLightY - stemShapeHeight, topNormalWidth, stemShapeHeight),
					(_rect, _step, _stepCount) => {
						DrawShape(
							new RoundedRect(
								_rect.width,
								_rect.height,
								_step <= bottomRadius ? bottomRadius :
								_step >= _stepCount - topRadius ? topRadius : 0
							), _rect.x, _rect.y, light, BlendMode.OneMinusAlpha
						);
					}
				);
			}
		}


		protected void DrawMushroomCap (RectInt rect, bool lightR, PixelSprite cap) {
			if (cap == null) { return; }
			// Cap
			DrawSprite(
				rect, cap, new Color32(255, 255, 255, 255),
				SpriteScaleMode.Slice,
				BlendMode.OneMinusAlpha, false, !lightR
			);
		}


		// LGC
		private PixelSprite[] GetDefaultCaps () => new PixelSprite[] {
			new PixelSprite("12_7_5_0_5_6_2_4_0_2521519359_2521519359_2521519359_2521519359_2521519359_2521519359_2521519359_2521519359_3345507327_3345507327_0_2521519359_2521519359_2521519359_3345507327_3345507327_3345507327_3345507327_3345507327_3345507327_3345507327_3345507327_4287854847_2521519359_2521519359_3345507327_3345507327_3345507327_3345507327_3345507327_3345507327_3345507327_3345507327_4287854847_4287854847_2521519359_2521519359_3345507327_3345507327_3345507327_3345507327_3345507327_3345507327_3345507327_3345507327_4287854847_4287854847_0_2521519359_2521519359_3345507327_3345507327_3345507327_3345507327_3345507327_3345507327_4287854847_4287854847_0_0_0_2521519359_2521519359_3345507327_3345507327_3345507327_3345507327_4287854847_4287854847_0_0_0_0_0_2521519359_2521519359_3345507327_4287854847_4287854847_4287854847_0_0_0_"),
			new PixelSprite("12_9_5_0_5_6_4_4_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_2894150655_3786092799_2894150655_3786092799_2894150655_2894150655_2894150655_2894150655_3786092799_3786092799_0_2894150655_3786092799_2894150655_3786092799_2894150655_3786092799_3786092799_3786092799_3786092799_3786092799_3786092799_4241836799_2894150655_3786092799_3786092799_3786092799_3786092799_3786092799_3786092799_3786092799_3786092799_3786092799_3786092799_4241836799_2894150655_2894150655_3786092799_3786092799_3786092799_3786092799_3786092799_4241836799_3786092799_4241836799_3786092799_4241836799_0_2894150655_2894150655_3786092799_3786092799_3786092799_4241836799_3786092799_4241836799_3786092799_4241836799_0_0_0_2894150655_2894150655_3786092799_3786092799_3786092799_4241836799_3786092799_4241836799_0_0_0_0_0_2894150655_2894150655_3786092799_4241836799_4241836799_4241836799_0_0_0_"),
			new PixelSprite("12_7_5_0_5_6_3_3_0_0_0_177178367_177178367_177178367_177178367_162898431_162898431_0_0_0_0_0_177178367_162898431_162898431_162898431_162898431_162898431_162898431_16764159_0_0_0_177178367_162898431_162898431_162898431_162898431_162898431_162898431_162898431_162898431_16764159_0_177178367_177178367_162898431_162898431_162898431_162898431_162898431_162898431_162898431_162898431_16764159_16764159_177178367_177178367_162898431_162898431_162898431_162898431_162898431_162898431_162898431_162898431_16764159_16764159_0_177178367_177178367_162898431_162898431_162898431_162898431_162898431_162898431_16764159_16764159_0_0_0_177178367_177178367_177178367_162898431_16764159_16764159_16764159_16764159_0_0_"),
			new PixelSprite("10_6_4_0_4_5_2_3_0_0_0_2323742719_2323742719_2323742719_3098322943_0_0_0_0_0_2323742719_3098322943_3098322943_3098322943_3098322943_3098322943_0_0_0_2323742719_3098322943_3098322943_3098322943_3098322943_3098322943_3098322943_4041661183_0_0_2323742719_2323742719_3098322943_3098322943_3098322943_3098322943_4041661183_4041661183_0_0_0_2323742719_2323742719_3098322943_3098322943_4041661183_4041661183_0_0_0_0_0_2323742719_3098322943_4041661183_4041661183_0_0_0_"),
		};



	}
}
