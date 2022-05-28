using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eFridge : eFurniture {

		private static readonly int CODE = "Fridge".AngeHash();
		private static readonly int CODE_OPEN = "Fridge Open".AngeHash();

		protected override Direction3 ModuleType => Direction3.None;
		protected override int ArtworkCode_LeftDown => CODE;
		protected override int ArtworkCode_Mid => CODE;
		protected override int ArtworkCode_RightUp => CODE;
		protected override int ArtworkCode_Single => Open ? CODE_OPEN : CODE;
		public bool Open { get; private set; } = false;

	}
}
