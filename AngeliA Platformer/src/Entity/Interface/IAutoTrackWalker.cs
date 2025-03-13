using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public interface IAutoTrackWalker : IRouteWalker {

	public bool OnTrack => Game.GlobalFrame <= LastWalkingFrame + 1;
	public int LastWalkingFrame { get; set; }
	public int WalkStartFrame { get; set; }
	public int TrackWalkSpeedRate => 1000;
	private static readonly HashSet<int> WalkerSet = new(typeof(IAutoTrackWalker).AllClassImplementedID());

	public static bool IsTypeAutoTrackWalker (int id) => WalkerSet.Contains(id);

}
