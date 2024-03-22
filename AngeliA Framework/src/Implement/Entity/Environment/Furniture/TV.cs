using System.Collections;
using System.Collections.Generic;

namespace AngeliA; 
public class TV : Furniture, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}
