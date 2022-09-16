using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PixelJelly {
	public class Rock : JellyBehaviour {






		#region --- VAR ---


		// Api
		public override string DisplayName => "Rock";
		public override string DisplayGroup => "Environment";

		public override int MaxFrameCount => 1;
		public override int MaxSpriteCount => 128;
		public ColorGradient Light { get => m_Light; set => m_Light = value; }
		public ColorGradient Normal { get => m_Normal; set => m_Normal = value; }
		public Color32 Dark { get => m_Dark; set => m_Dark = value; }
		public Vector2 Amount { get => m_Amount; set => m_Amount = value; }
		public Vector2Int LayerCount { get => m_LayerCount; set => m_LayerCount = value; }
		public LightDirection2 LightingDirection { get => m_LightDirection; set => m_LightDirection = value; }

		// Ser
		[SerializeField, ArrowNumber(true)] LightDirection2 m_LightDirection = LightDirection2.Right;
		[SerializeField, MinMaxNumber(1, 128, 1, true)] Vector2Int m_MinRange = new Vector2Int(12, 8);
		[SerializeField, MinMaxNumber(1)] Vector2Int m_BottomWidth = new Vector2Int(6, 10);
		[SerializeField, MinMaxNumber(1)] Vector2Int m_TopWidth = new Vector2Int(6, 10);
		[SerializeField, MinMaxNumber(1, 128)] Vector2Int m_LayerCount = new Vector2Int(1, 2);
		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] Vector2 m_Amount = new Vector2(0.6f, 0.9f);
		[SerializeField] bool m_Tilted = true;
		[SerializeField, ColorGradient] ColorGradient m_Light = new ColorGradient();
		[SerializeField, ColorGradient] ColorGradient m_Normal = new ColorGradient();
		[SerializeField] Color32 m_Dark = new Color32(138, 129, 127, 255);
		[SerializeField, PixelEditor] PixelSprite[] m_Rocks = new PixelSprite[0];


		#endregion




		#region --- MSG ---


		protected override void OnCreated () {
			Width = 24;
			Height = 16;
			SpriteCount = 21;
			FrameCount = 1;
			m_Light = new ColorGradient(new Color32(240, 230, 218, 255), new Color32(217, 208, 197, 255));
			m_Normal = new ColorGradient(new Color32(184, 172, 167, 255), new Color32(164, 154, 149, 255));
			m_Rocks = GetDefaultRocks();
		}


		protected override void BeforePopulateAllPixels () {
			FrameCount = 1;
			m_MinRange.x = Mathf.Min(m_MinRange.x, Width);
			m_MinRange.y = Mathf.Min(m_MinRange.y, Height);
		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2(width / 2f, 0);
			border = new RectOffset(0, 0, 0, 0);
			if (sprite == 0 && frame == 0) { CheckInspectorMsg(); }
			int realWidth = Random.Next(Mathf.Min(m_MinRange.x, width), width + 1);
			int realHeight = Random.Next(Mathf.Min(m_MinRange.y, height - 1), height);
			realHeight = Mathf.Min(realHeight, realWidth);
			Layout_Shrub(
				new RectInt((width - realWidth) / 2, 0, realWidth, realHeight),
				m_LayerCount.RandomBetween(Random),
				m_Amount.RandomBetween(Random),
				(_rect) => DrawRock(
					_rect,
					m_BottomWidth.RandomBetween(Random),
					m_TopWidth.RandomBetween(Random),
					m_Tilted,
					m_LightDirection == LightDirection2.Left,
					m_Light, m_Normal, m_Dark,
					m_Rocks.Length > 0 ? m_Rocks[Random.Next(0, m_Rocks.Length)] : null
				)
			);
		}


		#endregion




		#region --- LGC ---


		protected virtual void DrawRock (RectInt rect, int bottomWidth, int topWidth, bool tilt, bool flip, ColorGradient light, ColorGradient normal, Color32 dark, PixelSprite face) {
			RectInt from = default;
			RectInt to = default;
			from.width = Mathf.Min(bottomWidth, rect.width);
			to.width = Mathf.Min(topWidth, rect.width);
			from.height = rect.height + Random.Next((int)(rect.height * -0.7f), (int)(rect.height * -0.5f + 1));
			to.height = rect.height + Random.Next((int)(rect.height * -0.7f), (int)(rect.height * -0.5f + 1));
			if (tilt) {
				from.x = Random.Next(0, rect.width - from.width);
				to.x = Random.Next(0, rect.width - to.width);
			} else {
				from.x = (rect.width - from.width) / 2;
				to.x = (rect.width - to.width) / 2;
			}
			from.y = 0;
			to.y = rect.height - to.height;
			// Stack
			Layout_Stack(from, to, (_rect, _step, _stepCount) => {
				_rect.x += rect.x;
				_rect.y += rect.y;
				if (_step == 0) {
					DrawFace(_rect, dark);
				} else {
					DrawFaceGradient(_rect, normal);
				}
				if (_step == _stepCount - 1) {
					_rect = _rect.Expand(-1, -1, 0, 0);
					DrawFaceGradient(_rect, light);
				}
			});
			// Func
			void DrawFace (RectInt _rect, Color32 _color) {
				if (face == null) {
					DrawRect(_rect, _color, BlendMode.OneMinusAlpha);
				} else {
					DrawSprite(_rect, face, _color, SpriteScaleMode.Slice, BlendMode.OneMinusAlpha, false, flip);
				}
			}
			void DrawFaceGradient (RectInt _rect, ColorGradient _gradient) {
				if (face == null) {
					DrawRect(_rect, _gradient, BlendMode.OneMinusAlpha);
				} else {
					DrawSprite(_rect, face, _gradient, SpriteScaleMode.Slice, BlendMode.OneMinusAlpha, false, flip);
				}
			}
		}


		private void CheckInspectorMsg () {
			if (m_Amount.y < 0 || m_Amount.y.AlmostZero()) {
				AddInspectorMessage("Amount is too small", MessageType.Warning, "m_Amount");
			}
		}


		private PixelSprite[] GetDefaultRocks () => new PixelSprite[] {
			new PixelSprite("5_5_2_0_1_1_1_1_0_2863311615_4294967295_4294967295_0_2863311615_2863311615_4294967295_4294967295_4294967295_2863311615_2863311615_4294967295_4294967295_4294967295_2863311615_4294967295_4294967295_4294967295_4294967295_0_4294967295_4294967295_4294967295_0_"),
			new PixelSprite("5_5_2_0_1_1_1_1_0_2863311615_2863311615_4294967295_0_2863311615_2863311615_2863311615_4294967295_4294967295_2863311615_2863311615_4294967295_4294967295_4294967295_0_4294967295_4294967295_4294967295_4294967295_0_0_4294967295_4294967295_0_"),
			new PixelSprite("5_5_2_0_1_1_1_1_0_2863311615_2863311615_4294967295_0_2863311615_2863311615_2863311615_4294967295_4294967295_2863311615_2863311615_4294967295_4294967295_4294967295_2863311615_2863311615_4294967295_4294967295_0_0_4294967295_4294967295_0_0_"),
			new PixelSprite("9_9_4_0_4_2_2_2_0_0_2863311615_2863311615_2863311615_4294967295_4294967295_0_0_0_2863311615_2863311615_2863311615_4294967295_4294967295_4294967295_4294967295_0_2863311615_2863311615_4294967295_2863311615_4294967295_4294967295_4294967295_4294967295_4294967295_2863311615_4294967295_2863311615_4294967295_4294967295_4294967295_4294967295_4294967295_4294967295_2863311615_2863311615_4294967295_2863311615_4294967295_4294967295_4294967295_4294967295_4294967295_2863311615_4294967295_2863311615_4294967295_4294967295_4294967295_4294967295_4294967295_4294967295_0_2863311615_4294967295_4294967295_4294967295_4294967295_4294967295_4294967295_0_0_0_4294967295_4294967295_4294967295_4294967295_4294967295_4294967295_0_0_0_0_0_4294967295_4294967295_4294967295_0_0_"),
		};


		#endregion




	}
}
