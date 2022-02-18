

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class EnumValueDefinition {
		/// <summary>
		/// An array of 4 Int values that refers to the tile in the tileset image: `[ x, y, width,
		/// height ]`
		/// </summary>
		public long[] tileSrcRect;

		/// <summary>
		/// Optional color
		/// </summary>
		public long color;

		/// <summary>
		/// Enum value
		/// </summary>
		public string id;

		/// <summary>
		/// The optional ID of the tile
		/// </summary>
		public long? tileId;
	}
}
