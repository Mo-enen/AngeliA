using System.Collections.Generic;

namespace LdtkToAngeliA {

	public partial class EntityDefinition {
		/// <summary>
		/// Base entity color
		/// </summary>
		public string Color { get; set; }

		/// <summary>
		/// Array of field definitions
		/// </summary>
		public FieldDefinition[] FieldDefs { get; set; }

		public double FillOpacity { get; set; }

		/// <summary>
		/// Pixel height
		/// </summary>
		public long Height { get; set; }

		public bool Hollow { get; set; }

		/// <summary>
		/// Unique String identifier
		/// </summary>
		public string Identifier { get; set; }

		/// <summary>
		/// Only applies to entities resizable on both X/Y. If TRUE, the entity instance width/height
		/// will keep the same aspect ratio as the definition.
		/// </summary>
		public bool KeepAspectRatio { get; set; }

		/// <summary>
		/// Possible values: `DiscardOldOnes`, `PreventAdding`, `MoveLastOne`
		/// </summary>
		public LimitBehavior LimitBehavior { get; set; }

		/// <summary>
		/// If TRUE, the maxCount is a "per world" limit, if FALSE, it's a "per level". Possible
		/// values: `PerLayer`, `PerLevel`, `PerWorld`
		/// </summary>
		public LimitScope LimitScope { get; set; }

		public double LineOpacity { get; set; }

		/// <summary>
		/// Max instances count
		/// </summary>
		public long MaxCount { get; set; }

		/// <summary>
		/// Pivot X coordinate (from 0 to 1.0)
		/// </summary>
		public double PivotX { get; set; }

		/// <summary>
		/// Pivot Y coordinate (from 0 to 1.0)
		/// </summary>
		public double PivotY { get; set; }

		/// <summary>
		/// Possible values: `Rectangle`, `Ellipse`, `Tile`, `Cross`
		/// </summary>
		public RenderMode RenderMode { get; set; }

		/// <summary>
		/// If TRUE, the entity instances will be resizable horizontally
		/// </summary>
		public bool ResizableX { get; set; }

		/// <summary>
		/// If TRUE, the entity instances will be resizable vertically
		/// </summary>
		public bool ResizableY { get; set; }

		/// <summary>
		/// Display entity name in editor
		/// </summary>
		public bool ShowName { get; set; }

		/// <summary>
		/// An array of strings that classifies this entity
		/// </summary>
		public string[] Tags { get; set; }

		/// <summary>
		/// Tile ID used for optional tile display
		/// </summary>
		public long? TileId { get; set; }

		/// <summary>
		/// Possible values: `Cover`, `FitInside`, `Repeat`, `Stretch`
		/// </summary>
		public TileRenderMode TileRenderMode { get; set; }

		/// <summary>
		/// Tileset ID used for optional tile display
		/// </summary>
		public long? TilesetId { get; set; }

		/// <summary>
		/// Unique Int identifier
		/// </summary>
		public long Uid { get; set; }

		/// <summary>
		/// Pixel width
		/// </summary>
		public long Width { get; set; }
	}

	/// <summary>
	/// This section is mostly only intended for the LDtk editor app itself. You can safely
	/// ignore it.
	/// </summary>
	public partial class FieldDefinition {
		/// <summary>
		/// Human readable value type (eg. `Int`, `Float`, `Point`, etc.). If the field is an array,
		/// this field will look like `Array<...>` (eg. `Array<Int>`, `Array<Point>` etc.)
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Optional list of accepted file extensions for FilePath value type. Includes the dot:
		/// `.ext`
		/// </summary>
		public string[] AcceptFileTypes { get; set; }

		/// <summary>
		/// Array max length
		/// </summary>
		public long? ArrayMaxLength { get; set; }

		/// <summary>
		/// Array min length
		/// </summary>
		public long? ArrayMinLength { get; set; }

		/// <summary>
		/// TRUE if the value can be null. For arrays, TRUE means it can contain null values
		/// (exception: array of Points can't have null values).
		/// </summary>
		public bool CanBeNull { get; set; }

