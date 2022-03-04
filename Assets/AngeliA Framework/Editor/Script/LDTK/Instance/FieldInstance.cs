

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class FieldInstance {
		/// <summary>
		/// Field definition identifier
		/// </summary>
		public string __identifier;

		/// <summary>
		/// Actual value of the field instance. The value type may vary, depending on `__type`
		/// (Integer, Boolean, String etc.)<br/>  It can also be an `Array` of those same types.
		/// </summary>
		public int __value;

		/// <summary>
		/// Reference of the **Field definition** UID
		/// </summary>
		public int defUid;

	}
}
