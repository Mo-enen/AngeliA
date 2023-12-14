using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	[EntityAttribute.ItemCombination(typeof(iBanana), typeof(iTreeBranch), 1)]
	public class iBoomerang : ThrowingWeapon {
		public override Bullet SpawnBullet (Character sender, int bulletID) {
			var bullet = base.SpawnBullet(sender, bulletID);
			if (bullet is MovableBullet mBullet) {
				mBullet.RotateSpeed = sender.FacingSign * 24;
				mBullet.EndRotation = 90;
				mBullet.EndRotationRange = 20;
			}
			return bullet;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iNinjaStar), 2)]
	public class iNinjaStarHalf : ThrowingWeapon {
		public override Bullet SpawnBullet (Character sender, int bulletID) {
			var bullet = base.SpawnBullet(sender, bulletID);
			if (bullet is MovableBullet mBullet) {
				mBullet.RotateSpeed = sender.FacingSign * 24;
				mBullet.CurrentRotation = Util.QuickRandom(Game.GlobalFrame).UMod(360);
				mBullet.EndRotation = 90;
				mBullet.EndRotationRange = 20;
			}
			return bullet;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iStar), typeof(iIngotIron), 1)]
	public class iNinjaStar : ThrowingWeapon {
		public override Bullet SpawnBullet (Character sender, int bulletID) {
			var bullet = base.SpawnBullet(sender, bulletID);
			if (bullet is MovableBullet mBullet) {
				mBullet.RotateSpeed = sender.FacingSign * 24;
				mBullet.CurrentRotation = Util.QuickRandom(Game.GlobalFrame).UMod(360);
			}
			return bullet;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iThrowingKnife), typeof(iIngotIron), 1)]
	public class iKunai : ThrowingWeapon {
		public override Bullet SpawnBullet (Character sender, int bulletID) {
			var bullet = base.SpawnBullet(sender, bulletID);
			if (bullet is MovableBullet mBullet) {
				mBullet.CurrentRotation = sender.FacingRight ? 90 : -90;
				mBullet.EndRotation = 90;
				mBullet.EndRotationRange = 10;
			}
			return bullet;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iBraceletIron), 1)]
	public class iChakram : ThrowingWeapon {
		public override Bullet SpawnBullet (Character sender, int bulletID) {
			var bullet = base.SpawnBullet(sender, bulletID);
			if (bullet is MovableBullet mBullet) {
				mBullet.Velocity = new Vector2Int(sender.FacingSign * 32, 0);
				mBullet.RotateSpeed = sender.FacingSign * 12;
				mBullet.CurrentRotation = Util.QuickRandom(Game.GlobalFrame).UMod(360);
				mBullet._DestroyOnHitReceiver = false;
				mBullet._DestroyOnHitEnvironment = true;
			}
			return bullet;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iFlintPolished), typeof(iIngotIron), 1)]
	public class iThrowingKnife : ThrowingWeapon {
		public override Bullet SpawnBullet (Character sender, int bulletID) {
			var bullet = base.SpawnBullet(sender, bulletID);
			if (bullet is MovableBullet mBullet) {
				mBullet.RotateSpeed = sender.FacingSign * 24;
				mBullet.EndRotation = 90;
				mBullet.EndRotationRange = 20;
			}
			return bullet;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iFlintPolished), typeof(iIngotIron), typeof(iTreeBranch), 1)]
	public class iThrowingAxe : ThrowingWeapon {
		public override Bullet SpawnBullet (Character sender, int bulletID) {
			var bullet = base.SpawnBullet(sender, bulletID);
			if (bullet is MovableBullet mBullet) {
				mBullet.RotateSpeed = sender.FacingSign * 24;
				mBullet.EndRotation = 90;
				mBullet.EndRotationRange = 20;
			}
			return bullet;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iIronWire), typeof(iIronWire), typeof(iIronWire), 1)]
	public class iNeedle : ThrowingWeapon {
		public override Bullet SpawnBullet (Character sender, int bulletID) {
			var bullet = base.SpawnBullet(sender, bulletID);
			if (bullet is MovableBullet mBullet) {
				mBullet.CurrentRotation = sender.FacingRight ? 90 : -90;
				mBullet.EndRotation = 90;
				mBullet.EndRotationRange = 0;
			}
			return bullet;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iChainSpikeBall), typeof(iChainSpikeBall), typeof(iChain), 1)]
	public class iChainMaceBall : ThrowingWeapon {
		public override Bullet SpawnBullet (Character sender, int bulletID) {
			var bullet = base.SpawnBullet(sender, bulletID);
			if (bullet is MovableBullet mBullet) {
				mBullet._DestroyOnHitReceiver = false;
				mBullet._DestroyOnHitEnvironment = true;
				mBullet.RotateSpeed = sender.FacingSign * 24;
				mBullet.CurrentRotation = Util.QuickRandom(Game.GlobalFrame).UMod(360);
				mBullet.EndRotation = 90;
				mBullet.EndRotationRange = 40;
			}
			return bullet;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iBowlingBall), typeof(iGunpowder), 1)]
	public class iBomb : ThrowingWeapon {
		public override Bullet SpawnBullet (Character sender, int bulletID) {
			var bullet = base.SpawnBullet(sender, ExplosiveMovableBullet.TYPE_ID);
			if (bullet is ExplosiveMovableBullet mBullet) {
				mBullet.Velocity = new Vector2Int(sender.FacingSign * 42, 48);
				mBullet.Gravity = 5;
				mBullet.RotateSpeed = sender.FacingSign * 6;
				mBullet.CurrentRotation = 0;
				mBullet._DestroyOnHitEnvironment = true;
				mBullet._DestroyOnHitReceiver = true;
				mBullet._EnvironmentMask = PhysicsMask.SOLID;
				mBullet._ReceiverMask = PhysicsMask.CHARACTER;
				mBullet._Damage = 1;
				mBullet.ExplosionDuration = 10;
				mBullet.Radius = Const.CEL * 3;
			}
			return bullet;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iIronHook), typeof(iIngotIron), typeof(iIngotIron), typeof(iIngotIron), 1)]
	public class iAnchor : ThrowingWeapon {
		public override Bullet SpawnBullet (Character sender, int bulletID) {
			var bullet = base.SpawnBullet(sender, bulletID);
			if (bullet is MovableBullet mBullet) {
				mBullet.CurrentRotation = sender.FacingRight ? 90 : -90;
				mBullet.EndRotation = 90;
				mBullet.EndRotationRange = 5;
			}
			return bullet;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iThrowingAxe), typeof(iIngotGold), 1)]
	public class iCrossAxe : ThrowingWeapon {
		public override Bullet SpawnBullet (Character sender, int bulletID) {
			var bullet = base.SpawnBullet(sender, bulletID);
			if (bullet is MovableBullet mBullet) {
				mBullet.RotateSpeed = sender.FacingSign * 24;
				mBullet.EndRotation = 90;
				mBullet.EndRotationRange = 20;
			}
			return bullet;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iGrapePurple), typeof(iGunpowder), 1)]
	public class iGrapeBomb : ThrowingWeapon {
		public override Bullet SpawnBullet (Character sender, int bulletID) {
			var bullet = base.SpawnBullet(sender, ExplosiveMovableBullet.TYPE_ID);
			if (bullet is ExplosiveMovableBullet mBullet) {
				mBullet.Velocity = new Vector2Int(sender.FacingSign * 42, 48);
				mBullet.Gravity = 5;
				mBullet.RotateSpeed = sender.FacingSign * 6;
				mBullet.CurrentRotation = 0;
				mBullet._DestroyOnHitEnvironment = true;
				mBullet._DestroyOnHitReceiver = true;
				mBullet._EnvironmentMask = PhysicsMask.SOLID;
				mBullet._ReceiverMask = PhysicsMask.CHARACTER;
				mBullet._Damage = 1;
				mBullet.ExplosionDuration = 10;
				mBullet.Radius = Const.CEL * 3;
			}
			return bullet;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iPotionEmpty), typeof(iBlackPepper), typeof(iGunpowder), 1)]
	public class iTearGas : ThrowingWeapon {
		public override Bullet SpawnBullet (Character sender, int bulletID) {
			var bullet = base.SpawnBullet(sender, bulletID);
			if (bullet is MovableBullet mBullet) {
				mBullet.Velocity = new Vector2Int(sender.FacingSign * 42, 48);
				mBullet.Gravity = 5;
				mBullet.RotateSpeed = sender.FacingSign * 6;
				mBullet.CurrentRotation = 0;
			}
			return bullet;
		}
	}


	[EntityAttribute.ItemCombination(typeof(iPotionEmpty), typeof(iGunpowder), 1)]
	public class iGrenade : ThrowingWeapon {
		public override Bullet SpawnBullet (Character sender, int bulletID) {
			var bullet = base.SpawnBullet(sender, ExplosiveMovableBullet.TYPE_ID);
			if (bullet is ExplosiveMovableBullet mBullet) {
				mBullet.Velocity = new Vector2Int(sender.FacingSign * 64, 64);
				mBullet.Gravity = 5;
				mBullet.RotateSpeed = sender.FacingSign * 6;
				mBullet.CurrentRotation = 0;
				mBullet._DestroyOnHitEnvironment = true;
				mBullet._DestroyOnHitReceiver = true;
				mBullet._EnvironmentMask = PhysicsMask.SOLID;
				mBullet._ReceiverMask = PhysicsMask.CHARACTER;
				mBullet._Damage = 1;
				mBullet.ExplosionDuration = 10;
				mBullet.Radius = Const.CEL * 3;
			}
			return bullet;
		}
	}


}
