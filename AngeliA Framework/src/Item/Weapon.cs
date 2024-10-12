namespace AngeliA;

public abstract class Weapon<B> : Weapon where B : Bullet {
	public Weapon () : base() => BulletID = typeof(B).AngeHash();
}


public abstract class Weapon : HandTool {

}