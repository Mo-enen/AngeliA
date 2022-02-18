using System.Collections.Generic;


namespace LdtkToAngeliA {


	[System.Serializable]
	public class CustomDataItem {
		public int tileId;
		public string data;
	}


	[System.Serializable]
	public partial class TilesetDefinition {
		/// <summary>
		/// Grid-based height
		/// </summary>
		public long cHei;

		/// <summary>
		/// Grid-based width
		/// </summary>
		public long cWid;

		/// <summary>
		/// An array of custom tile metadata
		/// </summary>
		public CustomDataItem[] customData;

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
		public long padding;

		/// <summary>
		/// Image height in pixels
		/// </summary>
		public long pxHei;

		/// <summary>
		/// Image width in pixels
		/// </summary>
		public long pxWid;

		/// <summary>
		/// Path to the source file, relative to the current project JSON file
		/// </summary>
		public string relPath;

		/// <summary>
		/// Space in pixels between all tiles
		/// </summary>
		public long spacing;

		/// <summary>
		/// Optional Enum definition UID used for this tileset meta-data
		/// </summary>
		public long? tagsSourceEnumUid;

		public long tileGridSize;

		/// <summary>
		/// Unique Intidentifier
		/// </summary>
		public long uid;
	}
}
