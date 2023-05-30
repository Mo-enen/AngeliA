using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eDiningTableA : eDiningTable { }
	public class eDiningTableB : eDiningTable { }
	public class eDiningTableC : eDiningTable { }
	public class eDiningTableD : eDiningTable { }
	public abstract class eDiningTable : Furniture, ICombustible {
		protected override Direction3 ModuleType => Direction3.Horizontal;
		int ICombustible.BurnStartFrame { get; set; }
	}
}
