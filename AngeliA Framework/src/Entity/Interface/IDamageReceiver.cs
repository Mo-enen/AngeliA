namespace AngeliA;

/// <summary>
/// Interface that makes entity take damage from other
/// </summary>
public interface IDamageReceiver {

	[OnDealDamage_Damage_IDamageReceiver] internal static System.Action<Damage, IDamageReceiver> OnDamagedCallback;
	/// <summary>
	/// Which team does this entity belongs to
	/// </summary>
	public int Team { get; }
	/// <summary>
	/// True if this entity is invincible
	/// </summary>
	public bool IsInvincible => false;
	/// <summary>
	/// True if this entity take damage when overlap with colliders from PhysicsLayer.Damage
	/// </summary>
	public bool TakeDamageFromLevel => true;
	/// <summary>
	/// This entity do not take damage with this tags
	/// </summary>
	public Tag IgnoreDamageType => Tag.None;

	/// <summary>
	/// This function is called when the entity takes a damage
	/// </summary>
	void OnDamaged (Damage damage);

	/// <summary>
	/// True if the damage will be take by this entity
	/// </summary>
	public bool ValidDamage (Damage damage) {
		if (IsInvincible) return false;
		if (damage.Amount <= 0) return false;
		if (this is Entity e && !e.Active) return false;
		damage.Type &= ~IgnoreDamageType;
		if (damage.Type == Tag.None) return false;
		if ((Team & damage.TargetTeam) != Team) return false;
		return true;
	}

	/// <summary>
	/// Make this entity take the given damage. This function will call ValidDamage internally
	/// </summary>
	/// <returns>True if the damage is taken</returns>
	public bool TakeDamage (Damage damage) {
		if (!ValidDamage(damage)) return false;
		OnDamaged(damage);
		OnDamagedCallback?.Invoke(damage, this);
		return true;
	}

	/// <summary>
	/// Deal damage to all IDamageReceiver overlap by given range
	/// </summary>
	/// <param name="rect">The range in global space</param>
	/// <param name="damage"></param>
	/// <param name="physicsMask">Which physics layer is included</param>
	/// <param name="host">Sender of this damage</param>
	/// <param name="mode">Does this operation include collider or trigger</param>
	/// <param name="allowMultipleDamage">True if more than one receiver will take damage</param>
	public static void DamageAllOverlap (IRect rect, Damage damage, int physicsMask = PhysicsMask.ENTITY, Entity host = null, OperationMode mode = OperationMode.ColliderAndTrigger, bool allowMultipleDamage = true) {
		var hits = Physics.OverlapAll(physicsMask, rect, out int count, host, mode);
		for (int i = 0; i < count; i++) {
			if (hits[i].Entity is not IDamageReceiver receiver) continue;
			receiver.TakeDamage(damage);
			if (!allowMultipleDamage) break;
		}
	}

}