		/// <summary>
		/// Default value if selected value is null or invalid.
		/// </summary>
		public object DefaultOverride { get; set; }

		public bool EditorAlwaysShow { get; set; }
		public bool EditorCutLongValues { get; set; }

		/// <summary>
		/// Possible values: `Hidden`, `ValueOnly`, `NameAndValue`, `EntityTile`, `Points`,
		/// `PointStar`, `PointPath`, `PointPathLoop`, `RadiusPx`, `RadiusGrid`
		/// </summary>
		public EditorDisplayMode EditorDisplayMode { get; set; }

		/// <summary>
		/// Possible values: `Above`, `Center`, `Beneath`
		/// </summary>
		public EditorDisplayPos EditorDisplayPos { get; set; }

		/// <summary>
		/// Unique String identifier
		/// </summary>
		public string Identifier { get; set; }

		/// <summary>
		/// TRUE if the value is an array of multiple values
		/// </summary>
		public bool IsArray { get; set; }

		/// <summary>
		/// Max limit for value, if applicable
		/// </summary>
		public double? Max { get; set; }

		/// <summary>
		/// Min limit for value, if applicable
		/// </summary>
		public double? Min { get; set; }

		/// <summary>
		/// Optional regular expression that needs to be matched to accept values. Expected format:
		/// `/some_reg_ex/g`, with optional "i" flag.
		/// </summary>
		public string Regex { get; set; }

		/// <summary>
		/// Possible values: &lt;`null`&gt;, `LangPython`, `LangRuby`, `LangJS`, `LangLua`, `LangC`,
		/// `LangHaxe`, `LangMarkdown`, `LangJson`, `LangXml`
		/// </summary>
		public TextLanguageMode? TextLanguageMode { get; set; }

		/// <summary>
		/// Internal type enum
		/// </summary>
		public object FieldDefinitionType { get; set; }

		/// <summary>
		/// Unique Int identifier
		/// </summary>
		public long Uid { get; set; }
	}

	public partial class EnumDefinition {
		public string ExternalFileChecksum { get; set; }

		/// <summary>
		/// Relative path to the external file providing this Enum
		/// </summary>
		public string ExternalRelPath { get; set; }

		/// <summary>
		/// Tileset UID if provided
		/// </summary>
		public long? IconTilesetUid { get; set; }

		/// <summary>
		/// Unique String identifier
		/// </summary>
		public string Identifier { get; set; }

		/// <summary>
		/// Unique Int identifier
		/// </summary>
		public long Uid { get; set; }

		/// <summary>
		/// All possible enum values, with their optional Tile infos.
		/// </summary>
		public EnumValueDefinition[] Values { get; set; }
	}

	public partial class EnumValueDefinition {
		/// <summary>
		/// An array of 4 Int values that refers to the tile in the tileset image: `[ x, y, width,
		/// height ]`
		/// </summary>
		public long[] TileSrcRect { get; set; }

		/// <summary>
		/// Optional color
		/// </summary>
		public long Color { get; set; }

		/// <summary>
		/// Enum value
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// The optional ID of the tile
		/// </summary>
		public long? TileId { get; set; }
	}

	public partial class LayerDefinition {
		/// <summary>
		/// Type of the layer (*IntGrid, Entities, Tiles or AutoLayer*)
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Contains all the auto-layer rule definitions.
		/// </summary>
		public AutoLayerRuleGroup[] AutoRuleGroups { get; set; }

		public long? AutoSourceLayerDefUid { get; set; }

		/// <summary>
		/// Reference to the Tileset UID being used by this auto-layer rules. WARNING: some layer
		/// *instances* might use a different tileset. So most of the time, you should probably use
		/// the `__tilesetDefUid` value from layer instances.
		/// </summary>
		public long? AutoTilesetDefUid { get; set; }

