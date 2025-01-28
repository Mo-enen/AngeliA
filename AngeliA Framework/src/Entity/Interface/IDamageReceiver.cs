namespace AngeliA;

public interface IDamageReceiver {

	[OnDealDamage_Damage_IDamageReceiver] internal static System.Action<Damage, IDamageReceiver> OnDamagedCallback;
	public int Team { get; }
	public bool IsInvincible => false;
	public bool TakeDamageFromLevel => true;
	public Tag IgnoreDamageType => Tag.None;

	void OnDamaged (Damage damage);

	public bool TakeDamage (Damage damage) {
		if (IsInvincible) return false;
		if (damage.Amount <= 0) return false;
		if (this is Entity e && !e.Active) return false;
		var oldType = damage.Type;
		damage.Type &= ~IgnoreDamageType;
		if (damage.Type == Tag.None) return false;
		if ((Team & damage.TargetTeam) != Team) return false;
		OnDamaged(damage);
		OnDamagedCallback?.Invoke(damage, this);
		return true;
	}

	public static void DamageAllOverlap (IRect rect, Damage damage, int physicsMask = PhysicsMask.ENTITY, Entity host = null, OperationMode mode = OperationMode.ColliderAndTrigger, bool allowMultipleDamage = true) {
		var hits = Physics.OverlapAll(physicsMask, rect, out int count, host, mode);
		for (int i = 0; i < count; i++) {
			if (hits[i].Entity is not IDamageReceiver receiver) continue;
			receiver.TakeDamage(damage);
			if (!allowMultipleDamage) break;
		}
	}

}
