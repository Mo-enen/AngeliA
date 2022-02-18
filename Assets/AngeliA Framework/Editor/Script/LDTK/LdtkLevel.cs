using System.Collections.Generic;

namespace LdtkToAngeliA {

	/// <summary>
	/// This file is a JSON schema of files created by LDtk level editor (https://ldtk.io).
	///
	/// This is the root of any Project JSON file. It contains:  - the project settings, - an
	/// array of levels, - a group of definitions (that can probably be safely ignored for most
	/// users).
	/// </summary>
	public class LdtkLevel {
		/// <summary>
		/// Number of backup files to keep, if the `backupOnSave` is TRUE
		/// </summary>
		public long BackupLimit { get; set; }

		/// <summary>
		/// If TRUE, an extra copy of the project will be created in a sub folder, when saving.
		/// </summary>
		public bool BackupOnSave { get; set; }

		/// <summary>
		/// Project background color
		/// </summary>
		public string BgColor { get; set; }

		/// <summary>
		/// Default grid size for new layers
		/// </summary>
		public long DefaultGridSize { get; set; }

		/// <summary>
		/// Default background color of levels
		/// </summary>
		public string DefaultLevelBgColor { get; set; }

		/// <summary>
		/// Default new level height
		/// </summary>
		public long DefaultLevelHeight { get; set; }

		/// <summary>
		/// Default new level width
		/// </summary>
		public long DefaultLevelWidth { get; set; }

		/// <summary>
		/// Default X pivot (0 to 1) for new entities
		/// </summary>
		public double DefaultPivotX { get; set; }

		/// <summary>
		/// Default Y pivot (0 to 1) for new entities
		/// </summary>
		public double DefaultPivotY { get; set; }

		/// <summary>
		/// A structure containing all the definitions of this project
		/// </summary>
		public Definitions Defs { get; set; }

		/// <summary>
		/// **WARNING**: this deprecated value is no longer exported since version 0.9.3  Replaced
		/// by: `imageExportMode`
		/// </summary>
		public bool? ExportPng { get; set; }

		/// <summary>
		/// If TRUE, a Tiled compatible file will also be generated along with the LDtk JSON file
		/// (default is FALSE)
		/// </summary>
		public bool ExportTiled { get; set; }

		/// <summary>
		/// If TRUE, one file will be saved for the project (incl. all its definitions) and one file
		/// in a sub-folder for each level.
		/// </summary>
		public bool ExternalLevels { get; set; }

		/// <summary>
		/// An array containing various advanced flags (ie. options or other states). Possible
		/// values: `DiscardPreCsvIntGrid`, `IgnoreBackupSuggest`
		/// </summary>
		public Flag[] Flags { get; set; }

		/// <summary>
		/// "Image export" option when saving project. Possible values: `None`, `OneImagePerLayer`,
		/// `OneImagePerLevel`
		/// </summary>
		public ImageExportMode ImageExportMode { get; set; }

		/// <summary>
		/// File format version
		/// </summary>
		public string JsonVersion { get; set; }

		/// <summary>
		/// The default naming convention for level identifiers.
		/// </summary>
		public string LevelNamePattern { get; set; }

		/// <summary>
		/// All levels. The order of this array is only relevant in `LinearHorizontal` and
		/// `linearVertical` world layouts (see `worldLayout` value). Otherwise, you should refer to
		/// the `worldX`,`worldY` coordinates of each Level.
		/// </summary>
		public Level[] Levels { get; set; }

		/// <summary>
		/// If TRUE, the Json is partially minified (no indentation, nor line breaks, default is
		/// FALSE)
		/// </summary>
		public bool MinifyJson { get; set; }

		/// <summary>
		/// Next Unique integer ID available
		/// </summary>
		public long NextUid { get; set; }

		/// <summary>
		/// File naming pattern for exported PNGs
		/// </summary>
		public string PngFilePattern { get; set; }

		/// <summary>
		/// Height of the world grid in pixels.
		/// </summary>
		public long WorldGridHeight { get; set; }

		/// <summary>
		/// Width of the world grid in pixels.
		/// </summary>
		public long WorldGridWidth { get; set; }

		/// <summary>
		/// An enum that describes how levels are organized in this project (ie. linearly or in a 2D
		/// space). Possible values: `Free`, `GridVania`, `LinearHorizontal`, `LinearVertical`
		/// </summary>
		public WorldLayout WorldLayout { get; set; }
	}

