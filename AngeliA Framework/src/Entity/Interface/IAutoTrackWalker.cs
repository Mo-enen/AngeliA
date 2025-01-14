using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public interface IAutoTrackWalker : IRouteWalker {
	public bool OnTrack => Game.GlobalFrame <= LastWalkingFrame + 1;
	public int LastWalkingFrame { get; set; }
	public int WalkStartFrame { get; set; }
	public int TrackWalkSpeed => 12;
}
