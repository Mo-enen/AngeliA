using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public sealed class FailbackAgent : RegionMapAgent {
	public override void Generate () {

	}
}


public sealed class TestAgent0 : RegionMapAgent {
	public override float Priority => 2f;
	public override void Generate () {

	}
}
public sealed class TestAgent1 : RegionMapAgent {
	public override float Priority => 1f;
	public override void Generate () {

	}
}
public sealed class TestAgent2 : RegionMapAgent {
	public override float Priority => 0.5f;
	public override void Generate () {

	}
}



public abstract class RegionMapAgent {

	// Api
	public virtual float Priority => 1f;
	public long Seed;
	public int UnitX;
	public int UnitY;
	public int UnitZ;
	public int Altitude;
	public int ResultLevel;
	public int ResultBG;
	public int ResultEntity;
	public int ResultElement;

	// MSG
	public abstract void Generate ();



}
