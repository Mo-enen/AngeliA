using System.Collections;
using System.Collections.Generic;

namespace AngeliA.Framework; 
public class Basket : Furniture, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}
