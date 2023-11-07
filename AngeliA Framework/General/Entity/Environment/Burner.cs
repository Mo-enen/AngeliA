using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {

	public class CommonBurnerLeft : Burner<CommonFire> {
		protected override Direction4 Direction => Direction4.Left;
	}

	public class CommonBurnerRight : Burner<CommonFire> {
		protected override Direction4 Direction => Direction4.Right;
	}

	public class CommonBurnerDown : Burner<CommonFire> {
		protected override Direction4 Direction => Direction4.Down;
	}

	public class CommonBurnerUp : Burner<CommonFire> {
		protected override Direction4 Direction => Direction4.Up;
	}

}
