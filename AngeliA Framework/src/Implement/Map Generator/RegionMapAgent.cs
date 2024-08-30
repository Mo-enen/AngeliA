using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public sealed class FailbackAgent : RegionMapAgent {


}


public sealed class TestAgent0 : RegionMapAgent {
	public override float Priority => 2f;

}
public sealed class TestAgent1 : RegionMapAgent {
	public override float Priority => 1f;

}
public sealed class TestAgent2 : RegionMapAgent {
	public override float Priority => 0.5f;

}



public abstract class RegionMapAgent {

	// Api
	public virtual float Priority => 1f;




}
