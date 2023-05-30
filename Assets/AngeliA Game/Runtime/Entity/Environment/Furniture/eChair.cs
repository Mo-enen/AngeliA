using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eChairWoodA : Furniture, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class eChairWoodB : Furniture, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class eChairWoodC : Furniture, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class eChairWoodD : Furniture, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
}
