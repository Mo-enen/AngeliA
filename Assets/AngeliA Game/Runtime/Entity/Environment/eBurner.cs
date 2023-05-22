using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {

	public class eCommonBurnerLeft : Burner<eCommonFire> {
		protected override Direction4 Direction => Direction4.Left;
	}

	public class eCommonBurnerRight : Burner<eCommonFire> {
		protected override Direction4 Direction => Direction4.Right;
	}

	public class eCommonBurnerDown : Burner<eCommonFire> {
		protected override Direction4 Direction => Direction4.Down;
	}

	public class eCommonBurnerUp : Burner<eCommonFire> {
		protected override Direction4 Direction => Direction4.Up;
	}

}
