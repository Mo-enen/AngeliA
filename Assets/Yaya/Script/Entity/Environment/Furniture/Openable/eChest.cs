using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eChest : eOpenableFurniture {

		private static readonly int CODE = "Chest".AngeHash();
		private static readonly int CODE_OPEN = "Chest Open".AngeHash();

		protected override Direction3 ModuleType => Direction3.None;
		protected override int ArtworkCode_LeftDown => CODE;
		protected override int ArtworkCode_Mid => CODE;
		protected override int ArtworkCode_RightUp => CODE;
		protected override int ArtworkCode_Single => Open ? CODE_OPEN : CODE;
		protected override bool UseHighlightAnimation => !Open;



	}
}
