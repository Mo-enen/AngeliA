using System.Collections;
using System.Collections.Generic;

namespace AngeliaFramework {
	public class Basket : Furniture, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
}
