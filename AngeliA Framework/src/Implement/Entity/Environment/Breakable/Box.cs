using System.Collections;
using System.Collections.Generic;



namespace AngeliA; 
public class Box : Breakable, ICombustible {
	public int BurnedDuration => 320;
	int ICombustible.BurnStartFrame { get; set; }
	
}
