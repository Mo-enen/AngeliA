using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 


// Axe
public abstract class Axe<B> : Axe where B : MeleeBullet {
	public Axe () => BulletID = typeof(B).AngeHash();
}
public abstract class Axe : MeleeWeapon {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	public sealed override WeaponType WeaponType => WeaponType.Axe;
	public override int AttackDuration => 12;
	public override int AttackCooldown => 2;
	public override int RangeXLeft => 275;
	public override int RangeXRight => 384;
	public override int RangeY => 512;
}


// Implement

public class iAxeWood : Axe { }

public class iAxeIron : Axe { }

public class iAxeGold : Axe { }

public class iBattleAxe : Axe { }

public class iErgonomicAxe : Axe { }

public class iAxeJagged : Axe { }

public class iAxeOrc : Axe { }

public class iAxeCursed : Axe {
	protected override Cell DrawWeaponSprite (PoseCharacter character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
		var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
		FrameworkUtil.DrawGlitchEffect(cell, Game.GlobalFrame);
		return cell;
	}
}

public class iPickWood : Axe { }

public class iPickIron : Axe { }

public class iPickGold : Axe { }

public class iAxeGreat : Axe {
	public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
	public override int AttackDuration => 16;
	public override int AttackCooldown => 3;
}

public class iAxeButterfly : Axe { }

public class iAxeBone : Axe { }

public class iAxeStone : Axe { }
