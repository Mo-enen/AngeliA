namespace AngeliA;

/// <summary>
/// Basic unit of a physics data structure
/// </summary>
public struct PhysicsCell {

	public static readonly PhysicsCell EMPTY = new();

	/// <summary>
	/// Rect position in global space
	/// </summary>
	public IRect Rect;
	/// <summary>
	/// Target entity (null if from block)
	/// </summary>
	public Entity Entity;
	internal uint Frame;
	/// <summary>
	/// True if this cell is marked as trigger
	/// </summary>
	public bool IsTrigger;
	public Tag Tag;
	/// <summary>
	/// ID for identify which object filled this cell
	/// </summary>
	public int SourceID;

}
