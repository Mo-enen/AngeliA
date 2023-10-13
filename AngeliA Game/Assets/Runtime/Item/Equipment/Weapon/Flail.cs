using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class iFlailWood : AutoSpriteFlail {
		public override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iFlailIron : AutoSpriteFlail {
		public override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iFlailGold : AutoSpriteFlail {
		public override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iFlailTriple : AutoSpriteFlail {
		public override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
		protected override int HeadCount => 3;
	}
	public class iFlailEye : AutoSpriteFlail {
		public override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iFlailSkull : AutoSpriteFlail {
		public override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iFishingPole : AutoSpriteFlail {
		public override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iFlailMace : AutoSpriteFlail {
		public override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iFlailHook : AutoSpriteFlail {
		public override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iNunchaku : AutoSpriteFlail {
		public override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
		protected override int ChainLength => Const.CEL * 2 / 9;
		protected override int ChainLengthAttackGrow => 2000;
	}
	public class iFlailPick : AutoSpriteFlail {
		public override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Polearm;
	}
	public class iChainMace : AutoSpriteFlail {
		public override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		protected override int ChainLength => Const.CEL * 2 / 9;
		protected override int ChainLengthAttackGrow => 2000;
	}
	public class iChainSpikeBall : AutoSpriteFlail {
		public override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		protected override int ChainLengthAttackGrow => 2000;
	}
	public class iChainBarbed : AutoSpriteFlail {
		public override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		protected override int ChainLengthAttackGrow => 2000;
	}
	public class iChainFist : AutoSpriteFlail {
		public override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		protected override int ChainLengthAttackGrow => 2000;
	}
}
