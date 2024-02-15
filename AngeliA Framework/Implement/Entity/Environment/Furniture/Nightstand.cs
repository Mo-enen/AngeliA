using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
public class Nightstand : Furniture, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}
