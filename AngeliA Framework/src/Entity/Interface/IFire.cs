namespace AngeliA;

/// <summary>
/// Interface that makes the entity behave like fire
/// </summary>
public interface IFire {

	// INTER
	/// <summary>
	/// Set fire onto a target
	/// </summary>
	public void Setup (ICombustible com);

	/// <summary>
	/// Putout this fire
	/// </summary>
	/// <param name="manually">True if the fire got putout by something else</param>
	public void Putout (bool manually);

	// API
	/// <summary>
	/// Set fire at given range
	/// </summary>
	/// <param name="fireID">Which type of fire will be spawn</param>
	/// <param name="rect">Target range in global space</param>
	/// <param name="ignore">Do not set this entity on fire</param>
	public static void SpreadFire (int fireID, IRect rect, Entity ignore = null) {
		var hits = Physics.OverlapAll(
			PhysicsMask.ENTITY,
			rect, out int count,
			ignore, OperationMode.ColliderAndTrigger
		);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not ICombustible com || !hit.Entity.Active || com.IsBurning) continue;
			if (Stage.TrySpawnEntity(fireID, hit.Rect.x, hit.Rect.y, out var fEntity) && fEntity is IFire fire) {
				fire.Setup(com);
			}
		}
	}

	/// <summary>
	/// Putout fire in given range
	/// </summary>
	/// <param name="rect">Range in global space</param>
	public static void PutoutFire (IRect rect) {
		var hits = Physics.OverlapAll(
			PhysicsMask.ENVIRONMENT,
			rect, out int count,
			null, OperationMode.TriggerOnly
		);
		for (int i = 0; i < count; i++) {
			var e = hits[i].Entity;
			if (e is not IFire fire) continue;
			fire.Putout(true);
		}
	}

}