using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PixelJelly {
	[HideRandomButton]
	public class MagicCircle : JellyBehaviour {



		// SUB
		[System.Serializable]
		public class Ring {


			// SUB
			public enum RingStyle { Ring = 0, Polygon = 1, Star = 2, Sprite = 3, Letter = 4, }


			// Api
			public string Name => m_Name;
			public RingStyle Style => m_Style;
			public Vector2 Center => m_Center;
			public float Distance => m_Distance;
			public float RadiusA => m_RadiusA;
			public Vector2 RadiusB => m_RadiusB;
			public float Thickness => m_Thickness;
			public int PolySideCount => m_PolySideCount;
			public float SpriteScale => m_SpriteScale;
			public float Revolution => m_Revolution;
			public float Rotation => m_Rotation;
			public int MoreLetter => m_MoreLetter;
			public float Gap => m_Gap;
			public bool Dynamic => m_Dynamic;
			public Color32 Color => m_Color;
			public PixelSprite Sprite => m_Sprite;
			public AnimationCurve CurveCenterX => m_CurveCenterX;
			public AnimationCurve CurveCenterY => m_CurveCenterY;
			public AnimationCurve CurveDistance => m_CurveDistance;
			public AnimationCurve CurveRadiusA => m_CurveRadiusA;
			public AnimationCurve CurveRadiusBX => m_CurveRadiusBX;
			public AnimationCurve CurveRadiusBY => m_CurveRadiusBY;
			public AnimationCurve CurveThickness => m_CurveThickness;
			public AnimationCurve CurveRevolution => m_CurveRevolution;
			public AnimationCurve CurveRotation => m_CurveRotation;
			public AnimationCurve CurveGap => m_CurveGap;
			public AnimationCurve CurveAlpha => m_CurveAlpha;
			public AnimationCurve CurveSpriteScale => m_CurveSpriteScale;
			public PixelSprite[] Letters => m_Letters;

			// Ser
			[SerializeField, Delayed] string m_Name = "";
			[SerializeField, ArrowNumber] RingStyle m_Style = RingStyle.Ring;
			[SerializeField, MinMaxNumber(float.MinValue, float.MaxValue, 0.5f, true)] Vector2 m_Center = Vector2.zero;
			[SerializeField, ArrowNumber(0f)] float m_Distance = 0f;
			[SerializeField, ArrowNumber(0f)] float m_RadiusA = 16f;
			[SerializeField, MinMaxNumber(float.MinValue, float.MaxValue, 1f)] Vector2 m_RadiusB = new Vector2(8f, 16f);
			[SerializeField, ArrowNumber(0f)] float m_Thickness = 1f;
			[SerializeField, ArrowNumber(3)] int m_PolySideCount = 5;
			[SerializeField, ArrowNumber(int.MinValue, int.MaxValue)] int m_MoreLetter = 0;
			[SerializeField, ArrowNumber(0f)] float m_SpriteScale = 1f;
			[SerializeField, ArrowNumber(0f, 360f, 15f, true, true)] float m_Revolution = 0;
			[SerializeField, ArrowNumber(0f, 360f, 15f, true, true)] float m_Rotation = 0;
			[SerializeField, ArrowNumber(0f, 360f, 15f)] float m_Gap = 0;
			[SerializeField] Color32 m_Color = new Color32(255, 255, 255, 255);
			[SerializeField, PixelEditor] PixelSprite m_Sprite = new PixelSprite(1, 1);
			[SerializeField] bool m_Dynamic = false;
			[SerializeField] AnimationCurve m_CurveCenterX = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			[SerializeField] AnimationCurve m_CurveCenterY = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			[SerializeField] AnimationCurve m_CurveDistance = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			[SerializeField] AnimationCurve m_CurveRadiusA = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			[SerializeField] AnimationCurve m_CurveRadiusBX = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			[SerializeField] AnimationCurve m_CurveRadiusBY = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			[SerializeField] AnimationCurve m_CurveSpriteScale = AnimationCurve.Linear(0f, 1f, 1f, 1f);
			[SerializeField] AnimationCurve m_CurveThickness = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			[SerializeField] AnimationCurve m_CurveRevolution = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			[SerializeField] AnimationCurve m_CurveRotation = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			[SerializeField] AnimationCurve m_CurveGap = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			[SerializeField] AnimationCurve m_CurveAlpha = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			[SerializeField, PixelEditor] PixelSprite[] m_Letters = new PixelSprite[0];

		}




		// VAR
		public override string DisplayName => "Magic Circle";
		public override string DisplayGroup => "Prop";
		public override int MaxFrameCount => 128;
		public override int MaxSpriteCount => 1;

		// Ser
		[SerializeField, LabelOnlyAttribute] Ring[] m_Rings = new Ring[0];



		// MSG
		protected override void OnCreated () {
			Width = 64;
			Height = 64;
			FrameCount = 32;
			SpriteCount = 1;
			FrameDuration = 16;
		}


		protected override void BeforePopulateAllPixels () {
			SpriteCount = 1;
		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2(width / 2f, height / 2f);
			border = new RectOffset(0, 0, 0, 0);
			foreach (var ring in m_Rings) {
				DrawMagicCircle((float)frame / frameCount, ring);
			}
		}


		protected virtual void DrawMagicCircle (float frame01, Ring ring) {
			if (ring == null) { return; }

			var color = ring.Color;
			float radiusA = ring.RadiusA;
			var radiusB = ring.RadiusB;
			float thickness = ring.Thickness;
			int sideCount = ring.PolySideCount;
			Vector2 center = ring.Center;
			float revolution = ring.Revolution;
			float rotation = ring.Rotation;
			float gap = ring.Gap;
			float distance = ring.Distance;
			float spriteScale = ring.SpriteScale;

			if (ring.Dynamic) {
				color.a = (byte)Mathf.Clamp(
					color.a * ring.CurveAlpha.Evaluate(frame01),
					0, 255
				);
				radiusA += ring.CurveRadiusA.Evaluate(frame01);
				radiusB.x += ring.CurveRadiusBX.Evaluate(frame01);
				radiusB.y += ring.CurveRadiusBY.Evaluate(frame01);
				thickness += ring.CurveThickness.Evaluate(frame01);
				center.x += ring.CurveCenterX.Evaluate(frame01);
				center.y += ring.CurveCenterY.Evaluate(frame01);
				revolution += (int)ring.CurveRevolution.Evaluate(frame01);
				rotation += ring.CurveRotation.Evaluate(frame01);
				gap += ring.CurveGap.Evaluate(frame01);
				distance += ring.CurveDistance.Evaluate(frame01);
				spriteScale *= ring.CurveSpriteScale.Evaluate(frame01);
			}

			var _center = ((Vector2)((Vector3)center + Quaternion.Euler(0f, 0f, -revolution) * (Vector2.up * distance))).FloorToInt();

			switch (ring.Style) {
				default:
				case Ring.RingStyle.Ring:
					DrawRing(_center, radiusA, rotation, 360f - gap, thickness, color, BlendMode.OneMinusAlpha);
					break;
				case Ring.RingStyle.Polygon:
					DrawPolygonRing(_center, radiusA, thickness, rotation, 360f - gap, sideCount, color, BlendMode.OneMinusAlpha);
					break;
				case Ring.RingStyle.Star:
					DrawPolygonRing(_center, radiusB.y, radiusB.x, thickness, rotation, 360f - gap, sideCount, color, BlendMode.OneMinusAlpha);
					break;
				case Ring.RingStyle.Sprite:
					var sprite = ring.Sprite;
					if (sprite == null) { break; }
					if (rotation != 0 || spriteScale.NotAlmost(1f)) {
						sprite = sprite.CreateRS(rotation, Vector3.one * spriteScale);
					}
					DrawSprite(
						new RectInt((int)center.x, (int)center.y, sprite.Width, sprite.Height),
						sprite, color, SpriteScaleMode.Original, BlendMode.OneMinusAlpha
					);
					break;
				case Ring.RingStyle.Letter:
					if (ring.Letters == null || ring.Letters.Length == 0) { break; }
					Layout_Ring(ring.Letters.Length + ring.MoreLetter, radiusA, rotation, 360f - gap, (i, j, index, angle) => {
						var letter = ring.Letters[index % ring.Letters.Length];
						var sprite = letter.CreateRS(angle, Vector2.one * spriteScale);
						DrawSprite(
							new RectInt(center.x.RoundToInt() + i, center.y.RoundToInt() + j, sprite.Width, sprite.Height),
							sprite, color, SpriteScaleMode.Original, BlendMode.OneMinusAlpha
						);
					});
					break;
			}
		}


	}
}
