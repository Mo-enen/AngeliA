using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace PixelJelly {
	[HideInPixelJelly]
	public class BrokenTiles : Tiles {


		// Api
		public float Hole { get => m_Hole; set => m_Hole = value; }
		public float Broken { get => m_Broken; set => m_Broken = value; }
		public Color32 StoneBrokenEdge { get => m_StoneBrokenEdge; set => m_StoneBrokenEdge = value; }
		public Color32 FloorBrokenEdge { get => m_FloorBrokenEdge; set => m_FloorBrokenEdge = value; }
		public Color32 WindowBrokenEdge { get => m_WindowBrokenEdge; set => m_WindowBrokenEdge = value; }
		public Color32 BrickBrokenEdge { get => m_BrickBrokenEdge; set => m_BrickBrokenEdge = value; }
		public bool StoneHoleWithBackground { get => m_StoneHoleWithBackground; set => m_StoneHoleWithBackground = value; }
		public bool BrickHoleWithBackground { get => m_BrickHoleWithBackground; set => m_BrickHoleWithBackground = value; }
		public bool FloorHoleWithBackground { get => m_FloorHoleWithBackground; set => m_FloorHoleWithBackground = value; }
		public bool BrokeFromEdge { get => m_BrokeFromEdge; set => m_BrokeFromEdge = value; }
		public Color32 CrackedStoneBrokenEdge { get => m_CrackedStoneBrokenEdge; set => m_CrackedStoneBrokenEdge = value; }

		// Ser
		[SerializeField, ArrowNumber(0f, 1f, 0.05f)] private float m_Hole = 0f;
		[SerializeField, ArrowNumber(0f, 1f, 0.05f)] private float m_Broken = 0f;
		[SerializeField] bool m_StoneHoleWithBackground = true;
		[SerializeField] bool m_BrickHoleWithBackground = true;
		[SerializeField] bool m_FloorHoleWithBackground = true;
		[SerializeField] bool m_BrokeFromEdge = true;
		[SerializeField] Color32 m_StoneBrokenEdge = new Color32(76, 71, 70, 255);
		[SerializeField] Color32 m_CrackedStoneBrokenEdge = new Color32(76, 71, 70, 255);
		[SerializeField] Color32 m_FloorBrokenEdge = new Color32(92, 57, 46, 255);
		[SerializeField] Color32 m_WindowBrokenEdge = new Color32(58, 45, 51, 255);
		[SerializeField] Color32 m_BrickBrokenEdge = new Color32(61, 48, 54, 255);
		[SerializeField] Color32 m_MarbleBrokenEdge = new Color32(85, 79, 78, 255);
		[SerializeField] Color32 m_DirtBrokenEdge = new Color32(85, 52, 42, 255);
		[SerializeField] Color32 m_IceBrokenEdge = new Color32(177, 221, 221, 255);
		[SerializeField] Color32 m_MetalBrokenEdge = new Color32(194, 210, 226, 255);
		[SerializeField] Color32 m_WallpaperBrokenEdge = new Color32(226, 226, 226, 255);
		[SerializeField] Color32 m_SandBrokenEdge = new Color32(206, 169, 89, 255);



		// Broken/Hole
		protected override void DrawStoneGroup (RectInt rect, float edge, int countX, int countY) => DrawWithBroken(rect, () => base.DrawStoneGroup(rect, edge, countX, countY),
			m_StoneBrokenEdge
		);
		protected override void DrawStone (RectInt rect, float edge, Color32 light, Color32 dark, ColorGradient normal) => DrawWithHole(
			() => base.DrawStone(rect, edge, light, dark, normal),
			m_Hole, m_StoneHoleWithBackground ? new RectInt(0, 0, 0, 0) : rect.Expand(LightR ? 1 : 0, LightR ? 0 : 1, LightT ? 1 : 0, LightT ? 0 : 1)
		);


		protected override void DrawCrackedStone (RectInt rect, float edge, int point, Color32 crack) => DrawWithBroken(rect, () => base.DrawCrackedStone(rect, edge, point, crack), m_CrackedStoneBrokenEdge);


		protected override void DrawMarble (RectInt rect, float crackAmount, ColorGradient crack) => DrawWithBroken(rect, () => base.DrawMarble(rect, crackAmount, crack), m_MarbleBrokenEdge);


		protected override void DrawDirt (RectInt rect, float edge, float gravel, int gravelGapX, int gravelGapY) => DrawWithBroken(rect, () => base.DrawDirt(rect, edge, gravel, gravelGapX, gravelGapY), m_DirtBrokenEdge);


		protected override void DrawIce (RectInt rect, float highlight, int minHighlightSize, int maxHighlightSize, int minSpecularSize, int maxSpecularSize, int minDarkSpecularSize, int maxDarkSpecularSize, Color32 specular) => DrawWithBroken(rect, () => base.DrawIce(rect, highlight, minHighlightSize, maxHighlightSize, minSpecularSize, maxSpecularSize, minDarkSpecularSize, maxDarkSpecularSize, specular), m_IceBrokenEdge);


		protected override void DrawBrickGroup (RectInt rect, bool _, int row, int minWidth, int maxWidth) => DrawWithBroken(rect, () => base.DrawBrickGroup(rect, m_BrickHoleWithBackground, row, minWidth, maxWidth), m_BrickBrokenEdge);
		protected override void DrawBrick (RectInt rect, Color32 light, Color32 dark, ColorGradient normal) => DrawWithHole(
			() => base.DrawBrick(rect, light, dark, normal), m_Hole
		);


		protected override void DrawMetal (RectInt rect, bool edge, float highlight, int highlightMinSize, int highlightMaxSize, int nailCountX, int nailCountY, int sunkenSizeX, int sunkenSizeY, float rustAmount, Color32 rust) => DrawWithBroken(rect, () => base.DrawMetal(rect, edge, highlight, highlightMinSize, highlightMaxSize, nailCountX, nailCountY, sunkenSizeX, sunkenSizeY, rustAmount, rust), m_MetalBrokenEdge);


		protected override void DrawIron (RectInt rect, bool edge, float highlight, int highlightSizeMin, int highlightSizeMax, int patternSize, float rustAmount, Color32 rust) => DrawWithBroken(rect, () => base.DrawIron(rect, edge, highlight, highlightSizeMin, highlightSizeMax, patternSize, rustAmount, rust), m_MetalBrokenEdge);


		protected override void DrawFloorGroup (RectInt rect, bool _, int row, bool nail) => DrawWithBroken(rect, () => base.DrawFloorGroup(rect, m_FloorHoleWithBackground, row, nail), m_FloorBrokenEdge);
		protected override void DrawFloor (RectInt rect, bool nail, Color32 light, Color32 dark, ColorGradient normal) => DrawWithHole(
			() => base.DrawFloor(rect, nail, light, dark, normal), m_Hole
		);


		protected override void DrawWindow (RectInt rect, int glassCountX, int glassCountY, bool dithering, float _, Color32 glassNormal, Color32 glassShadow) => DrawWithBroken(rect, () => base.DrawWindow(rect, glassCountX, glassCountY, dithering, m_Hole, glassNormal, glassShadow), m_WindowBrokenEdge);


		protected override void DrawWallpaper (RectInt rect, int stripeCountMin, int stripeCountMax, int patternCountMin, int patternCountMax, Color32 stripeLight, Color32 stripeDark, Color32 stripeNormal, PixelSprite pattern) => DrawWithBroken(rect, () => base.DrawWallpaper(rect, stripeCountMin, stripeCountMax, patternCountMin, patternCountMax, stripeLight, stripeDark, stripeNormal, pattern), m_WallpaperBrokenEdge);


		protected override void DrawSand (RectInt rect, float edge, float gravel, int gravelGapX, int gravelGapY, int traceCountX, int traceCountY, int traceLength, ColorGradient trace) => DrawWithBroken(rect, () => base.DrawSand(rect, edge, gravel, gravelGapX, gravelGapY, traceCountX, traceCountY, traceLength, trace), m_SandBrokenEdge);



		// LGC
		private void DrawWithHole (System.Action draw, float hole, RectInt? clearRect = null) {
			if (hole.NotAlmostZero() && hole > 0f && Random.NextDouble() < hole) {
				if (clearRect.HasValue) {
					DrawBackground(clearRect.Value, new Color32(0, 0, 0, 0));
				}
				return;
			}
			draw();
		}


		private void DrawWithBroken (RectInt rect, System.Action draw, Color32 edge) {
			if (m_Broken.GreaterThanZero()) {

				draw();
				float offsetX = (float)Random.NextDouble() * rect.width;
				float offsetY = (float)Random.NextDouble() * rect.height;
				BrokenErase(rect, offsetX, offsetY, m_Broken, m_BrokeFromEdge, new Color32(0, 0, 0, 0));
				if (edge.a > 0) {
					BrokenErase(rect, offsetX, offsetY, m_Broken + 0.05f, m_BrokeFromEdge, edge);
				}

			} else {
				draw();
			}
		}


		private void BrokenErase (RectInt rect, float offsetX, float offsetY, float amount, bool fromEdge, Color32 color) {
			if (amount.GreaterThan(1f)) {
				var CLEAR = new Color32(0, 0, 0, 0);
				for (int j = 0; j < rect.height; j++) {
					for (int i = 0; i < rect.width; i++) {
						SetColor(i + rect.x, j + rect.y, CLEAR);
					}
				}
				return;
			}
			// Perlin to Cache
			var blend = color.a == 0 ? BlendMode.Override : BlendMode.OneMinusAlpha;
			Vector2 center = default, pos = default;
			float posMulti = 0f;
			if (fromEdge) {
				center = new Vector2(rect.width / 2f, rect.height / 2f);
				pos = Vector2.zero;
				posMulti = Vector2.Distance(Vector2.zero, rect.size) / 2f;
			}
			for (int j = 0; j < rect.height; j++) {
				for (int i = 0; i < rect.width; i++) {
					float value = Mathf.PerlinNoise(
						i / (float)rect.width * 2f + offsetX,
						j / (float)rect.height * 2f + offsetY
					);
					pos.x = i;
					pos.y = j;
					if (value <= amount && (!fromEdge || Vector2.Distance(pos, center) >= (1f - amount) * posMulti)) {
						if (color.a != 0 && GetColor(i + rect.x, j + rect.y).a == 0) { continue; }
						SetColor(i + rect.x, j + rect.y, color, blend);
					}
				}
			}
		}


		protected override void CheckMessage () {
			base.CheckMessage();
			if (m_Hole.Almost(1f) || m_Hole > 1f) {
				AddInspectorMessage("Hole is too large", MessageType.Warning, "m_Hole");
			}
			if (m_Broken.Almost(1f) || m_Broken > 1f) {
				AddInspectorMessage("Broken is too large", MessageType.Warning, "m_Broken");
			}

		}


	}
}
