using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	[EntityAttribute.ItemCombination(typeof(iBanana), typeof(iTreeBranch), 1)]
	public class iBoomerang : ThrowingWeapon<iBoomerang.BoomerangBullet> {
		public class BoomerangBullet : MovableBullet {
			protected override int RotateSpeed => 24;
			protected override int EndRotation => 90;
			protected override int EndRotationRandomRange => 20;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iNinjaStar), 2)]
	public class iNinjaStarHalf : ThrowingWeapon<iNinjaStarHalf.NinjaStarHalfBullet> {
		public class NinjaStarHalfBullet : MovableBullet {
			protected override int RotateSpeed => 24;
			protected override int StartRotation => Util.QuickRandom(Game.GlobalFrame).UMod(360);
			protected override int EndRotation => 90;
			protected override int EndRotationRandomRange => 20;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iStar), typeof(iIngotIron), 1)]
	public class iNinjaStar : ThrowingWeapon<iNinjaStar.NinjaStarBullet> {
		public class NinjaStarBullet : MovableBullet {
			protected override int RotateSpeed => 24;
			protected override int StartRotation => Util.QuickRandom(Game.GlobalFrame).UMod(360);
		}
	}


	[EntityAttribute.ItemCombination(typeof(iThrowingKnife), typeof(iIngotIron), 1)]
	public class iKunai : ThrowingWeapon<iKunai.KunaiBullet> {
		public class KunaiBullet : MovableBullet {
			protected override int RotateSpeed => 24;
			protected override int StartRotation => 90;
			protected override int EndRotation => 90;
			protected override int EndRotationRandomRange => 10;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iBraceletIron), 1)]
	public class iChakram : ThrowingWeapon<iChakram.ChakramBullet> {
		public class ChakramBullet : MovableBullet {
			protected override int SpeedX => 32;
			protected override int RotateSpeed => 12;
			protected override int StartRotation => Util.QuickRandom(Game.GlobalFrame).UMod(360);
			protected override bool DestroyOnHitEnvironment => true;
			protected override bool DestroyOnHitReceiver => false;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iFlintPolished), typeof(iIngotIron), 1)]
	public class iThrowingKnife : ThrowingWeapon<iThrowingKnife.ThrowingKnifeBullet> {
		public class ThrowingKnifeBullet : MovableBullet {
			protected override int RotateSpeed => 24;
			protected override int EndRotation => 90;
			protected override int EndRotationRandomRange => 20;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iFlintPolished), typeof(iIngotIron), typeof(iTreeBranch), 1)]
	public class iThrowingAxe : ThrowingWeapon<iThrowingAxe.ThrowingAxeBullet> {
		public class ThrowingAxeBullet : MovableBullet {
			protected override int RotateSpeed => 24;
			protected override int EndRotation => 90;
			protected override int EndRotationRandomRange => 20;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iIronWire), typeof(iIronWire), typeof(iIronWire), 1)]
	public class iNeedle : ThrowingWeapon<iNeedle.NeedleBullet> {
		public class NeedleBullet : MovableBullet {
			protected override int StartRotation => 90;
			protected override int EndRotation => 90;
			protected override int EndRotationRandomRange => 0;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iChainSpikeBall), typeof(iChainSpikeBall), typeof(iChain), 1)]
	public class iChainMaceBall : ThrowingWeapon<iChainMaceBall.ChainMaceBallBullet> {
		public class ChainMaceBallBullet : MovableBullet {
			protected override bool DestroyOnHitReceiver => false;
			protected override bool DestroyOnHitEnvironment => true;
			protected override int RotateSpeed => 24;
			protected override int StartRotation => Util.QuickRandom(Game.GlobalFrame).UMod(360);
			protected override int EndRotation => 90;
			protected override int EndRotationRandomRange => 40;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iBowlingBall), typeof(iGunpowder), 1)]
	public class iBomb : ThrowingWeapon<iBomb.BombBullet> {
		public class BombBullet : ExplosiveMovableBullet {
			protected override int SpeedX => 42;
			protected override int SpeedY => 48;
			protected override int Gravity => 5;
			protected override int RotateSpeed => 6;
			protected override int StartRotation => 0;
			protected override bool DestroyOnHitEnvironment => true;
			protected override bool DestroyOnHitReceiver => true;
			protected override int EnvironmentMask => PhysicsMask.SOLID;
			protected override int ReceiverMask => PhysicsMask.CHARACTER;
			protected override int Damage => 1;
			protected override int ExplosionDuration => 10;
			protected override int Radius => Const.CEL * 3;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iIronHook), typeof(iIngotIron), typeof(iIngotIron), typeof(iIngotIron), 1)]
	public class iAnchor : ThrowingWeapon<iAnchor.AnchorBullet> {
		public class AnchorBullet : MovableBullet {
			protected override int StartRotation => 90;
			protected override int EndRotation => 90;
			protected override int EndRotationRandomRange => 5;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iThrowingAxe), typeof(iIngotGold), 1)]
	public class iCrossAxe : ThrowingWeapon<iCrossAxe.CrossAxeBullet> {
		public class CrossAxeBullet : MovableBullet {
			protected override int RotateSpeed => 24;
			protected override int EndRotation => 90;
			protected override int EndRotationRandomRange => 20;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iGrapePurple), typeof(iGunpowder), 1)]
	public class iGrapeBomb : ThrowingWeapon<iGrapeBomb.GrapeBombBullet> {
		public class GrapeBombBullet : ExplosiveMovableBullet {
			protected override int SpeedX => 42;
			protected override int SpeedY => 48;
			protected override int Gravity => 5;
			protected override int RotateSpeed => 6;
			protected override int StartRotation => 0;
			protected override bool DestroyOnHitEnvironment => true;
			protected override bool DestroyOnHitReceiver => true;
			protected override int EnvironmentMask => PhysicsMask.SOLID;
			protected override int ReceiverMask => PhysicsMask.CHARACTER;
			protected override int Damage => 1;
			protected override int ExplosionDuration => 10;
			protected override int Radius => Const.CEL * 3;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iPotionEmpty), typeof(iBlackPepper), typeof(iGunpowder), 1)]
	public class iTearGas : ThrowingWeapon<iTearGas.TearGasBullet> {
		public class TearGasBullet : ExplosiveMovableBullet {
			protected override int SpeedX => 64;
			protected override int SpeedY => 64;
			protected override int Gravity => 5;
			protected override int RotateSpeed => 6;
			protected override int StartRotation => 0;
			protected override bool DestroyOnHitEnvironment => true;
			protected override bool DestroyOnHitReceiver => true;
			protected override int EnvironmentMask => PhysicsMask.SOLID;
			protected override int ReceiverMask => PhysicsMask.CHARACTER;
			protected override int Damage => 1;
			protected override int ExplosionDuration => 10;
			protected override int Radius => Const.CEL * 3;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iPotionEmpty), typeof(iGunpowder), 1)]
	public class iGrenade : ThrowingWeapon<iGrenade.GrenadeBullet> {
		public class GrenadeBullet : ExplosiveMovableBullet {
			protected override int SpeedX => 64;
			protected override int SpeedY => 64;
			protected override int Gravity => 5;
			protected override int RotateSpeed => 6;
			protected override int StartRotation => 0;
			protected override bool DestroyOnHitEnvironment => true;
			protected override bool DestroyOnHitReceiver => true;
			protected override int EnvironmentMask => PhysicsMask.SOLID;
			protected override int ReceiverMask => PhysicsMask.CHARACTER;
			protected override int Damage => 1;
			protected override int ExplosionDuration => 10;
			protected override int Radius => Const.CEL * 3;
		}
	}


}
