

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class FieldDefinition {
		/// <summary>
		/// Human readable value type (eg. `Int`, `Float`, `Point`, etc.). If the field is an array,
		/// this field will look like `Array<...>` (eg. `Array<Int>`, `Array<Point>` etc.)
		/// </summary>
		public string type;

		/// <summary>
		/// Optional list of accepted file extensions for FilePath value type. Includes the dot:
		/// `.ext`
		/// </summary>
		public string[] acceptFileTypes;

		/// <summary>
		/// Array max length
		/// </summary>
		public long? arrayMaxLength;

		/// <summary>
		/// Array min length
		/// </summary>
		public long? arrayMinLength;

		/// <summary>
		/// TRUE if the value can be null. For arrays, TRUE means it can contain null values
		/// (exception: array of Points can't have null values).
		/// </summary>
		public bool canBeNull;

		/// <summary>
		/// Default value if selected value is null or invalid.
		/// </summary>
		public object defaultOverride;

		public bool editorAlwaysShow;

		public bool editorCutLongValues;

		/// <summary>
		/// Possible values: `Hidden`, `ValueOnly`, `NameAndValue`, `EntityTile`, `Points`,
		/// `PointStar`, `PointPath`, `PointPathLoop`, `RadiusPx`, `RadiusGrid`
		/// </summary>
		public EditorDisplayMode editorDisplayMode;

		/// <summary>
		/// Possible values: `Above`, `Center`, `Beneath`
		/// </summary>
		public EditorDisplayPos editorDisplayPos;

		/// <summary>
		/// Unique String identifier
		/// </summary>
		public string identifier;

		/// <summary>
		/// TRUE if the value is an array of multiple values
		/// </summary>
		public bool isArray;

		/// <summary>
		/// Max limit for value, if applicable
		/// </summary>
		public double? max;

		/// <summary>
		/// Min limit for value, if applicable
		/// </summary>
		public double? min;

		/// <summary>
		/// Optional regular expression that needs to be matched to accept values. Expected format:
		/// `/some_reg_ex/g`, with optional "i" flag.
		/// </summary>
		public string regex;

		/// <summary>
		/// Possible values: &lt;`null`&gt;, `LangPython`, `LangRuby`, `LangJS`, `LangLua`, `LangC`,
		/// `LangHaxe`, `LangMarkdown`, `LangJson`, `LangXml`
		/// </summary>
		public TextLangageMode? textLangageMode;

		/// <summary>
		/// Internal type enum
		/// </summary>
		public object fieldDefinitionType;

		/// <summary>
		/// Unique Int identifier
		/// </summary>
		public long uid;
	}
}
