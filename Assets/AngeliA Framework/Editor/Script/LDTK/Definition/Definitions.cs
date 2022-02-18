

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class Definitions {
		/// <summary>
		/// All entities, including their custom fields
		/// </summary>
		public EntityDefinition[] entities;

		public EnumDefinition[] enums;

		/// <summary>
		/// Note: external enums are exactly the same as `enums`, except they have a `relPath` to
		/// point to an external source file.
		/// </summary>
		public EnumDefinition[] externalEnums;

		public LayerDefinition[] layers;

		/// <summary>
		/// An array containing all custom fields available to all levels.
		/// </summary>
		public FieldDefinition[] levelFields;

		public TilesetDefinition[] tilesets;
	}
}
