using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eDiningTable : Furniture, ICombustible {

		private static readonly int CODE_LEFT = "Dining Table Left".AngeHash();
		private static readonly int CODE_MID = "Dining Table Mid".AngeHash();
		private static readonly int CODE_RIGHT = "Dining Table Right".AngeHash();
		private static readonly int CODE_SINGLE = "Dining Table Single".AngeHash();

		protected override Direction3 ModuleType => Direction3.Horizontal;
		protected override int ArtworkCode_LeftDown => CODE_LEFT;
		protected override int ArtworkCode_Mid => CODE_MID;
		protected override int ArtworkCode_RightUp => CODE_RIGHT;
		protected override int ArtworkCode_Single => CODE_SINGLE;
		int ICombustible.BurnStartFrame { get; set; }

	}
}
