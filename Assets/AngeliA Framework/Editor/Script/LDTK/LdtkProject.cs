namespace LdtkToAngeliA {
	[System.Serializable]
	public class LdtkProject {

		/// <summary>
		/// Number of backup files to keep, if the `backupOnSave` is TRUE
		/// </summary>
		public long backupLimit;

		/// <summary>
		/// If TRUE, an extra copy of the project will be created in a sub folder, when saving.
		/// </summary>
		public bool backupOnSave;

		/// <summary>
		/// Project background color
		/// </summary>
		public string bgColor;

		/// <summary>
		/// Default grid size for new layers
		/// </summary>
		public long defaultGridSize;

		/// <summary>
		/// Default background color of levels
		/// </summary>
		public string defaultLevelBgColor;

		/// <summary>
		/// Default new level height
		/// </summary>
		public long defaultLevelHeight;

		/// <summary>
		/// Default new level width
		/// </summary>
		public long defaultLevelWidth;

		/// <summary>
		/// Default X pivot (0 to 1) for new entities
		/// </summary>
		public double defaultPivotX;

		/// <summary>
		/// Default Y pivot (0 to 1) for new entities
		/// </summary>
		public double defaultPivotY;

		/// <summary>
		/// A structure containing all the definitions of this project
		/// </summary>
		public Definitions defs;

		/// <summary>
		/// If TRUE, all layers in all levels will also be exported as PNG along with the project
		/// file (default is FALSE)
		/// </summary>
		public bool exportPng;

		/// <summary>
		/// If TRUE, a Tiled compatible file will also be generated along with the LDtk JSON file
		/// (default is FALSE)
		/// </summary>
		public bool exportTiled;

		/// <summary>
		/// If TRUE, one file will be saved for the project (incl. all its definitions) and one file
		/// in a sub-folder for each level.
		/// </summary>
		public bool externalLevels;

		/// <summary>
		/// An array containing various advanced flags (ie. options or other states). Possible
		/// values: `DiscardPreCsvIntGrid`, `IgnoreBackupSuggest`
		/// </summary>
		public Flag[] flags;

		/// <summary>
		/// File format version
		/// </summary>
		public string jsonVersion;

		/// <summary>
		/// The default naming convention for level identifiers.
		/// </summary>
		public string levelNamePattern;

		/// <summary>
		/// All levels. The order of this array is only relevant in `LinearHorizontal` and
		/// `linearVertical` world layouts (see `worldLayout` value). Otherwise, you should refer to
		/// the `worldX`,`worldY` coordinates of each Level.
		/// </summary>
		public Level[] levels;

		/// <summary>
		/// If TRUE, the Json is partially minified (no indentation, nor line breaks, default is
		/// FALSE)
		/// </summary>
		public bool minifyJson;

		/// <summary>
		/// Next Unique integer ID available
		/// </summary>
		public long nextUid;

		/// <summary>
		/// File naming pattern for exported PNGs
		/// </summary>
		public string pngFilePattern;

		/// <summary>
		/// Height of the world grid in pixels.
		/// </summary>
		public long worldGridHeight;

		/// <summary>
		/// Width of the world grid in pixels.
		/// </summary>
		public long worldGridWidth;

		/// <summary>
		/// An enum that describes how levels are organized in this project (ie. linearly or in a 2D
		/// space). Possible values: `Free`, `GridVania`, `LinearHorizontal`, `LinearVertical`
		/// </summary>
		public WorldLayout worldLayout;
	}

}
