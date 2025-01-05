namespace AngeliA;

public interface IDamageReceiver {

	public int Team { get; }
	public bool TakeDamageFromLevel => true;
	public Tag IgnoreDamageType => Tag.None;

	void TakeDamage (Damage damage);

}