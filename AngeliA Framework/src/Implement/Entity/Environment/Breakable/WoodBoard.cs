using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 
public class WoodBoard : Breakable, ICombustible {
	public int BurnedDuration => 30;
	int ICombustible.BurnStartFrame { get; set; }
	
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	public override bool DestroyWhenInsideGround => true;
}
