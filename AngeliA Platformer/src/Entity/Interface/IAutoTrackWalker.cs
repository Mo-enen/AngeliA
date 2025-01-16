using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public interface IAutoTrackWalker : IRouteWalker {
	public bool OnTrack => Game.GlobalFrame <= LastWalkingFrame + 1;
	public int LastWalkingFrame { get; set; }
	public int WalkStartFrame { get; set; }
	public int TrackWalkSpeedRate => 1000;
}
