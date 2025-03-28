namespace AngeliA;

/// <summary>
/// Which direction can the weapon attack to
/// </summary>
public enum WeaponValidDirection {
	/// <summary>
	/// ← →
	/// </summary>
	Two = 2,
	/// <summary>
	/// ← → ↑
	/// </summary>
	Three = 3,
	/// <summary>
	/// ← → ↑ ↓
	/// </summary>
	Four = 4,
	/// <summary>
	/// ← → ↑ ↖ ↗
	/// </summary>
	Five = 5,
	/// <summary>
	/// ← → ↑ ↖ ↗ ↓ ↙ ↘
	/// </summary>
	Eight = 8,
}


/// <inheritdoc cref="Weapon"/>
public abstract class Weapon<B> : Weapon where B : Bullet {
	public Weapon () : base() => BulletID = typeof(B).AngeHash();
}


/// <summary>
/// A type of handtool that launch a type of bullet when being used
/// </summary>
public abstract class Weapon : HandTool {

	// VAR
	/// <summary>
	/// Entity ID of the bullet
	/// </summary>
	public int BulletID { get; protected set; }
	/// <summary>
	/// Which direction can this weapon attack
	/// </summary>
	protected virtual WeaponValidDirection ValidDirection => WeaponValidDirection.Two;

	// MSG
	public override void OnPoseAnimationUpdate_FromEquipment (PoseCharacterRenderer rendering) {
		base.OnPoseAnimationUpdate_FromEquipment(rendering);
		var character = rendering.TargetCharacter;
		var attackness = character.Attackness;
		switch (ValidDirection) {
			case WeaponValidDirection.Two:
				attackness.IgnoreAimingDirection(Direction8.Bottom);
				attackness.IgnoreAimingDirection(Direction8.Top);
				attackness.IgnoreAimingDirection(Direction8.TopLeft);
				attackness.IgnoreAimingDirection(Direction8.TopRight);
				attackness.IgnoreAimingDirection(Direction8.BottomLeft);
				attackness.IgnoreAimingDirection(Direction8.BottomRight);
				break;
			case WeaponValidDirection.Three:
				attackness.IgnoreAimingDirection(Direction8.Bottom);
				attackness.IgnoreAimingDirection(Direction8.TopLeft);
				attackness.IgnoreAimingDirection(Direction8.TopRight);
				attackness.IgnoreAimingDirection(Direction8.BottomLeft);
				attackness.IgnoreAimingDirection(Direction8.BottomRight);
				break;
			case WeaponValidDirection.Four:
				attackness.IgnoreAimingDirection(Direction8.TopLeft);
				attackness.IgnoreAimingDirection(Direction8.TopRight);
				attackness.IgnoreAimingDirection(Direction8.BottomLeft);
				attackness.IgnoreAimingDirection(Direction8.BottomRight);
				break;
			case WeaponValidDirection.Five:
				attackness.IgnoreAimingDirection(Direction8.BottomLeft);
				attackness.IgnoreAimingDirection(Direction8.BottomRight);
				attackness.IgnoreAimingDirection(Direction8.Bottom);
				break;
		}
	}

	/// <summary>
	/// Spawn the bullet entity
	/// </summary>
	/// <param name="sender">Character that use the weapon</param>
	/// <returns>Instance of the spawned bullet entity. Return null when invalid</returns>
	public virtual Bullet SpawnBullet (Character sender) {
		if (sender == null || BulletID == 0) return null;
		var rect = sender.Rect;
		if (Stage.SpawnEntity(BulletID, rect.x, rect.y) is not Bullet bullet) return null;
		bullet.Sender = sender;
		var sourceRect = sender.Rect;
		bullet.X = sourceRect.CenterX() - bullet.Width / 2;
		bullet.Y = sourceRect.CenterY() - bullet.Height / 2;
		return bullet;
	}

}