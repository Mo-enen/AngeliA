

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class FieldInstance {
		/// <summary>
		/// Field definition identifier
		/// </summary>
		public string identifier;

		/// <summary>
		/// Type of the field, such as `Int`, `Float`, `Enum(my_enum_name)`, `Bool`, etc.
		/// </summary>
		public string type;

		/// <summary>
		/// Actual value of the field instance. The value type may vary, depending on `__type`
		/// (Integer, Boolean, String etc.)<br/>  It can also be an `Array` of those same types.
		/// </summary>
		public object value;

		/// <summary>
		/// Reference of the **Field definition** UID
		/// </summary>
		public long defUid;

		/// <summary>
		/// Editor internal raw values
		/// </summary>
		public object[] realEditorValues;
	}
}
