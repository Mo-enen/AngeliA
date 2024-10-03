namespace AngeliA;

public interface ICombustible {
	public bool IsBurning => this is Entity e && e.SpawnFrame <= BurnStartFrame;
	public int BurnedDuration => 120;
	public int BurnStartFrame { get; set; }
	public void OnBurned () {
		if (this is not Entity e) return;
		FrameworkUtil.RemoveFromWorldMemory(e);
	}
}
