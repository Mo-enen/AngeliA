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
		damage.Type &= ~IgnoreDamageType;
		if (damage.Type == Tag.None) return false;
		if ((Team & damage.TargetTeam) != Team) return false;
		OnDamaged(damage);
		OnDamagedCallback?.Invoke(damage, this);
		return true;
	}

}
