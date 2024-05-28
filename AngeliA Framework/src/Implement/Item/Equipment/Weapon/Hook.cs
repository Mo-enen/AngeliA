using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 



// Hook
public abstract class Hook<B> : Hook where B : MeleeBullet {
	public Hook () => BulletID = typeof(B).AngeHash();
}
public abstract class Hook : MeleeWeapon {
	public sealed override WeaponType WeaponType => WeaponType.Hook;
	public override int RangeXLeft => 275;
	public override int RangeXRight => 275;
	public override int RangeY => 432;
}


// Implement

public class iScytheWood : Hook {
	public override WeaponHandheld Handheld => WeaponHandheld.Pole;
}

public class iScytheIron : Hook {
	public override WeaponHandheld Handheld => WeaponHandheld.Pole;
}

public class iScytheGold : Hook {
	public override WeaponHandheld Handheld => WeaponHandheld.Pole;
}

public class iSickle : Hook {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iHookIron : Hook {
	public override WeaponHandheld Handheld => WeaponHandheld.OneOnEachHand;
}

public class iHookGold : Hook {
	public override WeaponHandheld Handheld => WeaponHandheld.OneOnEachHand;
}

public class iHookHand : Hook {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iHookJungle : Hook {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iHookBone : Hook {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iHookJagged : Hook {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iHookTripple : Hook {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iHookBig : Hook {
	public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
}

public class iHookPudge : Hook {
	public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
}

public class iHookChicken : Hook {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

public class iHookRusty : Hook {
	public override WeaponHandheld Handheld => WeaponHandheld.OneOnEachHand;
}
