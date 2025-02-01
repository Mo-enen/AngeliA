namespace AngeliA;

public enum WeaponValidDirection {
	Two = 2,    // ← →
	Three = 3,  // ← → ↑
	Four = 4,   // ← → ↑ ↓
	Five = 5,   // ← → ↑ ↖ ↗
	Eight = 8,  // ← → ↑ ↖ ↗ ↓ ↙ ↘
}

public abstract class Weapon<B> : Weapon where B : Bullet {
	public Weapon () : base() => BulletID = typeof(B).AngeHash();
}


public abstract class Weapon : HandTool {
	protected virtual WeaponValidDirection ValidDirection => WeaponValidDirection.Two;
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
}