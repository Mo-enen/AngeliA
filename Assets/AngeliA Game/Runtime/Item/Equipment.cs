using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {

	// Helmet
	public class iHelmetWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	public class iHelmetIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	public class iHelmetGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	public class iSafetyHelmet : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	public class iPirateHat : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	public class iWizardHat : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	public class iTopHat : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	public class iFoxMask : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	public class iCirclet : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	public class iHelmetFull : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	public class iCrown : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	public class iGasMask : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	public class iHelmetViking : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	public class iHelmetKnight : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}
	public class iHelmetBandit : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Helmet;
	}

	// Armor
	public class iArmorWood : Equipment {
		private static readonly int BROKEN_CODE = typeof(iArmorWoodBroken).AngeHash();
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
		public override void OnTakeDamage (Entity holder, ItemLocation location, ref int damage, Entity sender) {
			base.OnTakeDamage(holder, location, ref damage, sender);
			Inventory.SetEquipment(holder.TypeID, EquipmentType, BROKEN_CODE);
			SpawnEquipmentBrokeParticle(TypeID, holder.X, holder.Y);
			damage--;
		}
	}
	public class iArmorWoodBroken : Equipment {
		private static readonly int FIX_CODE = typeof(iArmorWood).AngeHash();
		private static readonly int MAT_CODE_0 = typeof(iTreeStump).AngeHash();
		private static readonly int MAT_CODE_1 = typeof(iItemWoodBoard).AngeHash();
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
		public override void OnSquat (Entity holder, ItemLocation location) {
			base.OnSquat(holder, location);
			// Try 0
			int tookCount = Inventory.FindAndTakeItem(holder.TypeID, MAT_CODE_0, 1);
			if (tookCount > 0) {
				Inventory.SetEquipment(holder.TypeID, EquipmentType, FIX_CODE);
				SpawnItemLostParticle(MAT_CODE_0, holder.X, holder.Y);
				return;
			}
			// Try 1
			tookCount = Inventory.FindAndTakeItem(holder.TypeID, MAT_CODE_1, 1);
			if (tookCount > 0) {
				Inventory.SetEquipment(holder.TypeID, EquipmentType, FIX_CODE);
				SpawnItemLostParticle(MAT_CODE_1, holder.X, holder.Y);
				return;
			}
		}
	}
	public class iArmorIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorBrave : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorSkull : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iChainMail : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorClay : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iVelvetDress : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iCloak : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorKnight : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iMageRobe : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorLeather : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorStudded : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iPractitionerRobe : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}
	public class iArmorPaladin : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.BodySuit;
	}

	// Gloves
	public class iGlovesWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	public class iGlovesIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	public class iGlovesGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	public class iGlovesSki : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	public class iGlovesMachine : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	public class iGlovesGem : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	public class iGlovesIce : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	public class iGlovesFire : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	public class iGlovesVelvet : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	public class iGlovesOrc : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	public class iGlovesBoxing : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	public class iGlovesOven : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	public class iGlovesPaladin : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	public class iGlovesFairy : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}
	public class iGlovesMage : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Gloves;
	}

	// Shoes
	public class iShoesWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	public class iShoesIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	public class iShoesGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	public class iShoesSki : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	public class iShoesWing : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	public class iShoesFairy : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	public class iShoesSand : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	public class iShoesVelvet : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	public class iShoesMage : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	public class iShoesKnight : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	public class iShoesHiking : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	public class iWoodenClogs : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	public class iShoesPaladin : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	public class iShoesStudded : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}
	public class iShoesSpike : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Shoes;
	}

	// Jewelry
	public class iNecklaceWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Jewelry;
	}
	public class iNecklaceIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Jewelry;
	}
	public class iNecklaceGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Jewelry;
	}
	public class iBraceletGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Jewelry;
	}
	public class iBraceletIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Jewelry;
	}
	public class iMagatama : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Jewelry;
	}
	public class iPendant : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Jewelry;
	}
	public class iPrayerBeads : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Jewelry;
	}
	public class iPrayerBeadsGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Jewelry;
	}
	public class iAmber : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Jewelry;
	}
	public class iRingGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Jewelry;
	}
	public class iRubyJewelRed : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Jewelry;
	}
	public class iRubyJewelBlue : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Jewelry;
	}
	public class iRubyJewelYellow : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Jewelry;
	}
	public class iRubyJewelGreen : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Jewelry;
	}

	// Sword
	public class iSwordWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iSwordIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iSwordGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iDagger : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iSwordCrimson : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iSwordScarlet : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iScimitar : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iSwordPirate : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iSwordAgile : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iScimitarAgile : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iSwordJagged : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iSwordGreat : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iSwordDark : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iSwordCrutch : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iKnifeGiant : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}

	// Axe
	public class iAxeWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iAxeIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iAxeGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBattleAxe : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iErgonomicAxe : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iAxeJagged : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iAxeOrc : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iAxeCursed : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iPickWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iPickIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iPickGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iAxeGreat : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iAxeButterfly : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iAxeBone : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iAxeStone : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}

	// Hammer
	public class iHammerWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHammerIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHammerGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iMaceRound : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iMaceSkull : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBaseballBatWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iMaceSpiked : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBian : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHammerRiceCake : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHammerGoatHorn : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBaseballBatIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHammerThunder : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHammerMoai : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHammerPaladin : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHammerRuby : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}

	// Flail
	public class iFlailWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iFlailIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iFlailGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iFlailTriple : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iFlailEye : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iFlailSkull : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iFishingPole : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iFlailMace : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iFlailHook : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iNunchaku : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iFlailPick : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iChainMace : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iChainSpikeBall : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iChainBarbed : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iChainFist : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}

	// Bow
	public class iBowWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBowIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBowGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iCrossbowWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iCrossbowIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iCrossbowGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBlowgun : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iSlingshot : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iCompoundBow : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iRepeatingCrossbow : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBowNature : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBowSkull : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBowMage : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBowSky : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBowHarp : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}

	// Spear
	public class iSpearWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iSpearIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iSpearGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iTrident : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBoStaffWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBoStaffIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBoStaffGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iNaginata : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHalberd : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iJi : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iMonkSpade : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iManCatcher : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iSwallow : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iFork : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBrandistock : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}

	// Hook
	public class iScytheWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iScytheIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iScytheGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iSickle : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHookIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHookGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHookHand : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHookJungle : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHookBone : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHookJagged : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHookTripple : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHookBig : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHookPudge : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHookChicken : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iHookRusty : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}

	// Claw
	public class iClawWood : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iClawIron : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iClawGold : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iMandarinDuckAxe : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iClawCat : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iClawFox : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iKatars : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iKatarsTripple : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iEmeiPiercer : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBaton : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iKnuckleDuster : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iEmeiFork : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iWuXingHook : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iKatarsRuby : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iKatarsJagged : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}

	// Wand
	public class iWand : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iTheAncientOne : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iStaffFire : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iStaffWater : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iStaffLightning : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iStaffPoision : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iRitualSkull : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBambooSlips : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iRitualRuneCube : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iGoblinTrophy : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iMagicOrb : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iMagicEyeball : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iMagicPotion : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iWandStar : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iWandFairy : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}

	// Throwing
	public class iBoomerang : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iNinjaStarHalf : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iNinjaStar : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iKunai : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iChakram : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iThrowingKnife : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iThrowingAxe : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iNeedle : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iChainMaceBall : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iBomb : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iAnchor : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iCrossAxe : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iGrapeBomb : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iTearGas : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}
	public class iGrenade : Equipment {
		public override EquipmentType EquipmentType => EquipmentType.Weapon;
	}


}