	/// <summary>
	/// A structure containing all the definitions of this project
	///
	/// If you're writing your own LDtk importer, you should probably just ignore *most* stuff in
	/// the `defs` section, as it contains data that are mostly important to the editor. To keep
	/// you away from the `defs` section and avoid some unnecessary JSON parsing, important data
	/// from definitions is often duplicated in fields prefixed with a double underscore (eg.
	/// `__identifier` or `__type`).  The 2 only definition types you might need here are
	/// **Tilesets** and **Enums**.
	/// </summary>
	public class Definitions {
		/// <summary>
		/// All entities definitions, including their custom fields
		/// </summary>
		public EntityDefinition[] Entities { get; set; }

		/// <summary>
		/// All internal enums
		/// </summary>
		public EnumDefinition[] Enums { get; set; }

		/// <summary>
		/// Note: external enums are exactly the same as `enums`, except they have a `relPath` to
		/// point to an external source file.
		/// </summary>
		public EnumDefinition[] ExternalEnums { get; set; }

		/// <summary>
		/// All layer definitions
		/// </summary>
		public LayerDefinition[] Layers { get; set; }

		/// <summary>
		/// All custom fields available to all levels.
		/// </summary>
		public FieldDefinition[] LevelFields { get; set; }

		/// <summary>
		/// All tilesets
		/// </summary>
		public TilesetDefinition[] Tilesets { get; set; }
	}

	public class EntityDefinition {
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
	public class FieldDefinition {
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

	public class EnumDefinition {
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

	public class EnumValueDefinition {
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

	public class LayerDefinition {
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

	public class AutoLayerRuleGroup {
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
	public class AutoLayerRuleDefinition {
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
	public class IntGridValueDefinition {
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
	public class TilesetDefinition {
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

	/// <summary>
	/// This section contains all the level data. It can be found in 2 distinct forms, depending
	/// on Project current settings:  - If "*Separate level files*" is **disabled** (default):
	/// full level data is *embedded* inside the main Project JSON file, - If "*Separate level
	/// files*" is **enabled**: level data is stored in *separate* standalone `.ldtkl` files (one
	/// per level). In this case, the main Project JSON file will still contain most level data,
	/// except heavy sections, like the `layerInstances` array (which will be null). The
	/// `externalRelPath` string points to the `ldtkl` file.  A `ldtkl` file is just a JSON file
	/// containing exactly what is described below.
	/// </summary>
	public class Level {
		/// <summary>
		/// Background color of the level (same as `bgColor`, except the default value is
		/// automatically used here if its value is `null`)
		/// </summary>
		public string BgColor { get; set; }

		/// <summary>
		/// Position informations of the background image, if there is one.
		/// </summary>
		public LevelBackgroundPosition BgPos { get; set; }

		/// <summary>
		/// An array listing all other levels touching this one on the world map. In "linear" world
		/// layouts, this array is populated with previous/next levels in array, and `dir` depends on
		/// the linear horizontal/vertical layout.
		/// </summary>
		public NeighbourLevel[] Neighbours { get; set; }

		/// <summary>
		/// Background color of the level. If `null`, the project `defaultLevelBgColor` should be
		/// used.
		/// </summary>
		public string LevelBgColor { get; set; }

		/// <summary>
		/// Background image X pivot (0-1)
		/// </summary>
		public double BgPivotX { get; set; }

		/// <summary>
		/// Background image Y pivot (0-1)
		/// </summary>
		public double BgPivotY { get; set; }

		/// <summary>
		/// An enum defining the way the background image (if any) is positioned on the level. See
		/// `__bgPos` for resulting position info. Possible values: &lt;`null`&gt;, `Unscaled`,
		/// `Contain`, `Cover`, `CoverDirty`
		/// </summary>
		public BgPos? LevelBgPos { get; set; }

		/// <summary>
		/// The *optional* relative path to the level background image.
		/// </summary>
		public string BgRelPath { get; set; }

		/// <summary>
		/// This value is not null if the project option "*Save levels separately*" is enabled. In
		/// this case, this **relative** path points to the level Json file.
		/// </summary>
		public string ExternalRelPath { get; set; }

		/// <summary>
		/// An array containing this level custom field values.
		/// </summary>
		public FieldInstance[] FieldInstances { get; set; }

		/// <summary>
		/// Unique String identifier
		/// </summary>
		public string Identifier { get; set; }

