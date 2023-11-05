using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class iFlailWood : AutoSpriteFlail {}
	public class iFlailIron : AutoSpriteFlail { }
	public class iFlailGold : AutoSpriteFlail { }
	public class iFlailTriple : AutoSpriteFlail {
		protected override int HeadCount => 3;
	}
	public class iFlailEye : AutoSpriteFlail { }
	public class iFlailSkull : AutoSpriteFlail { }
	public class iFishingPole : AutoSpriteFlail { }
	public class iFlailMace : AutoSpriteFlail { }
	public class iFlailHook : AutoSpriteFlail { }
	public class iNunchaku : AutoSpriteFlail {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
		protected override int ChainLength => Const.CEL * 2 / 9;
		protected override int ChainLengthAttackGrow => 2000;
	}
	public class iFlailPick : AutoSpriteFlail {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
		public override int AttackDuration => 24;
	}
	public class iChainMace : AutoSpriteFlail {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		protected override int ChainLength => Const.CEL * 2 / 9;
		protected override int ChainLengthAttackGrow => 2000;
		public override int AttackDuration => 24;
	}
	public class iChainSpikeBall : AutoSpriteFlail {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		protected override int ChainLengthAttackGrow => 2000;
		public override int AttackDuration => 24;
	}
	public class iChainBarbed : AutoSpriteFlail {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		protected override int ChainLengthAttackGrow => 2000;
		public override int AttackDuration => 24;
	}
	public class iChainFist : AutoSpriteFlail {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		protected override int ChainLengthAttackGrow => 2000;
		public override int AttackDuration => 24;
	}
}
