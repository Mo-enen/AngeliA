using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Interace that provide logic for auto walking on tracks
/// </summary>
public interface IAutoTrackWalker : IRouteWalker {

	/// <summary>
	/// True if this object is currently on track
	/// </summary>
	public bool OnTrack => Game.GlobalFrame <= LastWalkingFrame + 1;

	/// <summary>
	/// Last frame for this object when it's walking on track
	/// </summary>
	public int LastWalkingFrame { get; set; }

	/// <summary>
	/// Last frame for this object when it start to walk on track
	/// </summary>
	public int WalkStartFrame { get; set; }

	/// <summary>
	/// Value scale for speed from track. (0 means 0%, 1000 means 100%)
	/// </summary>
	public int TrackWalkSpeedRate => 1000;

	private static readonly HashSet<int> WalkerSet = new(typeof(IAutoTrackWalker).AllClassImplementedID());

	/// <summary>
	/// True if given type ID refers to a valid track walker
	/// </summary>
	public static bool IsTypeAutoTrackWalker (int id) => WalkerSet.Contains(id);

}
