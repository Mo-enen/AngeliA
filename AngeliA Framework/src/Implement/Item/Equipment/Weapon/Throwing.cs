using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


// Throwing
public abstract class ThrowingWeapon<B> : ProjectileWeapon<B> where B : MovableBullet {
	public sealed override WeaponType WeaponType => WeaponType.Throwing;
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}


// Implement
[ItemCombination(typeof(iBanana), typeof(iTreeBranch), 1)]
public class iBoomerang : ThrowingWeapon<iBoomerang.BoomerangBullet> {
	public class BoomerangBullet : MovableBullet {
		public override int RotateSpeed => 24;
		public override int EndRotation => 90;
		public override int EndRotationRandomRange => 20;
	}
}


[ItemCombination(typeof(iNinjaStar), 2)]
public class iNinjaStarHalf : ThrowingWeapon<iNinjaStarHalf.NinjaStarHalfBullet> {
	public class NinjaStarHalfBullet : MovableBullet {
		public override int RotateSpeed => 24;
		public override int StartRotation => Util.QuickRandom(Game.GlobalFrame).UMod(360);
		public override int EndRotation => 90;
		public override int EndRotationRandomRange => 20;
	}
}


[ItemCombination(typeof(iStar), typeof(iIngotIron), 1)]
public class iNinjaStar : ThrowingWeapon<iNinjaStar.NinjaStarBullet> {
	public class NinjaStarBullet : MovableBullet {
		public override int RotateSpeed => 24;
		public override int StartRotation => Util.QuickRandom(Game.GlobalFrame).UMod(360);
	}
}


[ItemCombination(typeof(iThrowingKnife), typeof(iIngotIron), 1)]
public class iKunai : ThrowingWeapon<iKunai.KunaiBullet> {
	public class KunaiBullet : MovableBullet {
		public override int RotateSpeed => 24;
		public override int StartRotation => 90;
		public override int EndRotation => 90;
		public override int EndRotationRandomRange => 10;
	}
}


[ItemCombination(typeof(iAxeWood), typeof(iBraceletIron), 1)]
public class iChakram : ThrowingWeapon<iChakram.ChakramBullet> {
	public class ChakramBullet : MovableBullet {
		public override int SpeedX => 32;
		public override int RotateSpeed => 12;
		public override int StartRotation => Util.QuickRandom(Game.GlobalFrame).UMod(360);
		protected override bool DestroyOnHitEnvironment => true;
		protected override bool DestroyOnHitReceiver => false;
	}
}


[ItemCombination(typeof(iFlintPolished), typeof(iIngotIron), 1)]
public class iThrowingKnife : ThrowingWeapon<iThrowingKnife.ThrowingKnifeBullet> {
	public class ThrowingKnifeBullet : MovableBullet {
		public override int RotateSpeed => 24;
		public override int EndRotation => 90;
		public override int EndRotationRandomRange => 20;
	}
}


[ItemCombination(typeof(iFlintPolished), typeof(iIngotIron), typeof(iTreeBranch), 1)]
public class iThrowingAxe : ThrowingWeapon<iThrowingAxe.ThrowingAxeBullet> {
	public class ThrowingAxeBullet : MovableBullet {
		public override int RotateSpeed => 24;
		public override int EndRotation => 90;
		public override int EndRotationRandomRange => 20;
	}
}


[ItemCombination(typeof(iIronWire), typeof(iIronWire), typeof(iIronWire), 1)]
public class iNeedle : ThrowingWeapon<iNeedle.NeedleBullet> {
	public class NeedleBullet : MovableBullet {
		public override int StartRotation => 90;
		public override int EndRotation => 90;
		public override int EndRotationRandomRange => 0;
	}
}


[ItemCombination(typeof(iChainSpikeBall), typeof(iChainSpikeBall), typeof(iChain), 1)]
public class iChainMaceBall : ThrowingWeapon<iChainMaceBall.ChainMaceBallBullet> {
	public class ChainMaceBallBullet : MovableBullet {
		protected override bool DestroyOnHitReceiver => false;
		protected override bool DestroyOnHitEnvironment => true;
		public override int RotateSpeed => 24;
		public override int StartRotation => Util.QuickRandom(Game.GlobalFrame).UMod(360);
		public override int EndRotation => 90;
		public override int EndRotationRandomRange => 40;
	}
}


