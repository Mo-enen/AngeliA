namespace AngeliA;

/// <summary>
/// This interface makes the entity can be set on fire
/// </summary>
public interface ICombustible {
	/// <summary>
	/// True if the entity is having a fire entity on
	/// </summary>
	public bool IsBurning => this is Entity e && e.SpawnFrame <= BurnStartFrame;
	/// <summary>
	/// How many frames does it takes to burn down this entity
	/// </summary>
	public int BurnedDuration => 120;
	/// <summary>
	/// The frame this entity start on fire
	/// </summary>
	public int BurnStartFrame { get; set; }
	/// <summary>
	/// This function is called when the entity is burned down
	/// </summary>
	public void OnBurned () {
		if (this is not Entity e) return;
		FrameworkUtil.RemoveFromWorldMemory(e);
	}
}
