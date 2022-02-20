

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class Level {

		/// <summary>
		/// Unique String identifier
		/// </summary>
		public string identifier;

		/// <summary>
		/// Background color of the level (same as `bgColor`, except the default value is
		/// automatically used here if its value is `null`)
		/// </summary>
		public string __bgColor;

		/// <summary>
		/// Background color of the level. If `null`, the project `defaultLevelBgColor` should be
		/// used.
		/// </summary>
		public string levelBgColor;

		/// <summary>
		/// An array containing this level custom field values.
		/// </summary>
		public FieldInstance[] fieldInstances;

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
		/// World X coordinate in pixels
		/// </summary>
		public long worldX;

		/// <summary>
		/// World Y coordinate in pixels
		/// </summary>
		public long worldY;

	}
}
