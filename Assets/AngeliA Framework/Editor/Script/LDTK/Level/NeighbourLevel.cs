

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class NeighbourLevel {
		/// <summary>
		/// A single lowercase character tipping on the level location (`n`orth, `s`outh, `w`est,
		/// `e`ast).
		/// </summary>
		public string dir;

		public long levelUid;
	}
}
