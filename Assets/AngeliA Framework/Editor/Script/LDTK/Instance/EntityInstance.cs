

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class EntityInstance {
		/// <summary>
		/// Grid-based coordinates (`[x,y]` format)
		/// </summary>
		public long[] grid;

		/// <summary>
		/// Entity definition identifier
		/// </summary>
		public string identifier;

		/// <summary>
		/// Pivot coordinates  (`[x,y]` format, values are from 0 to 1) of the Entity
		/// </summary>
		public double[] pivot;

		/// <summary>
		/// Optional Tile used to display this entity (it could either be the default Entity tile, or
		/// some tile provided by a field value, like an Enum).
		/// </summary>
		public EntityInstanceTile tile;

		/// <summary>
		/// Reference of the **Entity definition** UID
		/// </summary>
		public long defUid;

		/// <summary>
		/// An array of all custom fields and their values.
		/// </summary>
		public FieldInstance[] fieldInstances;

		/// <summary>
		/// Entity height in pixels. For non-resizable entities, it will be the same as Entity
		/// definition.
		/// </summary>
		public long height;

		/// <summary>
		/// Pixel coordinates (`[x,y]` format) in current level coordinate space. Don't forget
		/// optional layer offsets, if they exist!
		/// </summary>
		public long[] px;

		/// <summary>
		/// Entity width in pixels. For non-resizable entities, it will be the same as Entity
		/// definition.
		/// </summary>
		public long width;
	}
}