		/// <summary>
		/// Opacity of the layer (0 to 1.0)
		/// </summary>
		public double DisplayOpacity { get; set; }

		/// <summary>
		/// An array of tags to forbid some Entities in this layer
		/// </summary>
		public string[] ExcludedTags { get; set; }

		/// <summary>
		/// Width and height of the grid in pixels
		/// </summary>
		public long GridSize { get; set; }

		/// <summary>
		/// Unique String identifier
		/// </summary>
		public string Identifier { get; set; }

		/// <summary>
		/// An array that defines extra optional info for each IntGrid value. The array is sorted
		/// using value (ascending).
		/// </summary>
		public IntGridValueDefinition[] IntGridValues { get; set; }

		/// <summary>
		/// X offset of the layer, in pixels (IMPORTANT: this should be added to the `LayerInstance`
		/// optional offset)
		/// </summary>
		public long PxOffsetX { get; set; }

		/// <summary>
		/// Y offset of the layer, in pixels (IMPORTANT: this should be added to the `LayerInstance`
		/// optional offset)
		/// </summary>
		public long PxOffsetY { get; set; }

		/// <summary>
		/// An array of tags to filter Entities that can be added to this layer
		/// </summary>
		public string[] RequiredTags { get; set; }

		/// <summary>
		/// If the tiles are smaller or larger than the layer grid, the pivot value will be used to
		/// position the tile relatively its grid cell.
		/// </summary>
		public double TilePivotX { get; set; }

		/// <summary>
		/// If the tiles are smaller or larger than the layer grid, the pivot value will be used to
		/// position the tile relatively its grid cell.
		/// </summary>
		public double TilePivotY { get; set; }

		/// <summary>
		/// Reference to the Tileset UID being used by this Tile layer. WARNING: some layer
		/// *instances* might use a different tileset. So most of the time, you should probably use
		/// the `__tilesetDefUid` value from layer instances.
		/// </summary>
		public long? TilesetDefUid { get; set; }

		/// <summary>
		/// Type of the layer as Haxe Enum Possible values: `IntGrid`, `Entities`, `Tiles`,
		/// `AutoLayer`
		/// </summary>
		public TypeEnum LayerDefinitionType { get; set; }

		/// <summary>
		/// Unique Int identifier
		/// </summary>
		public long Uid { get; set; }
	}

	public partial class AutoLayerRuleGroup {
		public bool Active { get; set; }
		public bool Collapsed { get; set; }
		public bool IsOptional { get; set; }
		public string Name { get; set; }
		public AutoLayerRuleDefinition[] Rules { get; set; }
		public long Uid { get; set; }
	}

	/// <summary>
	/// This complex section isn't meant to be used by game devs at all, as these rules are
	/// completely resolved internally by the editor before any saving. You should just ignore
	/// this part.
	/// </summary>
	public partial class AutoLayerRuleDefinition {
		/// <summary>
		/// If FALSE, the rule effect isn't applied, and no tiles are generated.
		/// </summary>
		public bool Active { get; set; }

		/// <summary>
		/// When TRUE, the rule will prevent other rules to be applied in the same cell if it matches
		/// (TRUE by default).
		/// </summary>
		public bool BreakOnMatch { get; set; }

		/// <summary>
		/// Chances for this rule to be applied (0 to 1)
		/// </summary>
		public double Chance { get; set; }

		/// <summary>
		/// Checker mode Possible values: `None`, `Horizontal`, `Vertical`
		/// </summary>
		public Checker Checker { get; set; }

		/// <summary>
		/// If TRUE, allow rule to be matched by flipping its pattern horizontally
		/// </summary>
		public bool FlipX { get; set; }

		/// <summary>
		/// If TRUE, allow rule to be matched by flipping its pattern vertically
		/// </summary>
		public bool FlipY { get; set; }

		/// <summary>
		/// Default IntGrid value when checking cells outside of level bounds
		/// </summary>
		public long? OutOfBoundsValue { get; set; }

		/// <summary>
		/// Rule pattern (size x size)
		/// </summary>
		public long[] Pattern { get; set; }

