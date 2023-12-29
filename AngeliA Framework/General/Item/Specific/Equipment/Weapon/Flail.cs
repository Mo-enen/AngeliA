using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	[ItemCombination(typeof(iSpikeBall), typeof(iChain), typeof(iTreeBranch), 1)]
	public class iFlailWood : Flail {}
	[ItemCombination(typeof(iFlailWood), typeof(iIngotIron), 1)]
	public class iFlailIron : Flail { }
	[ItemCombination(typeof(iIngotGold), typeof(iFlailIron), 1)]
	public class iFlailGold : Flail { }
	[ItemCombination(typeof(iFlailWood), typeof(iFlailWood), typeof(iFlailWood), 1)]
	public class iFlailTriple : Flail {
		protected override int HeadCount => 3;
	}
	[ItemCombination(typeof(iEyeBall), typeof(iFlailIron), 1)]
	public class iFlailEye : Flail { }
	[ItemCombination(typeof(iSkull), typeof(iFlailWood), 1)]
	public class iFlailSkull : Flail { }
	[ItemCombination(typeof(iIronHook), typeof(iRope), typeof(iTreeBranch), 1)]
	public class iFishingPole : Flail { }
	[ItemCombination(typeof(iChain), typeof(iMaceSpiked), typeof(iTreeBranch), 1)]
	public class iFlailMace : Flail { }
	[ItemCombination(typeof(iIronHook), typeof(iChain), 1)]
	public class iFlailHook : Flail { }
	[ItemCombination(typeof(iChain), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iNunchaku : Flail {
		public override WeaponHandheld Handheld => WeaponHandheld.OneOnEachHand;
		protected override int ChainLength => Const.CEL * 2 / 9;
		protected override int ChainLengthAttackGrow => 2000;
	}
	[ItemCombination(typeof(iPickWood), typeof(iChain), typeof(iTreeBranch), 1)]
	public class iFlailPick : Flail {
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
		public override int AttackDuration => 24;
	}
	[ItemCombination(typeof(iChain), typeof(iChain), typeof(iMaceSpiked), typeof(iMaceSpiked), 1)]
	public class iChainMace : Flail {
		public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
		protected override int ChainLength => Const.CEL * 2 / 9;
		protected override int ChainLengthAttackGrow => 2000;
		public override int AttackDuration => 24;
	}
	[ItemCombination(typeof(iSpikeBall), typeof(iChain), 1)]
	public class iChainSpikeBall : Flail {
		public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
		protected override int ChainLengthAttackGrow => 2000;
		public override int AttackDuration => 24;
	}
	[ItemCombination(typeof(iBolt), typeof(iBolt), typeof(iBolt), typeof(iChain), 1)]
	public class iChainBarbed : Flail {
		public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
		protected override int ChainLengthAttackGrow => 2000;
		public override int AttackDuration => 24;
	}
	[ItemCombination(typeof(iFist), typeof(iChain), 1)]
	public class iChainFist : Flail {
		public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
		protected override int ChainLengthAttackGrow => 2000;
		public override int AttackDuration => 24;
	}
}
