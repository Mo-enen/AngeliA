using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace PixelJelly.Editor {
	[CustomJellyInspector(typeof(BrokenTiles))]
	public class BrokenTiles_Inspector : Tiles_Inspector {
		public override void OnPropertySwape (SerializedObject serializedObject) {

			base.OnPropertySwape(serializedObject);

			var target = serializedObject.targetObject as BrokenTiles;
			var style = target.Style;

			SwapeProperty("m_Hole", style switch {
				Tiles.TileStyle.Stone => ".",
				Tiles.TileStyle.Brick => ".",
				Tiles.TileStyle.Floor => ".",
				Tiles.TileStyle.Window => ".",
				_ => "",
			});


			SwapeProperty("m_StoneHoleWithBackground",
				style == Tiles.TileStyle.Stone && target.Hole.GreaterThanZero() ? "Background" : ""
			);



			SwapeProperty("m_BrickHoleWithBackground",
				style == Tiles.TileStyle.Brick && target.Hole.GreaterThanZero() ? "Background" : ""
			);
			SwapeProperty("m_FloorHoleWithBackground",
				style == Tiles.TileStyle.Floor && target.Hole.GreaterThanZero() ? "Background" : ""
			);
			SwapeProperty("m_BrokeFromEdge",
				target.Broken.GreaterThanZero() ? "Broke From Edge" : ""
			);

			SwapeProperty("m_StoneBrokenEdge",
				style == Tiles.TileStyle.Stone && target.Broken.GreaterThanZero() ? "Broken Edge" : ""
			);
			SwapeProperty("m_CrackedStoneBrokenEdge",
				style == Tiles.TileStyle.CrackedStone && target.Broken.GreaterThanZero() ? "Broken Edge" : ""
			);
			SwapeProperty("m_FloorBrokenEdge",
				style == Tiles.TileStyle.Floor && target.Broken.GreaterThanZero() ? "Broken Edge" : ""
			);
			SwapeProperty("m_WindowBrokenEdge",
				style == Tiles.TileStyle.Window && target.Broken.GreaterThanZero() ? "Broken Edge" : ""
			);
			SwapeProperty("m_BrickBrokenEdge",
				style == Tiles.TileStyle.Brick && target.Broken.GreaterThanZero() ? "Broken Edge" : ""
			);
			SwapeProperty("m_MarbleBrokenEdge",
				style == Tiles.TileStyle.Marble && target.Broken.GreaterThanZero() ? "Broken Edge" : ""
			);
			SwapeProperty("m_DirtBrokenEdge",
				style == Tiles.TileStyle.Dirt && target.Broken.GreaterThanZero() ? "Broken Edge" : ""
			);
			SwapeProperty("m_IceBrokenEdge",
				style == Tiles.TileStyle.Ice && target.Broken.GreaterThanZero() ? "Broken Edge" : ""
			);
			SwapeProperty("m_MetalBrokenEdge",
				(style == Tiles.TileStyle.Metal || style == Tiles.TileStyle.Iron) && target.Broken.GreaterThanZero() ? "Broken Edge" : ""
			);
			SwapeProperty("m_WallpaperBrokenEdge",
				style == Tiles.TileStyle.Wallpaper && target.Broken.GreaterThanZero() ? "Broken Edge" : ""
			);
			SwapeProperty("m_SandBrokenEdge",
				style == Tiles.TileStyle.Sand && target.Broken.GreaterThanZero() ? "Broken Edge" : ""
			);

		}

	}
}