		/// <summary>
		/// An array containing all Layer instances. **IMPORTANT**: if the project option "*Save
		/// levels separately*" is enabled, this field will be `null`.<br/>  This array is **sorted
		/// in display order**: the 1st layer is the top-most and the last is behind.
		/// </summary>
		public LayerInstance[] LayerInstances { get; set; }

		/// <summary>
		/// Height of the level in pixels
		/// </summary>
		public long PxHei { get; set; }

		/// <summary>
		/// Width of the level in pixels
		/// </summary>
		public long PxWid { get; set; }

		/// <summary>
		/// Unique Int identifier
		/// </summary>
		public long Uid { get; set; }

		/// <summary>
		/// If TRUE, the level identifier will always automatically use the naming pattern as defined
		/// in `Project.levelNamePattern`. Becomes FALSE if the identifier is manually modified by
		/// user.
		/// </summary>
		public bool UseAutoIdentifier { get; set; }

		/// <summary>
		/// World X coordinate in pixels
		/// </summary>
		public long WorldX { get; set; }

		/// <summary>
		/// World Y coordinate in pixels
		/// </summary>
		public long WorldY { get; set; }
	}

	/// <summary>
	/// Level background image position info
	/// </summary>
	public class LevelBackgroundPosition {
		/// <summary>
		/// An array of 4 float values describing the cropped sub-rectangle of the displayed
		/// background image. This cropping happens when original is larger than the level bounds.
		/// Array format: `[ cropX, cropY, cropWidth, cropHeight ]`
		/// </summary>
		public double[] CropRect { get; set; }

		/// <summary>
		/// An array containing the `[scaleX,scaleY]` values of the **cropped** background image,
		/// depending on `bgPos` option.
		/// </summary>
		public double[] Scale { get; set; }

		/// <summary>
		/// An array containing the `[x,y]` pixel coordinates of the top-left corner of the
		/// **cropped** background image, depending on `bgPos` option.
		/// </summary>
		public long[] TopLeftPx { get; set; }
	}

	public class FieldInstance {
		/// <summary>
		/// Field definition identifier
		/// </summary>
		public string Identifier { get; set; }

		/// <summary>
		/// Type of the field, such as `Int`, `Float`, `Enum(my_enum_name)`, `Bool`, etc.
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Actual value of the field instance. The value type may vary, depending on `__type`
		/// (Integer, Boolean, String etc.)<br/>  It can also be an `Array` of those same types.
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		/// Reference of the **Field definition** UID
		/// </summary>
		public long DefUid { get; set; }

		/// <summary>
		/// Editor internal raw values
		/// </summary>
		public object[] RealEditorValues { get; set; }
	}

	public class LayerInstance {
		/// <summary>
		/// Grid-based height
		/// </summary>
		public long CHei { get; set; }

		/// <summary>
		/// Grid-based width
		/// </summary>
		public long CWid { get; set; }

		/// <summary>
		/// Grid size
		/// </summary>
		public long GridSize { get; set; }

		/// <summary>
		/// Layer definition identifier
		/// </summary>
		public string Identifier { get; set; }

		/// <summary>
		/// Layer opacity as Float [0-1]
		/// </summary>
		public double Opacity { get; set; }

		/// <summary>
		/// Total layer X pixel offset, including both instance and definition offsets.
		/// </summary>
		public long PxTotalOffsetX { get; set; }

		/// <summary>
		/// Total layer Y pixel offset, including both instance and definition offsets.
		/// </summary>
		public long PxTotalOffsetY { get; set; }

		/// <summary>
		/// The definition UID of corresponding Tileset, if any.
		/// </summary>
		public long? TilesetDefUid { get; set; }

		/// <summary>
		/// The relative path to corresponding Tileset, if any.
		/// </summary>
		public string TilesetRelPath { get; set; }

		/// <summary>
		/// Layer type (possible values: IntGrid, Entities, Tiles or AutoLayer)
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// An array containing all tiles generated by Auto-layer rules. The array is already sorted
		/// in display order (ie. 1st tile is beneath 2nd, which is beneath 3rd etc.).<br/><br/>
		/// Note: if multiple tiles are stacked in the same cell as the result of different rules,
		/// all tiles behind opaque ones will be discarded.
		/// </summary>
		public TileInstance[] AutoLayerTiles { get; set; }

		public EntityInstance[] EntityInstances { get; set; }
		public TileInstance[] GridTiles { get; set; }

