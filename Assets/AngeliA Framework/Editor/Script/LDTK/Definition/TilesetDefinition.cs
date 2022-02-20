using System.Collections.Generic;


namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class TilesetDefinition {

		public int tileGridSize;

		/// <summary>
		/// Grid-based height
		/// </summary>
		public int cHei;

		/// <summary>
		/// Grid-based width
		/// </summary>
		public int cWid;

		/// <summary>
		/// Tileset tags using Enum values specified by `tagsSourceEnumId`. This array contains 1
		/// element per Enum value, which contains an array of all Tile IDs that are tagged with it.
		/// </summary>
		public Dictionary<string, object>[] enumTags;

		/// <summary>
		/// Unique String identifier
		/// </summary>
		public string identifier;

		/// <summary>
		/// Distance in pixels from image borders
		/// </summary>
		public int padding;

		/// <summary>
		/// Image height in pixels
		/// </summary>
		public int pxHei;

		/// <summary>
		/// Image width in pixels
		/// </summary>
		public int pxWid;

		/// <summary>
		/// Path to the source file, relative to the current project JSON file
		/// </summary>
		public string relPath;

		/// <summary>
		/// Space in pixels between all tiles
		/// </summary>
		public int spacing;


	}
}
