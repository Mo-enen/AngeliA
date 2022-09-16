using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



namespace PixelJelly.Editor {
	[CustomJellyInspector(typeof(TilesPro))]
	public class TilesPro_Inspector : BrokenTiles_Inspector {


		public override void OnPropertySwape (SerializedObject serializedObject) {

			base.OnPropertySwape(serializedObject);

			var style = (serializedObject.targetObject as Tiles).Style;

			SwapeProperty("m_Width", ".");
			SwapeProperty("m_Height", ".");
			SwapeProperty("m_FrameCount", "");
			SwapeProperty("m_FrameDuration", "");
			
			SwapeProperty("m_Light", ".");
			SwapeProperty("m_Padding", ".");

			// Stone
			SwapeProperty("m_StoneEdge", style == Tiles.TileStyle.Stone ? "Edge" : "");
			SwapeProperty("m_StoneGroupX", style == Tiles.TileStyle.Stone ? "Group X" : "");
			SwapeProperty("m_StoneGroupY", style == Tiles.TileStyle.Stone ? "Group Y" : "");

			// C Stone
			SwapeProperty("m_CrackedStoneEdge", style == Tiles.TileStyle.CrackedStone ? "Edge" : "");
			SwapeProperty("m_CrackedStonePointCount", style == Tiles.TileStyle.CrackedStone ? "Point" : "");

			// Marble
			SwapeProperty("m_MarbleCrackAmount", style == Tiles.TileStyle.Marble ? "Crack" : "");

			// Dirt
			SwapeProperty("m_DirtEdge", style == Tiles.TileStyle.Dirt ? "Edge" : "");
			SwapeProperty("m_DirtGravel", style == Tiles.TileStyle.Dirt ? "Gravel Amount" : "");
			SwapeProperty("m_DirtGravelGapX", style == Tiles.TileStyle.Dirt ? "Gravel Gap X" : "");
			SwapeProperty("m_DirtGravelGapY", style == Tiles.TileStyle.Dirt ? "Gravel Gap Y" : "");

			// Ice
			SwapeProperty("m_IceHighlight", style == Tiles.TileStyle.Ice ? "Highlight Tint" : "");
			SwapeProperty("m_IceHighlightSize", style == Tiles.TileStyle.Ice ? "Highlight Size" : "");
			SwapeProperty("m_IceSpecularSize", style == Tiles.TileStyle.Ice ? "Specular A" : "");
			SwapeProperty("m_IceDarkSpecularSize", style == Tiles.TileStyle.Ice ? "Specular B" : "");

			// Sand
			SwapeProperty("m_SandEdge", style == Tiles.TileStyle.Sand ? "Edge" : "");
			SwapeProperty("m_SandGravel", style == Tiles.TileStyle.Sand ? "Gravel Amount" : "");
			SwapeProperty("m_SandGravelGapX", style == Tiles.TileStyle.Sand ? "Gravel Gap X" : "");
			SwapeProperty("m_SandGravelGapY", style == Tiles.TileStyle.Sand ? "Gravel Gap Y" : "");
			SwapeProperty("m_SandTraceCountX", style == Tiles.TileStyle.Sand ? "Trace Count X" : "");
			SwapeProperty("m_SandTraceCountY", style == Tiles.TileStyle.Sand ? "Trace Count Y" : "");
			SwapeProperty("m_SandTraceLength", style == Tiles.TileStyle.Sand ? "Trace Length" : "");

			// Brick
			SwapeProperty("m_BrickRow", style == Tiles.TileStyle.Brick ? "Row" : "");
			SwapeProperty("m_BrickWidth", style == Tiles.TileStyle.Brick ? "Width" : "");

			// Metal
			SwapeProperty("m_MetalEdge", style == Tiles.TileStyle.Metal ? "Edge" : "");
			SwapeProperty("m_MetalHighlightTint", style == Tiles.TileStyle.Metal ? "Highlight Tint" : "");
			SwapeProperty("m_MetalHighlight", style == Tiles.TileStyle.Metal ? "Highlight Size" : "");
			SwapeProperty("m_MetalNailCountX", style == Tiles.TileStyle.Metal ? "Nail Count X" : "");
			SwapeProperty("m_MetalNailCountY", style == Tiles.TileStyle.Metal ? "Nail Count Y" : "");
			SwapeProperty("m_MetalSunkenSizeX", style == Tiles.TileStyle.Metal ? "Sunken Size X" : "");
			SwapeProperty("m_MetalSunkenSizeY", style == Tiles.TileStyle.Metal ? "Sunken Size Y" : "");
			SwapeProperty("m_MetalRustAmount", style == Tiles.TileStyle.Metal ? "Rust" : "");

			// Iron
			SwapeProperty("m_IronEdge", style == Tiles.TileStyle.Iron ? "Edge" : "");
			SwapeProperty("m_IronHighlightTint", style == Tiles.TileStyle.Iron ? "Highlight Tint" : "");
			SwapeProperty("m_IronHighlight", style == Tiles.TileStyle.Iron ? "Highlight Size" : "");
			SwapeProperty("m_IronPatternSize", style == Tiles.TileStyle.Iron ? "Pattern Size" : "");
			SwapeProperty("m_IronRustAmount", style == Tiles.TileStyle.Iron ? "Rust" : "");

			// Floor
			SwapeProperty("m_FloorNail", style == Tiles.TileStyle.Floor ? "Nail" : "");
			SwapeProperty("m_FloorRow", style == Tiles.TileStyle.Floor ? "Row" : "");

			// Window
			SwapeProperty("m_WindowGlassColumn", style == Tiles.TileStyle.Window ? "Glass Column" : "");
			SwapeProperty("m_WindowGlassRow", style == Tiles.TileStyle.Window ? "Glass Row" : "");
			SwapeProperty("m_WindowGlassDithering", style == Tiles.TileStyle.Window ? "Glass Dithering" : "");

			// Wallpaper
			SwapeProperty("m_WallpaperStripeCount", style == Tiles.TileStyle.Wallpaper ? "Stripe Count" : "");
			SwapeProperty("m_WallpaperPatternCount", style == Tiles.TileStyle.Wallpaper ? "Pattern Count" : "");

		}


	}
}
