using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace PixelJelly.Editor {
	[CustomJellyInspector(typeof(Tiles))]
	public class Tiles_Inspector : JellyInspector {
		public override void OnPropertySwape (SerializedObject serializedObject) {

			var style = (serializedObject.targetObject as Tiles).Style;


			SwapeProperty("m_Width", "Size");
			SwapeProperty("m_Height", "");
			SwapeProperty("m_FrameCount", "");
			
			SwapeProperty("m_FrameDuration", "");
			SwapeProperty("m_Light", "");
			SwapeProperty("m_Padding", "");


			// Dirt Color
			SwapeProperty("m_DirtLight", style == Tiles.TileStyle.Dirt ? "Light" : "");
			SwapeProperty("m_DirtNormal", style == Tiles.TileStyle.Dirt ? "Normal" : "");
			SwapeProperty("m_DirtDark", style == Tiles.TileStyle.Dirt ? "Dark" : "");

			// Cracked Stone
			SwapeProperty("m_CrackedStoneLight", style == Tiles.TileStyle.CrackedStone ? "Light" : "");
			SwapeProperty("m_CrackedStoneNormal", style == Tiles.TileStyle.CrackedStone ? "Normal" : "");
			SwapeProperty("m_CrackedStoneDark", style == Tiles.TileStyle.CrackedStone ? "Dark" : "");
			SwapeProperty("m_CrackedStoneCrack", style == Tiles.TileStyle.CrackedStone ? "Crack" : "");

			// Stone Color
			SwapeProperty("m_StoneLight", style == Tiles.TileStyle.Stone ? "Light" : "");
			SwapeProperty("m_StoneNormal", style == Tiles.TileStyle.Stone ? "Normal" : "");
			SwapeProperty("m_StoneDark", style == Tiles.TileStyle.Stone ? "Dark" : "");

			// Floor Color
			SwapeProperty("m_FloorLight", style == Tiles.TileStyle.Floor ? "Light" : "");
			SwapeProperty("m_FloorNormal", style == Tiles.TileStyle.Floor ? "Normal" : "");
			SwapeProperty("m_FloorDark", style == Tiles.TileStyle.Floor ? "Dark" : "");

			// Window Color
			SwapeProperty("m_WindowLight", style == Tiles.TileStyle.Window ? "Light" : "");
			SwapeProperty("m_WindowNormal", style == Tiles.TileStyle.Window ? "Normal" : "");
			SwapeProperty("m_WindowDark", style == Tiles.TileStyle.Window ? "Dark" : "");
			SwapeProperty("m_GlassNormal", style == Tiles.TileStyle.Window ? "." : "");
			SwapeProperty("m_GlassShadow", style == Tiles.TileStyle.Window ? "." : "");

			// Marble Color
			SwapeProperty("m_MarbleLight", style == Tiles.TileStyle.Marble ? "Light" : "");
			SwapeProperty("m_MarbleNormal", style == Tiles.TileStyle.Marble ? "Normal" : "");
			SwapeProperty("m_MarbleCrack", style == Tiles.TileStyle.Marble ? "Crack" : "");
			SwapeProperty("m_MarbleDark", style == Tiles.TileStyle.Marble ? "Dark" : "");

			// Brick Color
			SwapeProperty("m_BrickLight", style == Tiles.TileStyle.Brick ? "Light" : "");
			SwapeProperty("m_BrickNormal", style == Tiles.TileStyle.Brick ? "Normal" : "");
			SwapeProperty("m_BrickDark", style == Tiles.TileStyle.Brick ? "Dark" : "");

			// Wallpaper Color
			SwapeProperty("m_WallpaperLight", style == Tiles.TileStyle.Wallpaper ? "Light" : "");
			SwapeProperty("m_WallpaperNormal", style == Tiles.TileStyle.Wallpaper ? "Normal" : "");
			SwapeProperty("m_WallpaperDark", style == Tiles.TileStyle.Wallpaper ? "Dark" : "");
			SwapeProperty("m_WallpaperPattern", style == Tiles.TileStyle.Wallpaper ? "Pattern" : "");

			// Metal Color
			SwapeProperty("m_MetalLight", style switch {
				Tiles.TileStyle.Metal => "Light",
				Tiles.TileStyle.Iron => "Light",
				_ => "",
			});
			SwapeProperty("m_MetalNormal", style switch {
				Tiles.TileStyle.Metal => "Normal",
				Tiles.TileStyle.Iron => "Normal",
				_ => "",
			});
			SwapeProperty("m_MetalDark", style switch {
				Tiles.TileStyle.Metal => "Dark",
				Tiles.TileStyle.Iron => "Dark",
				_ => "",
			});
			SwapeProperty("m_MetalRust", style switch {
				Tiles.TileStyle.Metal => "Rust",
				Tiles.TileStyle.Iron => "Rust",
				_ => "",
			});

			// Ice Color
			SwapeProperty("m_IceLight", style == Tiles.TileStyle.Ice ? "Light" : "");
			SwapeProperty("m_IceNormal", style == Tiles.TileStyle.Ice ? "Normal" : "");
			SwapeProperty("m_IceDark", style == Tiles.TileStyle.Ice ? "Dark" : "");
			SwapeProperty("m_IceSpecular", style == Tiles.TileStyle.Ice ? "Specular" : "");

			// Sand
			SwapeProperty("m_SandLight", style == Tiles.TileStyle.Sand ? "Light" : "");
			SwapeProperty("m_SandNormal", style == Tiles.TileStyle.Sand ? "Normal" : "");
			SwapeProperty("m_SandDark", style == Tiles.TileStyle.Sand ? "Dark" : "");
			SwapeProperty("m_SandTrace", style == Tiles.TileStyle.Sand ? "Trace" : "");


		}
	}
}
