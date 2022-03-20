using Newtonsoft.Json;

namespace LdtkToAngeliA {
	public partial class EntityDefinition {
		/// <summary>
		/// Base entity color
		/// </summary>
		[JsonProperty("color")]
		public string Color { get; set; }

		/// <summary>
		/// Array of field definitions
		/// </summary>
		[JsonProperty("fieldDefs")]
		public FieldDefinition[] FieldDefs { get; set; }

		[JsonProperty("tileOpacity")]
		public float TileOpacity { get; set; }

		[JsonProperty("fillOpacity")]
		public float FillOpacity { get; set; }

		/// <summary>
		/// Pixel height
		/// </summary>
		[JsonProperty("height")]
		public int Height { get; set; }

		[JsonProperty("hollow")]
		public bool Hollow { get; set; }

		/// <summary>
		/// Unique String identifier
		/// </summary>
		[JsonProperty("identifier")]
		public string Identifier { get; set; }

		/// <summary>
		/// Only applies to entities resizable on both X/Y. If TRUE, the entity instance width/height
		/// will keep the same aspect ratio as the definition.
		/// </summary>
		[JsonProperty("keepAspectRatio")]
		public bool KeepAspectRatio { get; set; }

		/// <summary>
		/// Possible values: `DiscardOldOnes`, `PreventAdding`, `MoveLastOne`
		/// </summary>
		[JsonProperty("limitBehavior")]
		public string LimitBehavior { get; set; }

		/// <summary>
		/// If TRUE, the maxCount is a "per world" limit, if FALSE, it's a "per level". Possible
		/// values: `PerLayer`, `PerLevel`, `PerWorld`
		/// </summary>
		[JsonProperty("limitScope")]
		public string LimitScope { get; set; }

		[JsonProperty("lineOpacity")]
		public float LineOpacity { get; set; }

		/// <summary>
		/// Max instances count
		/// </summary>
		[JsonProperty("maxCount")]
		public int MaxCount { get; set; }

		/// <summary>
		/// Pivot X coordinate (from 0 to 1.0)
		/// </summary>
		[JsonProperty("pivotX")]
		public float PivotX { get; set; }

		/// <summary>
		/// Pivot Y coordinate (from 0 to 1.0)
		/// </summary>
		[JsonProperty("pivotY")]
		public float PivotY { get; set; }

		/// <summary>
		/// Possible values: `Rectangle`, `Ellipse`, `Tile`, `Cross`
		/// </summary>
		[JsonProperty("renderMode")]
		public string RenderMode { get; set; }

		/// <summary>
		/// If TRUE, the entity instances will be resizable horizontally
		/// </summary>
		[JsonProperty("resizableX")]
		public bool ResizableX { get; set; }

		/// <summary>
		/// If TRUE, the entity instances will be resizable vertically
		/// </summary>
		[JsonProperty("resizableY")]
		public bool ResizableY { get; set; }

		/// <summary>
		/// Display entity name in editor
		/// </summary>
		[JsonProperty("showName")]
		public bool ShowName { get; set; }

		/// <summary>
		/// An array of strings that classifies this entity
		/// </summary>
		[JsonProperty("tags")]
		public string[] Tags { get; set; }

		/// <summary>
		/// Tile ID used for optional tile display
		/// </summary>
		[JsonProperty("tileId")]
		public int? TileId { get; set; }

		/// <summary>
		/// Possible values: `Cover`, `FitInside`, `Repeat`, `Stretch`
		/// </summary>
		[JsonProperty("tileRenderMode")]
		public string TileRenderMode { get; set; }

		/// <summary>
		/// Tileset ID used for optional tile display
		/// </summary>
		[JsonProperty("tilesetId")]
		public int? TilesetId { get; set; }

		/// <summary>
		/// Unique Int identifier
		/// </summary>
		[JsonProperty("uid")]
		public int Uid { get; set; }

		/// <summary>
		/// Pixel width
		/// </summary>
		[JsonProperty("width")]
		public int Width { get; set; }


		[JsonProperty("tileRect")]
		public TileRect TileRect { get; set; }


	}


	public class TileRect {
		[JsonProperty("tilesetUid")]
		public int id;
		[JsonProperty("x")]
		public int x;
		[JsonProperty("y")]
		public int y;
		[JsonProperty("w")]
		public int w;
		[JsonProperty("h")]
		public int h;
	}


}