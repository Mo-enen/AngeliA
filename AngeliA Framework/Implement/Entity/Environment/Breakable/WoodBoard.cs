using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
public class WoodBoard : Breakable, ICombustible {
	public int BurnedDuration => 30;
	int ICombustible.BurnStartFrame { get; set; }
	protected override bool PhysicsEnable => true;
	protected override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	protected override bool DestroyWhenInsideGround => true;
}
