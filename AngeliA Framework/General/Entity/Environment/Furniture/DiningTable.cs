using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {

	public class DiningTableA : Table, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}

	public class DiningTableB : Table, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}

	public class DiningTableC : Table, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}

	public class DiningTableD : Table, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}

}
