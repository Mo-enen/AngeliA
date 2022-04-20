using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eMeatLobster : eItem {
		private static readonly int CODE = "Lobster Raw".AngeHash();
		private static readonly int CODE_COOKED = "Lobster Cooked".AngeHash();
		protected override int ItemCode => Cooked ? CODE_COOKED : CODE;
		private bool Cooked = false;











	}
}
