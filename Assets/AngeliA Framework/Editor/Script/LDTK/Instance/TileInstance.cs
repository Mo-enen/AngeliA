

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class TileInstance {
		/// <summary>
		/// Internal data used by the editor.<br/>  For auto-layer tiles: `[ruleId, coordId]`.<br/>
		/// For tile-layer tiles: `[coordId]`.
		/// </summary>
		public long[] d;

		/// <summary>
		/// "Flip bits", a 2-bits integer to represent the mirror transformations of the tile.<br/>
		/// - Bit 0 = X flip<br/>   - Bit 1 = Y flip<br/>   Examples: f=0 (no flip), f=1 (X flip
		/// only), f=2 (Y flip only), f=3 (both flips)
		/// </summary>
		public long f;

		/// <summary>
		/// Pixel coordinates of the tile in the **layer** (`[x,y]` format). Don't forget optional
		/// layer offsets, if they exist!
		/// </summary>
		public long[] px;

		/// <summary>
		/// Pixel coordinates of the tile in the **tileset** (`[x,y]` format)
		/// </summary>
		public long[] src;

		/// <summary>
		/// The *Tile ID* in the corresponding tileset.
		/// </summary>
		public long t;
	}
}