		/// <summary>
		/// **WARNING**: this deprecated value will be *removed* completely on version 0.10.0+
		/// Replaced by: `intGridCsv`
		/// </summary>
		public IntGridValueInstance[] IntGrid { get; set; }

		/// <summary>
		/// A list of all values in the IntGrid layer, stored from left to right, and top to bottom
		/// (ie. first row from left to right, followed by second row, etc). `0` means "empty cell"
		/// and IntGrid values start at 1. This array size is `__cWid` x `__cHei` cells.
		/// </summary>
		public long[] IntGridCsv { get; set; }

		/// <summary>
		/// Reference the Layer definition UID
		/// </summary>
		public long LayerDefUid { get; set; }

		/// <summary>
		/// Reference to the UID of the level containing this layer instance
		/// </summary>
		public long LevelId { get; set; }

		/// <summary>
		/// An Array containing the UIDs of optional rules that were enabled in this specific layer
		/// instance.
		/// </summary>
		public long[] OptionalRules { get; set; }

		/// <summary>
		/// This layer can use another tileset by overriding the tileset UID here.
		/// </summary>
		public long? OverrideTilesetUid { get; set; }

		/// <summary>
		/// X offset in pixels to render this layer, usually 0 (IMPORTANT: this should be added to
		/// the `LayerDef` optional offset, see `__pxTotalOffsetX`)
		/// </summary>
		public long PxOffsetX { get; set; }

		/// <summary>
		/// Y offset in pixels to render this layer, usually 0 (IMPORTANT: this should be added to
		/// the `LayerDef` optional offset, see `__pxTotalOffsetY`)
		/// </summary>
		public long PxOffsetY { get; set; }

		/// <summary>
		/// Random seed used for Auto-Layers rendering
		/// </summary>
		public long Seed { get; set; }

		/// <summary>
		/// Layer instance visibility
		/// </summary>
		public bool Visible { get; set; }
	}

	/// <summary>
	/// This structure represents a single tile from a given Tileset.
	/// </summary>
	public class TileInstance {
		/// <summary>
		/// Internal data used by the editor.<br/>  For auto-layer tiles: `[ruleId, coordId]`.<br/>
		/// For tile-layer tiles: `[coordId]`.
		/// </summary>
		public long[] D { get; set; }

		/// <summary>
		/// "Flip bits", a 2-bits integer to represent the mirror transformations of the tile.<br/>
		/// - Bit 0 = X flip<br/>   - Bit 1 = Y flip<br/>   Examples: f=0 (no flip), f=1 (X flip
		/// only), f=2 (Y flip only), f=3 (both flips)
		/// </summary>
		public long F { get; set; }

		/// <summary>
		/// Pixel coordinates of the tile in the **layer** (`[x,y]` format). Don't forget optional
		/// layer offsets, if they exist!
		/// </summary>
		public long[] Px { get; set; }

		/// <summary>
		/// Pixel coordinates of the tile in the **tileset** (`[x,y]` format)
		/// </summary>
		public long[] Src { get; set; }

		/// <summary>
		/// The *Tile ID* in the corresponding tileset.
		/// </summary>
		public long T { get; set; }
	}

	public class EntityInstance {
		/// <summary>
		/// Grid-based coordinates (`[x,y]` format)
		/// </summary>
		public long[] Grid { get; set; }

		/// <summary>
		/// Entity definition identifier
		/// </summary>
		public string Identifier { get; set; }

		/// <summary>
		/// Pivot coordinates  (`[x,y]` format, values are from 0 to 1) of the Entity
		/// </summary>
		public double[] Pivot { get; set; }

		/// <summary>
		/// Optional Tile used to display this entity (it could either be the default Entity tile, or
		/// some tile provided by a field value, like an Enum).
		/// </summary>
		public EntityInstanceTile Tile { get; set; }

		/// <summary>
		/// Reference of the **Entity definition** UID
		/// </summary>
		public long DefUid { get; set; }

		/// <summary>
		/// An array of all custom fields and their values.
		/// </summary>
		public FieldInstance[] FieldInstances { get; set; }

		/// <summary>
		/// Entity height in pixels. For non-resizable entities, it will be the same as Entity
		/// definition.
		/// </summary>
		public long Height { get; set; }

		/// <summary>
		/// Pixel coordinates (`[x,y]` format) in current level coordinate space. Don't forget
		/// optional layer offsets, if they exist!
		/// </summary>
		public long[] Px { get; set; }

