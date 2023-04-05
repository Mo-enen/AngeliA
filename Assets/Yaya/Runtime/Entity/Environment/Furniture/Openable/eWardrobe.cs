using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eWardrobe : OpenableFurniture {


		private static readonly int CODE_OPEN = "Wardrobe Open".AngeHash();

		protected override Direction3 ModuleType => Direction3.Vertical;
		protected override int ArtworkCode_LeftDown => Open ? CODE_OPEN : TypeID;
		protected override int ArtworkCode_Mid => Open ? CODE_OPEN : TypeID;
		protected override int ArtworkCode_RightUp => Open ? CODE_OPEN : TypeID;
		protected override int ArtworkCode_Single => Open ? CODE_OPEN : TypeID;


	}
}
