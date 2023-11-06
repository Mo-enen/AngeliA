using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGeneral {
	public class eChairWoodA : Chair, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class eChairWoodB : Chair, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class eChairWoodC : Chair, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class eChairWoodD : Chair, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
}
