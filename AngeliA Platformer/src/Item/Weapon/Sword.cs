
using AngeliA;namespace AngeliA.Platformer;


public abstract class Sword<B> : Sword where B : MeleeBullet {
	public Sword () => BulletID = typeof(B).AngeHash();
}
public abstract class Sword : MeleeWeapon {
	public sealed override WeaponType WeaponType => WeaponType.Sword;
	public override int RangeXLeft => 275;
	public override int RangeXRight => 275;
	public override int RangeY => 432;
}
