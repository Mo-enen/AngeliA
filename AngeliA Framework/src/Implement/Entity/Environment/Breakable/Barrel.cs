using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class Barrel : Breakable, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }

}
