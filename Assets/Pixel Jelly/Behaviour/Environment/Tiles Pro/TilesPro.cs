using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PixelJelly {
	// === Main ===
	public partial class TilesPro : BrokenTiles {




		// Api
		public override string DisplayName => "Tiles Pro";



		// MSG
		protected override void OnCreated () {
			base.OnCreated();
			Padding = new RectOffset(0, 0, 0, 0);
			m_WallpaperPattern = new PixelSprite("5_5_0_0_0_0_0_0_0_4022509823_4022509823_4022509823_0_0_4022509823_4022509823_4022509823_0_4022509823_0_0_0_4022509823_0_0_4022509823_0_0_0_0_0_0_0_");
		}


		protected override void BeforePopulateAllPixels () {
			FrameCount = 1;
			if (Width > 128) {
				Width = 128;
			}
			if (Height > 128) {
				Height = 128;
			}
			if (Padding == null) {
				Padding = new RectOffset(0, 0, 0, 0);
			}
		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2Int(width / 2, height / 2);
			border = new RectOffset(0, 0, 0, 0);
			if (width <= 0 || height <= 0 || spriteCount <= 0 || frameCount <= 0) { return; }
			if (frame == 0 && sprite == 0) { CheckMessage(); }
			LightR = Light == LightDirection4.TopRight || Light == LightDirection4.BottomRight;
			LightT = Light == LightDirection4.TopRight || Light == LightDirection4.TopLeft;
			var rect = new RectInt(Padding.left, Padding.bottom, width - Padding.right - Padding.left, height - Padding.bottom - Padding.top);
			switch (Style) {
				case TileStyle.Stone:
					SetColors(StoneNormal, StoneLight, StoneDark);
					DrawStoneGroup(rect, m_StoneEdge.RandomBetween(Random), m_StoneGroupX.RandomBetween(Random), m_StoneGroupY.RandomBetween(Random));
					break;

				case TileStyle.CrackedStone:
					SetColors(CrackedStoneNormal, CrackedStoneLight, CrackedStoneDark);
					DrawCrackedStone(rect, m_CrackedStoneEdge.RandomBetween(Random), m_CrackedStonePointCount.RandomBetween(Random), CrackedStoneCrack);
					break;

				case TileStyle.Marble:
					SetColors(MarbleNormal, MarbleLight, MarbleDark);
					DrawMarble(rect, m_MarbleCrackAmount.RandomBetween(Random), MarbleCrack);
					break;

				case TileStyle.Dirt:
					SetColors(DirtNormal, DirtLight, DirtDark);
					DrawDirt(rect, m_DirtEdge.RandomBetween(Random), m_DirtGravel.RandomBetween(Random), m_DirtGravelGapX.RandomBetween(Random), m_DirtGravelGapY.RandomBetween(Random));
					break;

				case TileStyle.Ice:
					SetColors(IceNormal, IceLight, IceDark);
					DrawIce(
						rect,
						m_IceHighlight.RandomBetween(Random), m_IceHighlightSize.x, m_IceHighlightSize.y,
						m_IceSpecularSize.x, m_IceSpecularSize.y,
						m_IceDarkSpecularSize.x, m_IceDarkSpecularSize.y,
						IceSpecular
					);
					break;

				case TileStyle.Sand:
					SetColors(SandNormal, SandLight, SandDark);
					DrawSand(
						rect,
						m_SandEdge.RandomBetween(Random), m_SandGravel.RandomBetween(Random), m_SandGravelGapX.RandomBetween(Random), m_SandGravelGapY.RandomBetween(Random),
						m_SandTraceCountX.RandomBetween(Random), m_SandTraceCountY.RandomBetween(Random), m_SandTraceLength.RandomBetween(Random),
						SandTrace
					);
					break;

				case TileStyle.Brick:
					SetColors(BrickNormal, BrickLight, BrickDark);
					DrawBrickGroup(rect, true, m_BrickRow.RandomBetween(Random), m_BrickWidth.x, m_BrickWidth.y);
					break;

				case TileStyle.Metal:
					SetColors(MetalNormal, MetalLight, MetalDark);
					DrawMetal(
						rect, m_MetalEdge,
						m_MetalHighlightTint.RandomBetween(Random), m_MetalHighlight.x, m_MetalHighlight.y,
						m_MetalNailCountX.RandomBetween(Random), m_MetalNailCountY.RandomBetween(Random), m_MetalSunkenSizeX.RandomBetween(Random), m_MetalSunkenSizeY.RandomBetween(Random),
						m_MetalRustAmount.RandomBetween(Random), MetalRust
					);
					break;

				case TileStyle.Iron:
					SetColors(MetalNormal, MetalLight, MetalDark);
					DrawIron(rect, m_IronEdge, m_IronHighlightTint.RandomBetween(Random), m_IronHighlight.x, m_IronHighlight.y, m_IronPatternSize.RandomBetween(Random), m_IronRustAmount.RandomBetween(Random), MetalRust);
					break;

				case TileStyle.Floor:
					SetColors(FloorNormal, FloorLight, FloorDark);
					DrawFloorGroup(rect, true, m_FloorRow.RandomBetween(Random), m_FloorNail);
					break;

				case TileStyle.Window:
					SetColors(WindowNormal, WindowLight, WindowDark);
					DrawWindow(rect, m_WindowGlassColumn.RandomBetween(Random), m_WindowGlassRow.RandomBetween(Random), m_WindowGlassDithering, 0f, GlassNormal, GlassShadow);
					break;

				case TileStyle.Wallpaper:
					SetColors(WallpaperNormal, WallpaperLight, WallpaperDark);
					DrawWallpaper(
						rect,
						m_WallpaperStripeCount.x, m_WallpaperStripeCount.y,
						m_WallpaperPatternCount.x, m_WallpaperPatternCount.y,
						WallpaperNormal.Color, WallpaperDark, WallpaperDark,
						m_WallpaperPattern
					);
					break;
			}
		}


	}
}
