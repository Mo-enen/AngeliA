

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class EntityDefinition {
		/// <summary>
		/// Base entity color
		/// </summary>
		public string color;

		/// <summary>
		/// Array of field definitions
		/// </summary>
		public FieldDefinition[] fieldDefs;

		public float fillOpacity;

		/// <summary>
		/// Pixel height
		/// </summary>
		public int height;

		public bool hollow;

		/// <summary>
		/// Unique String identifier
		/// </summary>
		public string identifier;

		/// <summary>
		/// Only applies to entities resizable on both X/Y. If TRUE, the entity instance width/height
		/// will keep the same aspect ratio as the definition.
		/// </summary>
		public bool keepAspectRatio;

		/// <summary>
		/// Possible values: `DiscardOldOnes`, `PreventAdding`, `MoveLastOne`
		/// </summary>
		public LimitBehavior limitBehavior;

		/// <summary>
		/// If TRUE, the maxCount is a "per world" limit, if FALSE, it's a "per level". Possible
		/// values: `PerLayer`, `PerLevel`, `PerWorld`
		/// </summary>
		public LimitScope limitScope;

		public float lineOpacity;

		/// <summary>
		/// Max instances count
		/// </summary>
		public int maxCount;

		/// <summary>
		/// Pivot X coordinate (from 0 to 1.0)
		/// </summary>
		public float pivotX;

		/// <summary>
		/// Pivot Y coordinate (from 0 to 1.0)
		/// </summary>
		public float pivotY;

		/// <summary>
		/// Possible values: `Rectangle`, `Ellipse`, `Tile`, `Cross`
		/// </summary>
		public RenderMode renderMode;

		/// <summary>
		/// If TRUE, the entity instances will be resizable horizontally
		/// </summary>
		public bool resizableX;

		/// <summary>
		/// If TRUE, the entity instances will be resizable vertically
		/// </summary>
		public bool resizableY;

		/// <summary>
		/// Display entity name in editor
		/// </summary>
		public bool showName;

		/// <summary>
		/// An array of strings that classifies this entity
		/// </summary>
		public string[] tags;

		/// <summary>
		/// Tile ID used for optional tile display
		/// </summary>
		public int? tileId;

		/// <summary>
		/// Possible values: `Cover`, `FitInside`, `Repeat`, `Stretch`
		/// </summary>
		public TileRenderMode tileRenderMode;

		/// <summary>
		/// Tileset ID used for optional tile display
		/// </summary>
		public int? tilesetId;

		/// <summary>
		/// Unique Int identifier
		/// </summary>
		public int uid;

		/// <summary>
		/// Pixel width
		/// </summary>
		public int width;
	}
}
