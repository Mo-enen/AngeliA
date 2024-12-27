namespace AngeliA;

public interface IFire {

	// INTER
	public void Setup (ICombustible com);

	public void Putout (bool manually);

	// API
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