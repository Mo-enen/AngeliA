using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelJelly;

namespace PixelJelly {
	public class Coral : JellyBehaviour {



		// VAR
		public override string DisplayName => "Coral";
		public override string DisplayGroup => "Vegetation";
		public override int MaxSpriteCount => 64;

		public override int MaxFrameCount => 1;
		public Vector2 RootRange { get => m_RootRange; set => m_RootRange = value; }
		public Vector2 SubRange { get => m_SubRange; set => m_SubRange = value; }
		public Vector2 Bend { get => m_Bend; set => m_Bend = value; }
		public Vector2Int Thickness { get => m_Thickness; set => m_Thickness = value; }
		public Vector2Int BranchCount { get => m_BranchCount; set => m_BranchCount = value; }
		public ColorGradient[] Colors { get => m_Colors; set => m_Colors = value; }


		// Ser
		[SerializeField, MinMaxSlider(0f, 1f)] private Vector2 m_RootRange = new Vector2(0.25f, 0.75f);
		[SerializeField, MinMaxSlider(0f, 1f)] private Vector2 m_SubRange = new Vector2(0.1f, 0.5f);
		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] private Vector2 m_Bend = new Vector2(0.2f, 0.9f);
		[SerializeField, MinMaxNumber(2)] private Vector2Int m_Thickness = new Vector2Int(2, 4);
		[SerializeField, MinMaxNumber(1, 12)] private Vector2Int m_BranchCount = new Vector2Int(2, 6);
		[SerializeField, ColorGradient] private ColorGradient[] m_Colors = new ColorGradient[1];



		// MSG
		protected override void OnCreated () {
			Width = 32;
			Height = 32;
			FrameCount = 1;
			SpriteCount = 12;
			m_Colors = new ColorGradient[] {
				new ColorGradient(
					new Color32(199, 104, 99, 255), new Color32(181, 86, 86, 255)
				), new ColorGradient(
					new Color32(0, 255, 204, 255), new Color32(0, 179, 192, 255)
				), new ColorGradient(
					new Color32(236, 87, 225, 255), new Color32(173, 51, 188, 255)
				),new ColorGradient(
					new Color32(225, 171, 48, 255), new Color32(188, 123, 13, 255)
				),
			};
		}


		protected override void BeforePopulateAllPixels () {

		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			var rect = new RectInt(0, 0, Width, Height);
			pivot = new Vector2Int(
				(int)Mathf.Lerp(rect.x + rect.width * m_RootRange.x, rect.x + rect.width * m_RootRange.y, Random.NextFloat()),
				rect.y
			);
			border = new RectOffset(0, 0, 0, 0);
			DrawCoral(
				rect, pivot.RoundToInt(), m_SubRange, m_Bend.RandomBetween(Random), m_Thickness.RandomBetween(Random), m_BranchCount.RandomBetween(Random) - 1,
				m_Colors.Length > 0 ? m_Colors.MushroomPick(sprite, spriteCount) : new ColorGradient(new Color32(255, 255, 255, 255), new Color32(220, 220, 220, 255))
			);
		}


		protected void DrawCoral (RectInt rect, Vector2Int pivot, Vector2 subAmount, float bendAmount, int thickness, int deep, ColorGradient gradient) {
			// Start Stem

			int stemWidth = thickness;
			int stemHeight = Random.Next(2, rect.height / 6);
			DrawRect(new RectInt(pivot.x - thickness / 2, pivot.y, stemWidth, stemHeight), gradient, BlendMode.OneMinusAlpha);
			// Branch
			var queue = new Queue<(Vector2Int pivot, int deep, bool right)>();
			queue.Enqueue((
				new Vector2Int(pivot.x, pivot.y + stemHeight),
				0, Random.NextDouble() > 0.5f
			));
			for (int i = 0; i < 1024 && queue.Count > 0; i++) {
				var (_pivot, _deep, _right) = queue.Dequeue();
				int _thickness = Mathf.Max(thickness - _deep, 2);
				int _bHeight = rect.yMax - _pivot.y - _thickness / 2;
				int _endX = _right ?
					rect.xMax - _thickness / 2 - 1 :
					rect.xMin + _thickness / 2;
				_endX = (int)Mathf.Lerp(_endX, _pivot.x, _deep / (deep + 1f));
				DrawBranch(
					_pivot, Mathf.Lerp(subAmount.x, subAmount.y, Random.NextFloat()), bendAmount,
					_endX - _pivot.x,
					_bHeight - (int)(_bHeight / (_deep + 2f)),
					_thickness, gradient, out var subPivot
				);
				// Add Sub Branch
				if (_deep < deep) {
					queue.Enqueue((subPivot, _deep + 1, !_right));
				}
			}
		}


		protected void DrawBranch (Vector2Int pivot, float subAmount, float bendAmount, int rangeX, int rangeY, int thickness, ColorGradient gradient, out Vector2Int subPivot) {
			int _rangeX = Mathf.Abs(rangeX);
			int stepCount = Mathf.Max(_rangeX, rangeY);
			var p0 = pivot;
			var p3 = pivot + new Vector2(rangeX, rangeY);
			var p1 = Vector2.Lerp(p0, p3, 1f / 3f) + new Vector2(Random.Next(-(int)(_rangeX * bendAmount), (int)(_rangeX * bendAmount) + 1), Random.Next(-(int)(rangeY * bendAmount), (int)(rangeY * bendAmount) + 1));
			var p2 = Vector2.Lerp(p0, p3, 2f / 3f) - new Vector2(0f, Random.Next(0, (int)(rangeY * bendAmount) + 1));
			p1.x = Mathf.Clamp(p1.x, p0.x - _rangeX, p0.x + _rangeX);
			p2.x = Mathf.Clamp(p2.x, p0.x - _rangeX, p0.x + _rangeX);
			p1.y = Mathf.Clamp(p1.y, p0.y, p0.y + rangeY);
			p2.y = Mathf.Clamp(p2.y, p0.y, p0.y + rangeY);
			var circle = new Circle(thickness, new Vector2Int(thickness / 2, thickness / 2));
			DrawBezier(p0, p1, p2, p3, stepCount, gradient, circle, BlendMode.OneMinusAlpha);
			subPivot = Util.GetBezierPoint(p0, p1, p2, p3, subAmount).RoundToInt();
		}



	}
}