[ItemCombination(typeof(iBowlingBall), typeof(iGunpowder), 1)]
public class iBomb : ThrowingWeapon<iBomb.BombBullet> {
	public class BombBullet : ExplosiveMovableBullet {
		public override int SpeedX => 42;
		public override int SpeedY => 48;
		public override int Gravity => 5;
		public override int RotateSpeed => 6;
		public override int StartRotation => 0;
		protected override bool DestroyOnHitEnvironment => true;
		protected override bool DestroyOnHitReceiver => true;
		protected override int EnvironmentMask => PhysicsMask.SOLID;
		protected override int ReceiverMask => PhysicsMask.CHARACTER;
		protected override int Damage => 1;
		protected override int ExplosionDuration => 10;
		protected override int Radius => Const.CEL * 3;
	}
}


[ItemCombination(typeof(iIronHook), typeof(iIngotIron), typeof(iIngotIron), typeof(iIngotIron), 1)]
public class iAnchor : ThrowingWeapon<iAnchor.AnchorBullet> {
	public class AnchorBullet : MovableBullet {
		public override int StartRotation => 90;
		public override int EndRotation => 90;
		public override int EndRotationRandomRange => 5;
	}
}


[ItemCombination(typeof(iThrowingAxe), typeof(iIngotGold), 1)]
public class iCrossAxe : ThrowingWeapon<iCrossAxe.CrossAxeBullet> {
	public class CrossAxeBullet : MovableBullet {
		public override int RotateSpeed => 24;
		public override int EndRotation => 90;
		public override int EndRotationRandomRange => 20;
	}
}


[ItemCombination(typeof(iGrapePurple), typeof(iGunpowder), 1)]
public class iGrapeBomb : ThrowingWeapon<iGrapeBomb.GrapeBombBullet> {
	public class GrapeBombBullet : ExplosiveMovableBullet {
		public override int SpeedX => 42;
		public override int SpeedY => 48;
		public override int Gravity => 5;
		public override int RotateSpeed => 6;
		public override int StartRotation => 0;
		protected override bool DestroyOnHitEnvironment => true;
		protected override bool DestroyOnHitReceiver => true;
		protected override int EnvironmentMask => PhysicsMask.SOLID;
		protected override int ReceiverMask => PhysicsMask.CHARACTER;
		protected override int Damage => 1;
		protected override int ExplosionDuration => 10;
		protected override int Radius => Const.CEL * 3;
	}
}


[ItemCombination(typeof(iPotionEmpty), typeof(iBlackPepper), typeof(iGunpowder), 1)]
public class iTearGas : ThrowingWeapon<iTearGas.TearGasBullet> {
	public class TearGasBullet : ExplosiveMovableBullet {
		public override int SpeedX => 64;
		public override int SpeedY => 64;
		public override int Gravity => 5;
		public override int RotateSpeed => 6;
		public override int StartRotation => 0;
		protected override bool DestroyOnHitEnvironment => true;
		protected override bool DestroyOnHitReceiver => true;
		protected override int EnvironmentMask => PhysicsMask.SOLID;
		protected override int ReceiverMask => PhysicsMask.CHARACTER;
		protected override int Damage => 1;
		protected override int ExplosionDuration => 10;
		protected override int Radius => Const.CEL * 3;
	}
}


[ItemCombination(typeof(iPotionEmpty), typeof(iGunpowder), 1)]
public class iGrenade : ThrowingWeapon<iGrenade.GrenadeBullet> {
	public class GrenadeBullet : ExplosiveMovableBullet {
		public override int SpeedX => 64;
		public override int SpeedY => 64;
		public override int Gravity => 5;
		public override int RotateSpeed => 6;
		public override int StartRotation => 0;
		protected override bool DestroyOnHitEnvironment => true;
		protected override bool DestroyOnHitReceiver => true;
		protected override int EnvironmentMask => PhysicsMask.SOLID;
		protected override int ReceiverMask => PhysicsMask.CHARACTER;
		protected override int Damage => 1;
		protected override int ExplosionDuration => 10;
		protected override int Radius => Const.CEL * 3;
	}
}
