

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class EntityInstanceTile {
		/// <summary>
		/// An array of 4 Int values that refers to the tile in the tileset image: `[ x, y, width,
		/// height ]`
		/// </summary>
		public long[] srcRect;

		/// <summary>
		/// Tileset ID
		/// </summary>
		public long tilesetUid;
	}
}
