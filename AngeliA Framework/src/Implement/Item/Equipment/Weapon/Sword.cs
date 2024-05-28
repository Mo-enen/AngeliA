using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 


// Sword
public abstract class Sword<B> : Sword where B : MeleeBullet {
	public Sword () => BulletID = typeof(B).AngeHash();
}
public abstract class Sword : MeleeWeapon {
	public sealed override WeaponType WeaponType => WeaponType.Sword;
	public override int RangeXLeft => 275;
	public override int RangeXRight => 275;
	public override int RangeY => 432;
}


// Implement

public class iSwordWood : Sword {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iSwordIron : Sword {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iSwordGold : Sword {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iDagger : Sword {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iSwordCrimson : Sword {
	public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
}

public class iSwordScarlet : Sword {
	public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
}

public class iScimitar : Sword {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iSwordPirate : Sword {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iSwordAgile : Sword {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iScimitarAgile : Sword {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iSwordJagged : Sword {
	public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
}

public class iSwordGreat : Sword {
	public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
}

public class iSwordDark : Sword {
	public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
}

public class iSwordCrutch : Sword {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iKnifeGiant : Sword {
	public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
}
