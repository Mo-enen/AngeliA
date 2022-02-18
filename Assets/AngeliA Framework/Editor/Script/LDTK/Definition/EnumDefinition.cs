

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class EnumDefinition {
		public string externalFileChecksum;

		/// <summary>
		/// Relative path to the external file providing this Enum
		/// </summary>
		public string externalRelPath;

		/// <summary>
		/// Tileset UID if provided
		/// </summary>
		public long? iconTilesetUid;

		/// <summary>
		/// Unique String identifier
		/// </summary>
		public string identifier;

		/// <summary>
		/// Unique Int identifier
		/// </summary>
		public long uid;

		/// <summary>
		/// All possible enum values, with their optional Tile infos.
		/// </summary>
		public EnumValueDefinition[] values;
	}
}
