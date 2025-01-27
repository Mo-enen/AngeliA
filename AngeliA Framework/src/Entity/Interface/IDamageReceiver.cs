namespace AngeliA;

public interface IDamageReceiver {

	[OnDealDamage_Damage_IDamageReceiver] internal static System.Action<Damage, IDamageReceiver> OnDamagedCallback;
	public int Team { get; }
	public bool IsInvincible => false;
	public bool TakeDamageFromLevel => true;
	public Tag IgnoreDamageType => Tag.None;

	void OnDamaged (Damage damage);

	public void TakeDamage (Damage damage) {
		if (IsInvincible) return;
		if (damage.Amount <= 0) return;
		if (this is Entity e && !e.Active) return;
		damage.Type &= ~IgnoreDamageType;
		if (damage.Type == Tag.None) return;
		if ((Team & damage.TargetTeam) != Team) return;
		OnDamaged(damage);
		OnDamagedCallback?.Invoke(damage, this);
	}

}
