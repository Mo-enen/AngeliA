using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eWardrobeA : Wardrobe, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class eWardrobeB : Wardrobe, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class eWardrobeC : Wardrobe, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class eWardrobeD : Wardrobe, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}


	public abstract class Wardrobe : MiniGame_PlayerCustomizer {


		private static readonly int NAME_CODE = "Hint.Wardrobe".AngeHash();
		protected override string DisplayName => Language.Get(NAME_CODE, "Change Cloth");
		protected override bool BodypartAvailable => false;
		protected override bool SuitAvailable => true;

	}
}