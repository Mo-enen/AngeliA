namespace LdtkToAngeliA {



	/// <summary>
	/// This file is a JSON schema of files created by LDtk level editor (https://ldtk.io).
	///
	/// This is the root of any Project JSON file. It contains:  - the project settings, - an
	/// array of levels, - a group of definitions (that can probably be safely ignored for most
	/// users).
	/// </summary>
	public partial class LdtkJson {
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
	public partial class Definitions {
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





	public enum EditorDisplayMode { EntityTile, Hidden, NameAndValue, PointPath, PointPathLoop, PointStar, Points, RadiusGrid, RadiusPx, ValueOnly };

	public enum EditorDisplayPos { Above, Beneath, Center };

	public enum TextLanguageMode { LangC, LangHaxe, LangJs, LangJson, LangLua, LangMarkdown, LangPython, LangRuby, LangXml };

	public enum LimitBehavior { DiscardOldOnes, MoveLastOne, PreventAdding };

	public enum LimitScope { PerLayer, PerLevel, PerWorld };

	public enum RenderMode { Cross, Ellipse, Rectangle, Tile };

	public enum TileRenderMode { Cover, FitInside, Repeat, Stretch };

	public enum Checker { Horizontal, None, Vertical };

	public enum TileMode { Single, Stamp };

	public enum TypeEnum { AutoLayer, Entities, IntGrid, Tiles };

	public enum Flag { DiscardPreCsvIntGrid, IgnoreBackupSuggest };

	public enum ImageExportMode { None, OneImagePerLayer, OneImagePerLevel };

	public enum BgPos { Contain, Cover, CoverDirty, Unscaled };

	public enum WorldLayout { Free, GridVania, LinearHorizontal, LinearVertical };
}
