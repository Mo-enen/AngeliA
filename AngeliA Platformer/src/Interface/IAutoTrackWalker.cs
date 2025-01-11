using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public interface IAutoTrackWalker : IRouteWalker {
	public int LastWalkingFrame { get; set; }
	public int WalkStartFrame { get; set; }
	public int TrackWalkSpeed => 8;
}
