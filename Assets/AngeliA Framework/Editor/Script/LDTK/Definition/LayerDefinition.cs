

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class LayerDefinition {
		/// <summary>
		/// Type of the layer (*IntGrid, Entities, Tiles or AutoLayer*)
		/// </summary>
		public string type;

		/// <summary>
		/// Contains all the auto-layer rule definitions.
		/// </summary>
		public AutoLayerRuleGroup[] autoRuleGroups;

		public long? autoSourceLayerDefUid;

		/// <summary>
		/// Reference to the Tileset UID being used by this auto-layer rules. WARNING: some layer
		/// *instances* might use a different tileset. So most of the time, you should probably use
		/// the `__tilesetDefUid` value from layer instances.
		/// </summary>
		public long? autoTilesetDefUid;

		/// <summary>
		/// Opacity of the layer (0 to 1.0)
		/// </summary>
		public double displayOpacity;

		/// <summary>
		/// An array of tags to forbid some Entities in this layer
		/// </summary>
		public string[] excludedTags;

		/// <summary>
		/// Width and height of the grid in pixels
		/// </summary>
		public long gridSize;

		/// <summary>
		/// Unique String identifier
		/// </summary>
		public string identifier;

		/// <summary>
		/// An array that defines extra optional info for each IntGrid value. The array is sorted
		/// using value (ascending).
		/// </summary>
		public IntGridValueDefinition[] intGridValues;

		/// <summary>
		/// X offset of the layer, in pixels (IMPORTANT: this should be added to the `LayerInstance`
		/// optional offset)
		/// </summary>
		public long pxOffsetX;

		/// <summary>
		/// Y offset of the layer, in pixels (IMPORTANT: this should be added to the `LayerInstance`
		/// optional offset)
		/// </summary>
		public long pxOffsetY;

		/// <summary>
		/// An array of tags to filter Entities that can be added to this layer
		/// </summary>
		public string[] requiredTags;

		/// <summary>
		/// If the tiles are smaller or larger than the layer grid, the pivot value will be used to
		/// position the tile relatively its grid cell.
		/// </summary>
		public double tilePivotX;

		/// <summary>
		/// If the tiles are smaller or larger than the layer grid, the pivot value will be used to
		/// position the tile relatively its grid cell.
		/// </summary>
		public double tilePivotY;

		/// <summary>
		/// Reference to the Tileset UID being used by this Tile layer. WARNING: some layer
		/// *instances* might use a different tileset. So most of the time, you should probably use
		/// the `__tilesetDefUid` value from layer instances.
		/// </summary>
		public long? tilesetDefUid;

		/// <summary>
		/// Type of the layer as Haxe Enum Possible values: `IntGrid`, `Entities`, `Tiles`,
		/// `AutoLayer`
		/// </summary>
		public TypeEnum layerDefinitionType;

		/// <summary>
		/// Unique Int identifier
		/// </summary>
		public long uid;
	}
}
