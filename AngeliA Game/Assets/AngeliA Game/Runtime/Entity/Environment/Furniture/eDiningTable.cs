using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {

	public class eDiningTableA : Table, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}

	public class eDiningTableB : Table, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}

	public class eDiningTableC : Table, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}

	public class eDiningTableD : Table, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}

}
