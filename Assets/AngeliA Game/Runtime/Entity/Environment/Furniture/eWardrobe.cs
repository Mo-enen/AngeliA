using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eWardrobeA : eWardrobe { }
	public class eWardrobeB : eWardrobe { }
	public class eWardrobeC : eWardrobe { }
	public class eWardrobeD : eWardrobe { }

	public abstract class eWardrobe : OpenableFurniture, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
		protected override Direction3 ModuleType => Direction3.Vertical;
	}
}