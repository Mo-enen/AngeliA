using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	public class ChairWoodA : Chair, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class ChairWoodB : Chair, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class ChairWoodC : Chair, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class ChairWoodD : Chair, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
}