		/// <summary>
		/// If TRUE, enable Perlin filtering to only apply rule on specific random area
		/// </summary>
		public bool PerlinActive { get; set; }

		public double PerlinOctaves { get; set; }
		public double PerlinScale { get; set; }
		public double PerlinSeed { get; set; }

		/// <summary>
		/// X pivot of a tile stamp (0-1)
		/// </summary>
		public double PivotX { get; set; }

		/// <summary>
		/// Y pivot of a tile stamp (0-1)
		/// </summary>
		public double PivotY { get; set; }

		/// <summary>
		/// Pattern width & height. Should only be 1,3,5 or 7.
		/// </summary>
		public long Size { get; set; }

		/// <summary>
		/// Array of all the tile IDs. They are used randomly or as stamps, based on `tileMode` value.
		/// </summary>
		public long[] TileIds { get; set; }

		/// <summary>
		/// Defines how tileIds array is used Possible values: `Single`, `Stamp`
		/// </summary>
		public TileMode TileMode { get; set; }

		/// <summary>
		/// Unique Int identifier
		/// </summary>
		public long Uid { get; set; }

		/// <summary>
		/// X cell coord modulo
		/// </summary>
		public long XModulo { get; set; }

		/// <summary>
		/// Y cell coord modulo
		/// </summary>
		public long YModulo { get; set; }
	}

	/// <summary>
	/// IntGrid value definition
	/// </summary>
	public partial class IntGridValueDefinition {
		public string Color { get; set; }

		/// <summary>
		/// Unique String identifier
		/// </summary>
		public string Identifier { get; set; }

		/// <summary>
		/// The IntGrid value itself
		/// </summary>
		public long Value { get; set; }
	}

	/// <summary>
	/// The `Tileset` definition is the most important part among project definitions. It
	/// contains some extra informations about each integrated tileset. If you only had to parse
	/// one definition section, that would be the one.
	/// </summary>
	public partial class TilesetDefinition {
		/// <summary>
		/// Grid-based height
		/// </summary>
		public long CHei { get; set; }

		/// <summary>
		/// Grid-based width
		/// </summary>
		public long CWid { get; set; }

		/// <summary>
		/// The following data is used internally for various optimizations. It's always synced with
		/// source image changes.
		/// </summary>
		public Dictionary<string, object> CachedPixelData { get; set; }

		/// <summary>
		/// An array of custom tile metadata
		/// </summary>
		public Dictionary<string, object>[] CustomData { get; set; }

		/// <summary>
		/// Tileset tags using Enum values specified by `tagsSourceEnumId`. This array contains 1
		/// element per Enum value, which contains an array of all Tile IDs that are tagged with it.
		/// </summary>
		public Dictionary<string, object>[] EnumTags { get; set; }

		/// <summary>
		/// Unique String identifier
		/// </summary>
		public string Identifier { get; set; }

		/// <summary>
		/// Distance in pixels from image borders
		/// </summary>
		public long Padding { get; set; }

		/// <summary>
		/// Image height in pixels
		/// </summary>
		public long PxHei { get; set; }

		/// <summary>
		/// Image width in pixels
		/// </summary>
		public long PxWid { get; set; }

		/// <summary>
		/// Path to the source file, relative to the current project JSON file
		/// </summary>
		public string RelPath { get; set; }

		/// <summary>
		/// Array of group of tiles selections, only meant to be used in the editor
		/// </summary>
		public Dictionary<string, object>[] SavedSelections { get; set; }

		/// <summary>
		/// Space in pixels between all tiles
		/// </summary>
		public long Spacing { get; set; }

		/// <summary>
		/// Optional Enum definition UID used for this tileset meta-data
		/// </summary>
		public long? TagsSourceEnumUid { get; set; }

		public long TileGridSize { get; set; }

		/// <summary>
		/// Unique Intidentifier
		/// </summary>
		public long Uid { get; set; }
	}

}