		/// <summary>
		/// Entity width in pixels. For non-resizable entities, it will be the same as Entity
		/// definition.
		/// </summary>
		public long Width { get; set; }
	}

	/// <summary>
	/// Tile data in an Entity instance
	/// </summary>
	public class EntityInstanceTile {
		/// <summary>
		/// An array of 4 Int values that refers to the tile in the tileset image: `[ x, y, width,
		/// height ]`
		/// </summary>
		public long[] SrcRect { get; set; }

		/// <summary>
		/// Tileset ID
		/// </summary>
		public long TilesetUid { get; set; }
	}

	/// <summary>
	/// IntGrid value instance
	/// </summary>
	public class IntGridValueInstance {
		/// <summary>
		/// Coordinate ID in the layer grid
		/// </summary>
		public long CoordId { get; set; }

		/// <summary>
		/// IntGrid value
		/// </summary>
		public long V { get; set; }
	}

	/// <summary>
	/// Nearby level info
	/// </summary>
	public class NeighbourLevel {
		/// <summary>
		/// A single lowercase character tipping on the level location (`n`orth, `s`outh, `w`est,
		/// `e`ast).
		/// </summary>
		public string Dir { get; set; }

		public long LevelUid { get; set; }
	}

	/// <summary>
	/// Possible values: `Hidden`, `ValueOnly`, `NameAndValue`, `EntityTile`, `Points`,
	/// `PointStar`, `PointPath`, `PointPathLoop`, `RadiusPx`, `RadiusGrid`
	/// </summary>
	public enum EditorDisplayMode { EntityTile, Hidden, NameAndValue, PointPath, PointPathLoop, PointStar, Points, RadiusGrid, RadiusPx, ValueOnly };

	/// <summary>
	/// Possible values: `Above`, `Center`, `Beneath`
	/// </summary>
	public enum EditorDisplayPos { Above, Beneath, Center };

	public enum TextLanguageMode { LangC, LangHaxe, LangJs, LangJson, LangLua, LangMarkdown, LangPython, LangRuby, LangXml };

	/// <summary>
	/// Possible values: `DiscardOldOnes`, `PreventAdding`, `MoveLastOne`
	/// </summary>
	public enum LimitBehavior { DiscardOldOnes, MoveLastOne, PreventAdding };

	/// <summary>
	/// If TRUE, the maxCount is a "per world" limit, if FALSE, it's a "per level". Possible
	/// values: `PerLayer`, `PerLevel`, `PerWorld`
	/// </summary>
	public enum LimitScope { PerLayer, PerLevel, PerWorld };

	/// <summary>
	/// Possible values: `Rectangle`, `Ellipse`, `Tile`, `Cross`
	/// </summary>
	public enum RenderMode { Cross, Ellipse, Rectangle, Tile };

	/// <summary>
	/// Possible values: `Cover`, `FitInside`, `Repeat`, `Stretch`
	/// </summary>
	public enum TileRenderMode { Cover, FitInside, Repeat, Stretch };

	/// <summary>
	/// Checker mode Possible values: `None`, `Horizontal`, `Vertical`
	/// </summary>
	public enum Checker { Horizontal, None, Vertical };

	/// <summary>
	/// Defines how tileIds array is used Possible values: `Single`, `Stamp`
	/// </summary>
	public enum TileMode { Single, Stamp };

	/// <summary>
	/// Type of the layer as Haxe Enum Possible values: `IntGrid`, `Entities`, `Tiles`,
	/// `AutoLayer`
	/// </summary>
	public enum TypeEnum { AutoLayer, Entities, IntGrid, Tiles };

	public enum Flag { DiscardPreCsvIntGrid, IgnoreBackupSuggest };

	/// <summary>
	/// "Image export" option when saving project. Possible values: `None`, `OneImagePerLayer`,
	/// `OneImagePerLevel`
	/// </summary>
	public enum ImageExportMode { None, OneImagePerLayer, OneImagePerLevel };

	public enum BgPos { Contain, Cover, CoverDirty, Unscaled };

	/// <summary>
	/// An enum that describes how levels are organized in this project (ie. linearly or in a 2D
	/// space). Possible values: `Free`, `GridVania`, `LinearHorizontal`, `LinearVertical`
	/// </summary>
	public enum WorldLayout { Free, GridVania, LinearHorizontal, LinearVertical };
}
