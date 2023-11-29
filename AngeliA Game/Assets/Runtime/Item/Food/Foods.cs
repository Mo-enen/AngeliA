using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.ItemCombination(typeof(iBeefRaw), 4)]
	[EntityAttribute.ItemCombination(typeof(iPorkRaw), 3)]
	public class iMeatballRaw : Food { }

	[EntityAttribute.ItemCombination(typeof(iMeatballRaw), typeof(iMeatballRaw), typeof(iMeatballRaw), typeof(iTreeBranch), 1)]
	public class iMeatSkewersRaw : Food { }

	[EntityAttribute.ItemCombination(typeof(iMeatballCooked), typeof(iMeatballCooked), typeof(iMeatballCooked), typeof(iTreeBranch), 1)]
	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iMeatSkewersRaw), 1)]
	public class iMeatSkewersCooked : Food { }

	[EntityAttribute.ItemCombination(typeof(iBeefRaw), typeof(iRuneFire), 1)]
	public class iBeefCooked : Food { }

	[EntityAttribute.ItemCombination(typeof(iChickenLegRaw), typeof(iRuneFire), 1)]
	public class iChickenLegCooked : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iChickenWingRaw), 1)]
	public class iChickenWingCooked : Food { }

	[EntityAttribute.ItemCombination(typeof(iPorkRaw), typeof(iRuneFire), 1)]
	public class iPorkCooked : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iMeatballRaw), 1)]
	public class iMeatballCooked : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iSausageRaw), 1)]
	public class iSausageCooked : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iTurkeyRaw), 1)]
	public class iTurkeyCooked : Food { }

	[EntityAttribute.ItemCombination(typeof(iFishRaw), typeof(iRuneFire), 1)]
	public class iFishCooked : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iPterophyllum), 1)]
	public class iPterophyllumCooked : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iFlounder), 1)]
	public class iFlounderCooked : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iSquidRaw), 1)]
	public class iSquidCooked : Food { }

	[EntityAttribute.ItemCombination(typeof(iLobsterRaw), typeof(iRuneFire), 1)]
	public class iLobsterCooked : Food { }

	[EntityAttribute.ItemCombination(typeof(iCrabRaw), typeof(iRuneFire), 1)]
	public class iCrabCooked : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iSnailRaw), 1)]
	public class iSnailCooked : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iHorn), typeof(iCream), typeof(iFlour), 1)]
	public class iCreamHorn : Food { }

	[EntityAttribute.ItemCombination(typeof(iHorn), typeof(iBread), typeof(iCream), 1)]
	public class iCroissant : Food { }

	[EntityAttribute.ItemCombination(typeof(iSausageCooked), typeof(iBread), typeof(iKetchup), typeof(iButter), 1)]
	public class iHotdog : Food { }

	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iBread), 1)]
	public class iBreadKnot : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iSalt), typeof(iFlour), 1)]
	public class iLyeRoll : Food { }

	[EntityAttribute.ItemCombination(typeof(iCream), typeof(iCakeEmbryo), 1)]
	public class iCakeCream : Food { }

	[EntityAttribute.ItemCombination(typeof(iCream), typeof(iCream), typeof(iCakeEmbryo), 1)]
	public class iCakeRoll : Food { }

	[EntityAttribute.ItemCombination(typeof(iPorkCooked), typeof(iBread), typeof(iBread), typeof(iCollardCut), 1)]
	[EntityAttribute.ItemCombination(typeof(iPorkCooked), typeof(iBread), typeof(iBread), typeof(iCabbageCut), 1)]
	[EntityAttribute.ItemCombination(typeof(iChineseCabbageCut), typeof(iPorkCooked), typeof(iBread), typeof(iBread), 1)]
	public class iHamburger : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iFlour), typeof(iFlour), 1)]
	public class iBread : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iCream), typeof(iFlour), 1)]
	public class iDonut : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iPotatoCut), 1)]
	public class iFries : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iFlour), typeof(iMeatballRaw), 1)]
	public class iBaoZi : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iPorkRaw), typeof(iRuneFire), typeof(iFlour), 1)]
	public class iDumpling : Food { }

	[EntityAttribute.ItemCombination(typeof(iSausageCooked), typeof(iCucumberCut), typeof(iRice), typeof(iNori), 1)]
	public class iSushi : Food { }

	[EntityAttribute.ItemCombination(typeof(iMilk), typeof(iMilk), typeof(iSalt), 1)]
	public class iCheese : Food { }

	[EntityAttribute.ItemCombination(typeof(iRice), typeof(iNori), 1)]
	public class iOnigiri : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iFlour), typeof(iChocolate), 1)]
	public class iCookie : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iSausageRaw), typeof(iCheese), typeof(iPancake), 1)]
	public class iPizza : Food { }

	[EntityAttribute.ItemCombination(typeof(iCream), typeof(iCookie), 1)]
	public class iSandwichBiscuit : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iPotatoCut), typeof(iPotatoCut), 1)]
	public class iPotatoChips : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iEgg), typeof(iEgg), typeof(iFlour), 1)]
	public class iEggTart : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iCucumberCut), typeof(iRuneFire), typeof(iPumpkinCut), 1)]
	public class iSoup : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iFlour), typeof(iFlour), 1)]
	public class iPancake : Food { }

	[EntityAttribute.ItemCombination(typeof(iOrangeJuice), typeof(iMilk), typeof(iFlour), typeof(iCornCut), 1)]
	public class iPudding : Food { }

	[EntityAttribute.ItemCombination(typeof(iMilk), typeof(iCream), 1)]
	public class iIceCream : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iEgg), typeof(iFlour), typeof(iFlour), 1)]
	public class iToast : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iFlour), typeof(iFlour), typeof(iFlour), 1)]
	public class iNoodle : Food { }

	[EntityAttribute.ItemCombination(typeof(iSalt), typeof(iTomatoCut), typeof(iTomatoCut), 1)]
	public class iKetchup : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iOnionCut), 1)]
	public class iOnionRing : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iCornCut), 1)]
	public class iPopcorn : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iEgg), 1)]
	public class iFriedEgg : Food { }

	[EntityAttribute.ItemCombination(typeof(iCakeEmbryo), typeof(iChocolate), 1)]
	public class iChocolateMousse : Food { }

	[EntityAttribute.ItemCombination(typeof(iCakeEmbryo), typeof(iCherryCut), 1)]
	public class iCherryMousse : Food { }

	[EntityAttribute.ItemCombination(typeof(iPorkCooked), typeof(iRuneFire), 1)]
	public class iBacon : Food { }

	[EntityAttribute.ItemCombination(typeof(iBlackPepper), typeof(iSalt), typeof(iNoodle), 1)]
	public class iInstantNoodle : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iSalt), typeof(iGreenBeansCut), 1)]
	public class iTofu : Food { }

	[EntityAttribute.ItemCombination(typeof(iBeefCooked), typeof(iRuneFire), 1)]
	public class iDriedBeef : Food { }

	[EntityAttribute.ItemCombination(typeof(iMilk), typeof(iSalt), 1)]
	public class iButter : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iTentacle), typeof(iSalt), 1)]
	public class iTakoyaki : Food { }

	[EntityAttribute.ItemCombination(typeof(iBeefCooked), typeof(iChili), typeof(iPancake), typeof(iSpinachCut), 1)]
	public class iArepa : Food { }

	[EntityAttribute.ItemCombination(typeof(iPorkCooked), typeof(iKetchup), typeof(iPancake), typeof(iCabbageCut), 1)]
	public class iTaco : Food { }

	[EntityAttribute.ItemCombination(typeof(iKetchup), typeof(iNoodle), 1)]
	public class iSpaghetti : Food { }

	[EntityAttribute.ItemCombination(typeof(iRice), typeof(iCurry), 1)]
	public class iCurryRice : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iCorianderCut), typeof(iGarlicCut), typeof(iGingerCut), 1)]
	public class iCurry : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iEgg), typeof(iFlour), 1)]
	public class iCakeEmbryo : Food { }

	[EntityAttribute.ItemCombination(typeof(iMilk), typeof(iMilk), 1)]
	public class iCream : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iRice), typeof(iLeaf), 1)]
	public class iZongzi : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iTrayWood), typeof(iEgg), typeof(iFlour), 1)]
	public class iMoonCake : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iBlackPepper), typeof(iTofu), typeof(iChiliCut), 1)]
	public class iMapoTofu : Food { }

	[EntityAttribute.ItemCombination(typeof(iPorkRaw), typeof(iRuneFire), typeof(iSalt), typeof(iCarrotCut), 1)]
	public class iFriedPorkInScoop : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iRunePoison), typeof(iRice), 1)]
	public class iLiquor : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iLeaf), 1)]
	public class iGreenTea : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iRuneWater), typeof(iRice), typeof(iLiquor), 1)]
	public class iBeer : Food { }

	[EntityAttribute.ItemCombination(typeof(iTentacle), typeof(iMilk), 1)]
	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iRuneWater), typeof(iGrapePurpleCut), typeof(iLiquor), 1)]
	public class iGrapeWine : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iRuneWater), typeof(iAppleRedCut), typeof(iLiquor), 1)]
	public class iAppleWine : Food { }

	[EntityAttribute.ItemCombination(typeof(iOrangeCut), typeof(iRuneWater), 1)]
	public class iOrangeJuice : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iGrapePurpleCut), 1)]
	public class iGrapeJuice : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iStrawberryCut), 1)]
	public class iStrawberryJuice : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iLemonCut), 1)]
	public class iLemonWater : Food { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iCoconutCut), 1)]
	public class iCoconutJuice : Food { }

	[EntityAttribute.ItemCombination(typeof(iLiquor), typeof(iLiquor), 1)]
	public class iAlcohol : Food { }

	[EntityAttribute.ItemCombination(typeof(iCorn), 2)]
	public class iCornCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iPumpkin), 2)]
	public class iPumpkinCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iRadish), 2)]
	public class iRadishCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iPineapple), 2)]
	public class iPineappleCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iBambooShoot), 2)]
	public class iBambooShootCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iGreenPepper), 2)]
	public class iGreenPepperCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iGreenBeans), 2)]
	public class iGreenBeansCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iGarlic), 2)]
	public class iGarlicCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iGinger), 2)]
	public class iGingerCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iScallion), 2)]
	public class iScallionCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iPeanut), 2)]
	public class iPeanutCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iLotusRoot), 2)]
	public class iLotusRootCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iCoriander), 2)]
	public class iCorianderCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iCollard), 2)]
	public class iCollardCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iCabbage), 2)]
	public class iCabbageCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iCucumber), 2)]
	public class iCucumberCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iChineseCabbage), 2)]
	public class iChineseCabbageCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iEggplant), 2)]
	public class iEggplantCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iSpinach), 2)]
	public class iSpinachCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iTomato), 2)]
	public class iTomatoCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iPotato), 2)]
	public class iPotatoCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iBroccoli), 2)]
	public class iBroccoliCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iCauliflower), 2)]
	public class iCauliflowerCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iChili), 2)]
	public class iChiliCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iCarrot), 2)]
	public class iCarrotCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iEnokiMushroom), 2)]
	public class iEnokiMushroomCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iMushroomRed), 2)]
	public class iMushroomRedCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iMushroomGreen), 2)]
	public class iMushroomGreenCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iMushroomBlue), 2)]
	public class iMushroomBlueCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iOnion), 2)]
	public class iOnionCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iWatermelon), 2)]
	public class iWatermelonCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iAvocado), 2)]
	public class iAvocadoCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iMulberries), 2)]
	public class iMulberriesCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iLychee), 2)]
	public class iLycheeCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iMango), 2)]
	public class iMangoCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iCarambola), 2)]
	public class iCarambolaCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iPapaya), 2)]
	public class iPapayaCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iPassionFruit), 2)]
	public class iPassionFruitCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iKiwi), 2)]
	public class iKiwiCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iMangosteen), 2)]
	public class iMangosteenCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iCoconut), 2)]
	public class iCoconutCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iDurian), 2)]
	public class iDurianCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iCherry), 2)]
	public class iCherryCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iPomegranate), 2)]
	public class iPomegranateCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iHawthorn), 2)]
	public class iHawthornCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iAppleRed), 2)]
	public class iAppleRedCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iAppleGreen), 2)]
	public class iAppleGreenCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iPear), 2)]
	public class iPearCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iPeach), 2)]
	public class iPeachCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iBanana), 2)]
	public class iBananaCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iOrange), 2)]
	public class iOrangeCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iGrapePurple), 2)]
	public class iGrapePurpleCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iGrapeGreen), 2)]
	public class iGrapeGreenCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iStrawberry), 2)]
	public class iStrawberryCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iBlueberry), 2)]
	public class iBlueberryCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iLemon), 2)]
	public class iLemonCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iLime), 2)]
	public class iLimeCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iPitaya), 2)]
	public class iPitayaCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iApricot), 2)]
	public class iApricotCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iHoneydew), 2)]
	public class iHoneydewCut : Food { }

	[EntityAttribute.ItemCombination(typeof(iMilk), typeof(iStrawberryJuice), 1)]
	public class iBerryMilk : Food { }


	public class iBeefRaw : Food { }
	public class iChickenLegRaw : Food { }
	public class iChickenWingRaw : Food { }
	public class iPorkRaw : Food { }
	public class iSausageRaw : Food { }
	public class iTurkeyRaw : Food { }
	public class iFishRaw : Food { }
	public class iPterophyllum : Food { }
	public class iFlounder : Food { }
	public class iSquidRaw : Food { }
	public class iLobsterRaw : Food { }
	public class iCrabRaw : Food { }
	public class iSnailRaw : Food { }
	public class iChocolate : Food { }
	public class iCandy : Food { }
	public class iEgg : Food { }
	public class iRice : Food { }
	public class iHoney : Food { }
	public class iSalt : Food { }
	public class iBlackPepper : Food { }
	public class iFlour : Food { }
	public class iColaRed : Food { }
	public class iColaBlue : Food { }
	public class iCoffee : Food { }
	public class iMilk : Food { }
	public class iNori : Food { }




	public class iCorn : Food { }
	public class iPumpkin : Food { }
	public class iRadish : Food { }
	public class iPineapple : Food { }
	public class iBambooShoot : Food { }
	public class iGreenPepper : Food { }
	public class iGreenBeans : Food { }
	public class iGarlic : Food { }
	public class iGinger : Food { }
	public class iScallion : Food { }
	public class iPeanut : Food { }
	public class iLotusRoot : Food { }
	public class iCoriander : Food { }
	public class iCollard : Food { }
	public class iCabbage : Food { }
	public class iCucumber : Food { }
	public class iChineseCabbage : Food { }
	public class iEggplant : Food { }
	public class iSpinach : Food { }
	public class iTomato : Food { }
	public class iPotato : Food { }
	public class iBroccoli : Food { }
	public class iCauliflower : Food { }
	public class iChili : Food { }
	public class iCarrot : Food { }
	public class iEnokiMushroom : Food { }
	public class iMushroomRed : Food { }
	public class iMushroomGreen : Food { }
	public class iMushroomBlue : Food { }
	public class iOnion : Food { }


	public class iWatermelon : Food { }
	public class iAvocado : Food { }
	public class iMulberries : Food { }
	public class iLychee : Food { }
	public class iMango : Food { }
	public class iCarambola : Food { }
	public class iPapaya : Food { }
	public class iPassionFruit : Food { }
	public class iKiwi : Food { }
	public class iMangosteen : Food { }
	public class iCoconut : Food { }
	public class iDurian : Food { }
	public class iCherry : Food { }
	public class iPomegranate : Food { }
	public class iHawthorn : Food { }
	public class iAppleRed : Food { }
	public class iAppleGreen : Food { }
	public class iPear : Food { }
	public class iPeach : Food { }
	public class iBanana : Food { }
	public class iOrange : Food { }
	public class iGrapePurple : Food { }
	public class iGrapeGreen : Food { }
	public class iStrawberry : Food { }
	public class iBlueberry : Food { }
	public class iLemon : Food { }
	public class iLime : Food { }
	public class iPitaya : Food { }
	public class iApricot : Food { }
	public class iHoneydew : Food { }



}