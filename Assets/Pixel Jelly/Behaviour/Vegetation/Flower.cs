using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PixelJelly {
	public class Flower : JellyBehaviour {




		#region --- VAR ---


		// Api
		public override string DisplayName => "Flower";
		public override string DisplayGroup => "Vegetation";

		public override int MaxSpriteCount => 64;
		public override int MaxFrameCount => 1;
		public int Tilt { get => m_Tilt; set => m_Tilt = value; }
		public int Thickness { get => m_Thickness; set => m_Thickness = value; }
		public Vector2Int LeafLength { get => m_LeafLength; set => m_LeafLength = value; }
		public Vector2Int LeafCount { get => m_LeafCount; set => m_LeafCount = value; }
		public AnimationCurve LeafShape { get => m_LeafShape; set => m_LeafShape = value; }
		public Color32 Normal { get => m_Normal; set => m_Normal = value; }
		public Color32 Dark { get => m_Dark; set => m_Dark = value; }
		public PixelSprite[] Flowers { get => m_Flowers; set => m_Flowers = value; }

		// Ser
		[SerializeField, ArrowNumber(1)] private int m_Tilt = 6;
		[SerializeField, ArrowNumber(1)] private int m_Thickness = 1;
		[SerializeField, MinMaxNumber(1, 64)] private Vector2Int m_LeafLength = new Vector2Int(3, 5);
		[SerializeField, MinMaxNumber(0, 128)] private Vector2Int m_LeafCount = new Vector2Int(1, 3);
		[SerializeField] AnimationCurve m_LeafShape = new AnimationCurve();
		[SerializeField] Color32 m_Normal = new Color32(58, 166, 105, 255);
		[SerializeField] Color32 m_Dark = new Color32(60, 115, 96, 255);
		[SerializeField, PixelEditor] private PixelSprite[] m_Flowers = new PixelSprite[1];


		#endregion




		#region --- MSG ---


		protected override void OnCreated () {
			Width = 16;
			Height = 16;
			SpriteCount = 24;
			FrameCount = 1;
			m_Flowers = GetDefaultFlowers();
			m_LeafShape = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.5f);
		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2(width / 2f, 0);
			border = new RectOffset(0, 0, 0, 0);
			if (width <= 0 || height <= 0 || frameCount <= 0 || SpriteCount <= 0) { return; }
			if (sprite == 0 && frame == 0) { CheckInspectorMsg(width); }
			DrawFlower(
				new RectInt(0, 0, width, height - Random.Next(0, 4)),
				m_Thickness, m_Tilt, m_LeafLength, m_LeafCount,
				m_Normal, m_Dark, m_LeafShape, m_Flowers != null && m_Flowers.Length > 0 ? m_Flowers[Random.Next(0, m_Flowers.Length)] : null
			);
		}


		#endregion




		#region --- API ---


		protected void DrawFlower (
			RectInt rect, int thickness, int tilt,
			Vector2Int leafLength, Vector2Int leafCount,
			Color32 normal, Color32 dark, AnimationCurve leafShape, PixelSprite flower
		) {
			int flowerHeight = 0;
			int flowerPivotY = 0;
			if (flower != null) {
				flowerHeight = flower.Height;
				flowerPivotY = Mathf.Clamp(flower.PivotY, 0, flowerHeight - 1);
			}
			// Stem
			var stem0 = new Vector2(rect.x + rect.width / 2, rect.y);
			var stem3 = new Vector2(stem0.x, rect.yMax - flowerHeight + flowerPivotY);
			var stem1 = Vector2.Lerp(stem0, stem3, 1f / 3f);
			var stem2 = Vector2.Lerp(stem0, stem3, 2f / 3f);
			stem1.x += Random.Next(-tilt, tilt + 1);
			stem2.x += Random.Next(-tilt, tilt + 1);
			int stemHeight = (int)(stem3.y - stem0.y);
			Vector2Int prevPos = Util.GetBezierPoint(stem0, stem1, stem2, stem3, 0f).RoundToInt();
			var stemShape = new Rectangle(thickness, thickness);
			for (int i = 1; i <= stemHeight; i++) {
				var pos = Util.GetBezierPoint(stem0, stem1, stem2, stem3, (float)i / stemHeight).RoundToInt();
				DrawLine(prevPos, pos, Random.NextDouble() > 0.5f ? normal : dark, stemShape);
				prevPos = pos;
			}
			// Leaf
			int _leafCount = Random.Next(leafCount.x, leafCount.y + 1);
			for (int i = 0; i < _leafCount; i++) {
				float t01 = i == 0 ? 0f : Random.NextFloat();
				var pos = Util.GetBezierPoint(stem0, stem1, stem2, stem3, t01).RoundToInt();
				var dir = Util.GetBezierVelocity(stem0, stem1, stem2, stem3, t01).normalized;
				(dir.x, dir.y) = (dir.y, dir.x);
				if (dir.y < 0f) {
					dir *= -1f;
				}
				dir.Normalize();
				DrawLeaf(pos, Random.Next(leafLength.x, leafLength.y + 1), dir, Random.NextDouble() > 0.5f ? normal : dark, leafShape);
			}
			// Flower
			if (flower != null) {
				var pos = Util.GetBezierPoint(stem0, stem1, stem2, stem3, 1f).RoundToInt();
				DrawSprite(
					new RectInt(pos.x, pos.y, flower.Width, flower.Height),
					flower,
					new Color32(255, 255, 255, 255),
					SpriteScaleMode.Original
				);
			}
		}


		protected void DrawLeaf (Vector2Int pos, int length, Vector2 dir, Color32 color, AnimationCurve leafShape) {
			Vector2 step = dir;
			Vector2 _pos = pos;
			for (int i = 0; i < length; i++) {
				var p = _pos.RoundToInt();
				int size = Mathf.RoundToInt(leafShape.Evaluate(i / (length - 1f)) * length);
				size = Mathf.Max(size, 0);
				p.y -= size - 1;
				DrawRect(new RectInt(p.x, p.y, size, size), color, BlendMode.OneMinusAlpha);
				_pos += step;
			}
		}


		#endregion




		#region --- LGC ---


		private void CheckInspectorMsg (int width) {
			if (m_Thickness >= width) {
				AddInspectorMessage("Thickness is too large.", MessageType.Warning, "m_Thickness");
			}
			if (m_Flowers == null || m_Flowers.Length == 0) {
				AddInspectorMessage("Flowers is empty.", MessageType.Warning, "m_Flowers");
			}
		}


		private PixelSprite[] GetDefaultFlowers () => new PixelSprite[] {
			new PixelSprite("6_6_2_2_0_0_0_0_0_0_3342486271_3342486271_0_0_0_0_3342486271_3342486271_0_0_3342486271_3342486271_4294967295_4294967295_3342486271_3342486271_3342486271_3342486271_4294967295_4294967295_3342486271_3342486271_0_0_3342486271_3342486271_0_0_0_0_3342486271_3342486271_0_0_"),
			new PixelSprite("6_6_2_2_0_0_0_0_0_4032190207_4032190207_0_0_0_0_4032190207_4032190207_3342486271_4032190207_4032190207_0_3342486271_4294967295_4294967295_4032190207_4032190207_4032190207_4032190207_4294967295_4294967295_3342486271_0_4032190207_4032190207_3342486271_4032190207_4032190207_0_0_0_0_4032190207_4032190207_0_"),
			new PixelSprite("6_6_2_0_0_0_0_0_0_0_4032190207_4032190207_0_0_0_4032190207_4032190207_4032190207_4032190207_0_0_4032190207_4032190207_3342486271_4032190207_0_4032190207_4032190207_3342486271_4294967295_3342486271_4032190207_4032190207_3342486271_4294967295_3342486271_4294967295_3342486271_0_4032190207_3342486271_4294967295_3342486271_0_"),
			new PixelSprite("6_6_2_0_0_0_0_0_0_0_4032190207_4032190207_0_0_0_4032190207_4032190207_3342486271_4032190207_0_4032190207_4032190207_3342486271_3342486271_3342486271_4032190207_4032190207_3342486271_3342486271_4294967295_3342486271_3342486271_0_3342486271_4294967295_3342486271_4294967295_0_0_0_3342486271_4294967295_0_0_"),
		};


	}


	#endregion



}