

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class AutoLayerRuleDefinition {
		/// <summary>
		/// If FALSE, the rule effect isn't applied, and no tiles are generated.
		/// </summary>
		public bool active;

		/// <summary>
		/// When TRUE, the rule will prevent other rules to be applied in the same cell if it matches
		/// (TRUE by default).
		/// </summary>
		public bool breakOnMatch;

		/// <summary>
		/// Chances for this rule to be applied (0 to 1)
		/// </summary>
		public double chance;

		/// <summary>
		/// Checker mode Possible values: `None`, `Horizontal`, `Vertical`
		/// </summary>
		public Checker checker;

		/// <summary>
		/// If TRUE, allow rule to be matched by flipping its pattern horizontally
		/// </summary>
		public bool flipX;

		/// <summary>
		/// If TRUE, allow rule to be matched by flipping its pattern vertically
		/// </summary>
		public bool flipY;

		/// <summary>
		/// Default IntGrid value when checking cells outside of level bounds
		/// </summary>
		public long? outOfBoundsValue;

		/// <summary>
		/// Rule pattern (size x size)
		/// </summary>
		public long[] pattern;

		/// <summary>
		/// If TRUE, enable Perlin filtering to only apply rule on specific random area
		/// </summary>
		public bool perlinActive;

		public double perlinOctaves;

		public double perlinScale;

		public double perlinSeed;

		/// <summary>
		/// X pivot of a tile stamp (0-1)
		/// </summary>
		public double pivotX;

		/// <summary>
		/// Y pivot of a tile stamp (0-1)
		/// </summary>
		public double pivotY;

		/// <summary>
		/// Pattern width & height. Should only be 1,3,5 or 7.
		/// </summary>
		public long size;

		/// <summary>
		/// Array of all the tile IDs. They are used randomly or as stamps, based on `tileMode` value.
		/// </summary>
		public long[] tileIds;

		/// <summary>
		/// Defines how tileIds array is used Possible values: `Single`, `Stamp`
		/// </summary>
		public TileMode tileMode;

		/// <summary>
		/// Unique Int identifier
		/// </summary>
		public long uid;

		/// <summary>
		/// X cell coord modulo
		/// </summary>
		public long xModulo;

		/// <summary>
		/// Y cell coord modulo
		/// </summary>
		public long yModulo;
	}
}
