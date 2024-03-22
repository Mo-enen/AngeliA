using System.Collections;
using System.Collections.Generic;

namespace AngeliA; 
public class Basket : Furniture, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}
