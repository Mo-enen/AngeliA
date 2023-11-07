using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.ItemCombination(typeof(iSpikeBall), typeof(iChain), typeof(iTreeBranch), 1)]
	public class iFlailWood : AutoSpriteFlail {}
	[EntityAttribute.ItemCombination(typeof(iFlailWood), typeof(iIngotIron), 1)]
	public class iFlailIron : AutoSpriteFlail { }
	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iFlailIron), 1)]
	public class iFlailGold : AutoSpriteFlail { }
	[EntityAttribute.ItemCombination(typeof(iFlailWood), typeof(iFlailWood), typeof(iFlailWood), 1)]
	public class iFlailTriple : AutoSpriteFlail {
		protected override int HeadCount => 3;
	}
	[EntityAttribute.ItemCombination(typeof(iEyeBall), typeof(iFlailIron), 1)]
	public class iFlailEye : AutoSpriteFlail { }
	[EntityAttribute.ItemCombination(typeof(iSkull), typeof(iFlailWood), 1)]
	public class iFlailSkull : AutoSpriteFlail { }
	[EntityAttribute.ItemCombination(typeof(iIronHook), typeof(iRope), typeof(iTreeBranch), 1)]
	public class iFishingPole : AutoSpriteFlail { }
	[EntityAttribute.ItemCombination(typeof(iChain), typeof(iMaceSpiked), typeof(iTreeBranch), 1)]
	public class iFlailMace : AutoSpriteFlail { }
	[EntityAttribute.ItemCombination(typeof(iIronHook), typeof(iChain), 1)]
	public class iFlailHook : AutoSpriteFlail { }
	[EntityAttribute.ItemCombination(typeof(iChain), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iNunchaku : AutoSpriteFlail {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
		protected override int ChainLength => Const.CEL * 2 / 9;
		protected override int ChainLengthAttackGrow => 2000;
	}
	[EntityAttribute.ItemCombination(typeof(iPickWood), typeof(iChain), typeof(iTreeBranch), 1)]
	public class iFlailPick : AutoSpriteFlail {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
		public override int AttackDuration => 24;
	}
	[EntityAttribute.ItemCombination(typeof(iChain), typeof(iChain), typeof(iMaceSpiked), typeof(iMaceSpiked), 1)]
	public class iChainMace : AutoSpriteFlail {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		protected override int ChainLength => Const.CEL * 2 / 9;
		protected override int ChainLengthAttackGrow => 2000;
		public override int AttackDuration => 24;
	}
	[EntityAttribute.ItemCombination(typeof(iSpikeBall), typeof(iChain), 1)]
	public class iChainSpikeBall : AutoSpriteFlail {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		protected override int ChainLengthAttackGrow => 2000;
		public override int AttackDuration => 24;
	}
	[EntityAttribute.ItemCombination(typeof(iBolt), typeof(iBolt), typeof(iBolt), typeof(iChain), 1)]
	public class iChainBarbed : AutoSpriteFlail {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		protected override int ChainLengthAttackGrow => 2000;
		public override int AttackDuration => 24;
	}
	[EntityAttribute.ItemCombination(typeof(iFist), typeof(iChain), 1)]
	public class iChainFist : AutoSpriteFlail {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		protected override int ChainLengthAttackGrow => 2000;
		public override int AttackDuration => 24;
	}
}
