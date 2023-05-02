using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace AngeliaGame {
	public class eChestWood : eChest {
		private static readonly int CODE_OPEN = "ChestWood Open".AngeHash();
		protected override int OpenCode => CODE_OPEN;
	}
	public class eChestIron : eChest {
		private static readonly int CODE_OPEN = "ChestIron Open".AngeHash();
		protected override int OpenCode => CODE_OPEN;
	}
	public abstract class eChest : OpenableFurniture {

		protected abstract int OpenCode { get; }
		protected override Direction3 ModuleType => Direction3.None;
		protected override int ArtworkCode_LeftDown => TypeID;
		protected override int ArtworkCode_Mid => TypeID;
		protected override int ArtworkCode_RightUp => TypeID;
		protected override int ArtworkCode_Single => Open ? OpenCode : TypeID;

	}
}
