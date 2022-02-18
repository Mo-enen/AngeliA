

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class Level {
		/// <summary>
		/// Background color of the level (same as `bgColor`, except the default value is
		/// automatically used here if its value is `null`)
		/// </summary>
		public string bgColor;

		/// <summary>
		/// Position informations of the background image, if there is one.
		/// </summary>
		public LevelBackgroundPosition bgPos;

		/// <summary>
		/// An array listing all other levels touching this one on the world map. In "linear" world
		/// layouts, this array is populated with previous/next levels in array, and `dir` depends on
		/// the linear horizontal/vertical layout.
		/// </summary>
		public NeighbourLevel[] neighbours;

		/// <summary>
		/// Background color of the level. If `null`, the project `defaultLevelBgColor` should be
		/// used.
		/// </summary>
		public string levelBgColor;

		/// <summary>
		/// Background image X pivot (0-1)
		/// </summary>
		public double bgPivotX;

		/// <summary>
		/// Background image Y pivot (0-1)
		/// </summary>
		public double bgPivotY;

		/// <summary>
		/// An enum defining the way the background image (if any) is positioned on the level. See
		/// `__bgPos` for resulting position info. Possible values: &lt;`null`&gt;, `Unscaled`,
		/// `Contain`, `Cover`, `CoverDirty`
		/// </summary>
		public BgPos? levelBgPos;

		/// <summary>
		/// The *optional* relative path to the level background image.
		/// </summary>
		public string bgRelPath;

		/// <summary>
		/// This value is not null if the project option "*Save levels separately*" is enabled. In
		/// this case, this **relative** path points to the level Json file.
		/// </summary>
		public string externalRelPath;

		/// <summary>
		/// An array containing this level custom field values.
		/// </summary>
		public FieldInstance[] fieldInstances;

		/// <summary>
		/// Unique String identifier
		/// </summary>
		public string identifier;

		/// <summary>
		/// An array containing all Layer instances. **IMPORTANT**: if the project option "*Save
		/// levels separately*" is enabled, this field will be `null`.<br/>  This array is **sorted
		/// in display order**: the 1st layer is the top-most and the last is behind.
		/// </summary>
		public LayerInstance[] layerInstances;

		/// <summary>
		/// Height of the level in pixels
		/// </summary>
		public long pxHei;

		/// <summary>
		/// Width of the level in pixels
		/// </summary>
		public long pxWid;

		/// <summary>
		/// Unique Int identifier
		/// </summary>
		public long uid;

		/// <summary>
		/// If TRUE, the level identifier will always automatically use the naming pattern as defined
		/// in `Project.levelNamePattern`. Becomes FALSE if the identifier is manually modified by
		/// user.
		/// </summary>
		public bool useAutoIdentifier;

		/// <summary>
		/// World X coordinate in pixels
		/// </summary>
		public long worldX;

		/// <summary>
		/// World Y coordinate in pixels
		/// </summary>
		public long worldY;
	}
}
