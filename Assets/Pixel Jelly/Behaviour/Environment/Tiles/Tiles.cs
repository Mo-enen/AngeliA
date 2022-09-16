using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace PixelJelly {
	// === Main ===
	public partial class Tiles : JellyBehaviour {



		// SUB
		public enum TileStyle {
			Stone, [InspectorName("Stone (Cracked)")] CrackedStone, Marble, Dirt,
			Ice, Sand, Brick, Metal, Iron,
			Floor, Window, Wallpaper,

		}



		// Api
		public override string DisplayGroup => "Environment";
		public override string DisplayName => "Tiles";

		public override int MaxFrameCount => 1;
		public override int MaxSpriteCount => 128;

		// Cache
		protected bool LightR = true;
		protected bool LightT = true;
		private ColorGradient NormalColor = default;
		private Color32 LightColor = default;
		private Color32 DarkColor = default;
		private PixelSprite CatPowSprite = null;


		// MSG
		protected override void OnCreated () {
			Width = 16;
			Height = 16;
			FrameCount = 1;
			SpriteCount = 40;
			InitColorGradients();
		}


		protected override void BeforePopulateAllPixels () {
			Height = Width;
			CatPowSprite = new PixelSprite("5_5_0_0_0_0_0_0_0_4022509823_4022509823_4022509823_0_0_4022509823_4022509823_4022509823_0_4022509823_0_0_0_4022509823_0_0_4022509823_0_0_0_0_0_0_0_");
			m_Light = LightDirection4.TopRight;
			m_Padding = new RectOffset(0, 0, 0, 0);
		}


		protected override void OnPopulatePixels (int size, int _, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2(size / 2f, size / 2f);
			border = new RectOffset(0, 0, 0, 0);
			if (size <= 0 || spriteCount <= 0 || frameCount <= 0) { return; }
			if (frame == 0 && sprite == 0) { CheckMessage(); }
			LightR = m_Light == LightDirection4.BottomRight || m_Light == LightDirection4.TopRight;
			LightT = m_Light == LightDirection4.TopLeft || m_Light == LightDirection4.TopRight;
			var rect = new RectInt(Padding.left, Padding.bottom, size - Padding.right - Padding.left, size - Padding.bottom - Padding.top);
			switch (m_Style) {
				default:

				case TileStyle.Stone:
					SetColors(m_StoneNormal, m_StoneLight, m_StoneDark);
					DrawStoneGroup(rect, 0.5f, rect.width / 8 + 1, rect.height / 8 + 1);
					break;

				case TileStyle.CrackedStone:
					SetColors(m_CrackedStoneNormal, m_CrackedStoneLight, m_CrackedStoneDark);
					DrawCrackedStone(rect, 0.5f, size / 4, m_CrackedStoneCrack);
					break;

				case TileStyle.Marble:
					SetColors(m_MarbleNormal, m_MarbleLight, m_MarbleDark);
					DrawMarble(rect, 0.5f, m_MarbleCrack);
					break;

				case TileStyle.Dirt:
					SetColors(m_DirtNormal, m_DirtLight, m_DirtDark);
					DrawDirt(rect, 0.5f, 0.5f, 3, 3);
					break;

				case TileStyle.Ice:
					SetColors(m_IceNormal, m_IceLight, m_IceDark);
					DrawIce(rect, m_IceSpecular);
					break;

				case TileStyle.Sand:
					SetColors(m_SandNormal, m_SandLight, m_SandDark);
					DrawSand(rect, m_SandTrace);
					break;

				case TileStyle.Brick:
					SetColors(m_BrickNormal, m_BrickLight, m_BrickDark);
					DrawBrickGroup(rect);
					break;

				case TileStyle.Metal:
					SetColors(m_MetalNormal, m_MetalLight, m_MetalDark);
					DrawMetal(rect, true, 0.3f, size / 4, Random.Next(size / 3, Mathf.Max(size - 7, size / 2 + 1)), sprite < spriteCount / 2 ? 0f : 0.2f, m_MetalRust);
					break;

				case TileStyle.Iron:
					SetColors(m_MetalNormal, m_MetalLight, m_MetalDark);
					DrawIron(rect, true, 0.3f, 1, Mathf.Max(Mathf.Max(rect.width, rect.height) / 4, 1), 4, sprite < spriteCount / 2 ? 0f : 0.2f, m_MetalRust);
					break;

				case TileStyle.Floor:
					SetColors(m_FloorNormal, m_FloorLight, m_FloorDark);
					DrawFloorGroup(rect);
					break;

				case TileStyle.Window:
					SetColors(m_WindowNormal, m_WindowLight, m_WindowDark);
					DrawWindow(rect, m_GlassNormal, m_GlassShadow);
					break;

				case TileStyle.Wallpaper:
					SetColors(m_WallpaperNormal, m_WallpaperLight, m_WallpaperDark);
					DrawWallpaper(
						rect, 4, 4, 0, size / 4 + 1,
						m_WallpaperNormal.Color, m_WallpaperDark, m_WallpaperDark,
						CatPowSprite
					);
					break;

			}
		}



		// API
		protected void SetColors (ColorGradient normal, Color32 light, Color32 dark) {
			NormalColor = normal;
			LightColor = light;
			DarkColor = dark;
		}



		// Draw Item
		protected virtual void DrawStoneGroup (RectInt rect, float edge, int countX, int countY) {
			DrawBackground(rect, DarkColor);
			using var _ = new MaskScope(rect);
			Layout_PackingRect(
				rect.width, rect.height, countX, countY,
				(int _x, int _y, int _w, int _h) => DrawStone(
					new RectInt(rect.x + _x + (LightR ? 1 : 0), rect.y + _y + (LightT ? 1 : 0), _w, _h),
					edge, LightColor, DarkColor, NormalColor
				)
			);
		}


		protected virtual void DrawMarble (RectInt rect, float crackAmount, ColorGradient crack) {
			DrawBackground(rect, NormalColor);
			DrawSolidEdge(rect, LightColor, DarkColor, null, null);
			DrawPerlin(rect.Expand(-1), crackAmount, crack);
		}


		protected virtual void DrawDirt (RectInt rect, float edge, float gravel, int gravelGapX, int gravelGapY) {
			DrawBackground(rect, NormalColor);
			DrawEdge(rect, edge, LightColor, DarkColor);
			DrawGravel(rect.Expand(-1), gravel, gravelGapX, gravelGapY, LightColor, DarkColor);
		}


		protected virtual void DrawCrackedStone (RectInt rect, float edge, int pointCount, Color32 crack) {
			DrawBackground(rect, NormalColor, BlendMode.OneMinusAlpha);
			var xs = new double[pointCount];
			var ys = new double[pointCount];
			for (int i = 0; i < pointCount; i++) {
				xs[i] = Random.NextFloat(0f, rect.width);
				ys[i] = Random.NextFloat(0f, rect.height);
			}
			xs[0] = rect.width / 2f;
			ys[0] = rect.height / 2f;
			Layout_Voronoi(rect.size, xs, ys, (p0, p1, siteA, siteB) => {

				var shift = new Vector2Int(LightR ? 1 : -1, 0);
				DrawLine(
					(p0 + rect.position).RoundToInt() + shift,
					(p1 + rect.position).RoundToInt() + shift,
					DarkColor, null, BlendMode.OneMinusAlpha
				);
				DrawLine(
					 (p0 + rect.position).RoundToInt() - shift,
					 (p1 + rect.position).RoundToInt() - shift,
					 LightColor, null, BlendMode.OneMinusAlpha
				 );
				shift = new Vector2Int(0, LightT ? 1 : -1);
				DrawLine(
					(p0 + rect.position).RoundToInt() + shift,
					(p1 + rect.position).RoundToInt() + shift,
					DarkColor, null, BlendMode.OneMinusAlpha
				);
				DrawLine(
					 (p0 + rect.position).RoundToInt() - shift,
					 (p1 + rect.position).RoundToInt() - shift,
					 LightColor, null, BlendMode.OneMinusAlpha
				 );
			});
			Layout_Voronoi(rect.size, xs, ys, (p0, p1, siteA, siteB) => {
				DrawLine(
					(p0 + rect.position).RoundToInt(),
					(p1 + rect.position).RoundToInt(),
					crack, null, BlendMode.OneMinusAlpha
				);
			});
			DrawEdge(rect, edge, LightColor, DarkColor);
		}


		protected void DrawIce (RectInt rect, Color32 specular) => DrawIce(
			rect,
			0.4f, 1, Mathf.Max(Mathf.Max(rect.width, rect.height) / 4, 1),
			rect.width / 3 + Random.Next(-1, 2),
			rect.height / 3 + Random.Next(-1, 2),
			Random.Next(1, 3),
			Random.Next(1, 3),
			specular
		);
		protected virtual void DrawIce (RectInt rect, float highlight, int minHighlightSize, int maxHighlightSize, int minSpecularSize, int maxSpecularSize, int minDarkSpecularSize, int maxDarkSpecularSize, Color32 specular) {
			DrawBackground(rect, NormalColor);
			DrawHighlight(rect.Expand(-1), minHighlightSize, maxHighlightSize, LightT == LightR, highlight, LightColor, DarkColor, NormalColor.GetColor(Random));
			DrawSolidEdge(rect, LightColor, DarkColor);
			if (specular.a > 0) {
				if (minSpecularSize > maxSpecularSize) {
					(minSpecularSize, maxSpecularSize) = (maxSpecularSize, minSpecularSize);
				}
				if (minDarkSpecularSize > maxDarkSpecularSize) {
					(minDarkSpecularSize, maxDarkSpecularSize) = (maxDarkSpecularSize, minDarkSpecularSize);
				}
				DrawSpecular(
					rect.Expand(-2),
					Random.Next(minSpecularSize, maxSpecularSize + 1),
					Random.Next(minSpecularSize, maxSpecularSize + 1),
					Random.Next(minDarkSpecularSize, maxDarkSpecularSize + 1),
					Random.Next(minDarkSpecularSize, maxDarkSpecularSize + 1),
					specular
				);
			}
		}


		protected void DrawSand (RectInt rect, ColorGradient trace) => DrawSand(rect, 0.4f, 0.5f, 3, 3, rect.width / 4, rect.height / 4, rect.width / 2, trace);
		protected virtual void DrawSand (RectInt rect, float edge, float gravel, int gravelGapX, int gravelGapY, int traceCountX, int traceCountY, int traceLength, ColorGradient trace) {
			DrawBackground(rect, NormalColor);
			DrawGravel(rect.Expand(-1), gravel, gravelGapX, gravelGapY, LightColor, DarkColor);
			DrawTrace(rect, traceCountX, traceCountY, traceLength, trace);
			DrawEdge(rect, edge, LightColor, DarkColor);
		}


		protected void DrawBrickGroup (RectInt rect) => DrawBrickGroup(rect, true, 4, rect.width / 2 - rect.width / 5, rect.width / 2 + rect.width / 5);
		protected virtual void DrawBrickGroup (RectInt rect, bool background, int row, int minWidth, int maxWidth) {
			if (minWidth > maxWidth) {
				(minWidth, maxWidth) = (maxWidth, minWidth);
			}
			if (background) {
				DrawBackground(rect, DarkColor);
			}
			using var _mask = new MaskScope(rect);
			Layout_SideBySide(rect.height, row, (_i, _y, _height) => {
				int _width = Mathf.Clamp(Random.Next(minWidth, maxWidth + 1), 1, rect.width - 2);
				int _x = Random.Next(-_width, 0);
				DrawBrick(new RectInt(rect.x + _x, rect.y + _y, _width, _height), LightColor, DarkColor, NormalColor);
				DrawBrick(new RectInt(rect.x + _x + _width, rect.y + _y, _width, _height), LightColor, DarkColor, NormalColor);
				DrawBrick(new RectInt(rect.x + _x + _width * 2, rect.y + _y, rect.width - _width, _height), LightColor, DarkColor, NormalColor);
			});
		}


		protected void DrawMetal (RectInt rect, bool edge, float highlight, int nailCount, int sunkenSize, float rustAmount, Color32 rust) => DrawMetal(rect, edge, highlight, 1, Mathf.Max(Mathf.Max(rect.width, rect.height) / 4, 1), nailCount, nailCount, sunkenSize, sunkenSize, rustAmount, rust);
		protected virtual void DrawMetal (RectInt rect, bool edge, float highlight, int highlightMinSize, int highlightMaxSize, int nailCountX, int nailCountY, int sunkenSizeX, int sunkenSizeY, float rustAmount, Color32 rust) {
			DrawBackground(rect, NormalColor);
			if (highlight.NotAlmostZero() && highlight > 0f) {
				DrawHighlight(rect, highlightMinSize, highlightMaxSize, LightT == LightR, highlight, LightColor, NormalColor.GetColor(Random), DarkColor);
			}
			if (nailCountX > 0 && nailCountY > 0) {
				DrawMetalNails(rect.Expand(-1), nailCountX, nailCountY, sunkenSizeX <= 0 || sunkenSizeY <= 0, LightColor, DarkColor);
			}
			if (sunkenSizeX > 0 && sunkenSizeY > 0) {
				DrawSolidEdge(rect.Expand(
					-(rect.width - sunkenSizeX) / 2,
					-(rect.width - sunkenSizeX) / 2,
					-(rect.height - sunkenSizeY) / 2,
					-(rect.height - sunkenSizeY) / 2
				), DarkColor, LightColor, DarkColor, DarkColor);
			}
			if (edge) {
				DrawSolidEdge(rect, LightColor, DarkColor);
			}
			if (rustAmount.NotAlmostZero() && rustAmount > 0f) {
				DrawPerlin(rect, rustAmount, rust, BlendMode.OneMinusAlpha);
			}
		}


		protected virtual void DrawIron (RectInt rect, bool edge, float highlight, int highlightSizeMin, int highlightSizeMax, int patternSize, float rustAmount, Color32 rust) {
			DrawBackground(rect, NormalColor);
			if (highlight.NotAlmostZero() && highlight > 0f) {
				DrawHighlight(rect, highlightSizeMin, highlightSizeMax, LightT == LightR, highlight, LightColor, NormalColor.GetColor(Random), DarkColor);
			}
			if (patternSize > 0) {
				DrawIronPattern(rect, LightT, patternSize, LightColor, DarkColor);
			}
			if (edge) {
				DrawSolidEdge(rect, LightColor, DarkColor);
			}
			if (rustAmount.NotAlmostZero() && rustAmount > 0f) {
				DrawPerlin(rect, rustAmount, rust, BlendMode.OneMinusAlpha);
			}
		}


		protected void DrawFloorGroup (RectInt rect) => DrawFloorGroup(rect, true, Mathf.Max(rect.height / 4, 1), true);
		protected virtual void DrawFloorGroup (RectInt rect, bool background, int row, bool nail) {
			if (background) {
				DrawBackground(rect, DarkColor);
			}
			using var _mask = new MaskScope(rect);
			Layout_SideBySide(rect.height, row, (_i, _y, _height) => {
				int _x = Random.Next(rect.x, rect.xMax);
				DrawFloor(new RectInt(_x - rect.width, rect.y + _y, rect.width, _height), nail, LightColor, DarkColor, NormalColor);
				DrawFloor(new RectInt(_x, rect.y + _y, rect.width, _height), nail, LightColor, DarkColor, NormalColor);
			});
		}


		protected void DrawWindow (RectInt rect, Color32 glassNormal, Color32 glassShadow) => DrawWindow(
			rect, rect.width / 6, rect.height / 6, true, 0f, glassNormal, glassShadow
		);
		protected virtual void DrawWindow (RectInt rect, int glassCountX, int glassCountY, bool dithering, float hole, Color32 glassNormal, Color32 glassShadow) {
			DrawBackground(rect, NormalColor);
			DrawSolidEdge(rect, LightColor, DarkColor, NormalColor.GetColor(Random), NormalColor.GetColor(Random));
			SetColor(LightR ? rect.xMax - 2 : rect.x + 1, LightT ? rect.yMax - 2 : rect.y + 1, LightColor);
			SetColor(!LightR ? rect.xMax - 2 : rect.x + 1, !LightT ? rect.yMax - 2 : rect.y + 1, DarkColor);
			using var _ = new MaskScope(rect);
			Layout_PackingRect(rect.width - 4, rect.height - 4, glassCountX, glassCountY, (_x, _y, _w, _h) => {
				if (hole.GreaterThanZero() && Random.NextDouble() < hole) { return; }
				DrawGlass(new RectInt(rect.x + _x + 2, rect.y + _y + 2, _w + 1, _h + 1), dithering, LightColor, DarkColor, glassNormal, glassShadow);
			});
		}


		protected virtual void DrawWallpaper (RectInt rect, int stripeCountMin, int stripeCountMax, int patternCountMin, int patternCountMax, Color32 stripeLight, Color32 stripeDark, Color32 stripeNormal, PixelSprite pattern) {
			if (stripeCountMin > stripeCountMax) {
				(stripeCountMin, stripeCountMax) = (stripeCountMax, stripeCountMin);
			}
			if (patternCountMin > patternCountMax) {
				(patternCountMin, patternCountMax) = (patternCountMax, patternCountMin);
			}
			DrawBackground(rect, NormalColor);
			DrawSolidEdge(rect, LightColor, DarkColor, null, null);
			using var _ = new LayerScope();
			DrawStripe(rect, Random.Next(stripeCountMin, stripeCountMax + 1), Random.Next(0, rect.height), stripeLight, stripeDark, stripeNormal);
			DrawPattern(rect.Expand(-1), Random.Next(patternCountMin, patternCountMax + 1), pattern);
		}



	}
}
