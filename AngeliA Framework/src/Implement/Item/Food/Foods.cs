using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 


[ItemCombination(typeof(iBeefRaw), 4)]
[ItemCombination(typeof(iPorkRaw), 3)]
public class iMeatballRaw : Food { }

[ItemCombination(typeof(iMeatballRaw), typeof(iMeatballRaw), typeof(iMeatballRaw), typeof(iTreeBranch), 1)]
public class iMeatSkewersRaw : Food { }

[ItemCombination(typeof(iMeatballCooked), typeof(iMeatballCooked), typeof(iMeatballCooked), typeof(iTreeBranch), 1)]
[ItemCombination(typeof(iRuneFire), typeof(iMeatSkewersRaw), 1)]
public class iMeatSkewersCooked : Food { }

[ItemCombination(typeof(iBeefRaw), typeof(iRuneFire), 1)]
public class iBeefCooked : Food { }

[ItemCombination(typeof(iChickenLegRaw), typeof(iRuneFire), 1)]
public class iChickenLegCooked : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iChickenWingRaw), 1)]
public class iChickenWingCooked : Food { }

[ItemCombination(typeof(iPorkRaw), typeof(iRuneFire), 1)]
public class iPorkCooked : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iMeatballRaw), 1)]
public class iMeatballCooked : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iSausageRaw), 1)]
public class iSausageCooked : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iTurkeyRaw), 1)]
public class iTurkeyCooked : Food { }

[ItemCombination(typeof(iFishRaw), typeof(iRuneFire), 1)]
public class iFishCooked : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iPterophyllum), 1)]
public class iPterophyllumCooked : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iFlounder), 1)]
public class iFlounderCooked : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iSquidRaw), 1)]
public class iSquidCooked : Food { }

[ItemCombination(typeof(iLobsterRaw), typeof(iRuneFire), 1)]
public class iLobsterCooked : Food { }

[ItemCombination(typeof(iCrabRaw), typeof(iRuneFire), 1)]
public class iCrabCooked : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iSnailRaw), 1)]
public class iSnailCooked : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iHorn), typeof(iCream), typeof(iFlour), 1)]
public class iCreamHorn : Food { }

[ItemCombination(typeof(iHorn), typeof(iBread), typeof(iCream), 1)]
public class iCroissant : Food { }

[ItemCombination(typeof(iSausageCooked), typeof(iBread), typeof(iKetchup), typeof(iButter), 1)]
public class iHotdog : Food { }

[ItemCombination(typeof(iRope), typeof(iBread), 1)]
public class iBreadKnot : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iSalt), typeof(iFlour), 1)]
public class iLyeRoll : Food { }

[ItemCombination(typeof(iCream), typeof(iCakeEmbryo), 1)]
public class iCakeCream : Food { }

[ItemCombination(typeof(iCream), typeof(iCream), typeof(iCakeEmbryo), 1)]
public class iCakeRoll : Food { }

[ItemCombination(typeof(iPorkCooked), typeof(iBread), typeof(iBread), typeof(iCollardCut), 1)]
[ItemCombination(typeof(iPorkCooked), typeof(iBread), typeof(iBread), typeof(iCabbageCut), 1)]
[ItemCombination(typeof(iChineseCabbageCut), typeof(iPorkCooked), typeof(iBread), typeof(iBread), 1)]
public class iHamburger : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iFlour), typeof(iFlour), 1)]
public class iBread : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iCream), typeof(iFlour), 1)]
public class iDonut : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iPotatoCut), 1)]
public class iFries : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iFlour), typeof(iMeatballRaw), 1)]
public class iBaoZi : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iPorkRaw), typeof(iRuneFire), typeof(iFlour), 1)]
public class iDumpling : Food { }

[ItemCombination(typeof(iSausageCooked), typeof(iCucumberCut), typeof(iRice), typeof(iNori), 1)]
public class iSushi : Food { }

[ItemCombination(typeof(iMilk), typeof(iMilk), typeof(iSalt), 1)]
public class iCheese : Food { }

[ItemCombination(typeof(iRice), typeof(iNori), 1)]
public class iOnigiri : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iFlour), typeof(iChocolate), 1)]
public class iCookie : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iSausageRaw), typeof(iCheese), typeof(iPancake), 1)]
public class iPizza : Food { }

[ItemCombination(typeof(iCream), typeof(iCookie), 1)]
public class iSandwichBiscuit : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iPotatoCut), typeof(iPotatoCut), 1)]
public class iPotatoChips : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iEgg), typeof(iEgg), typeof(iFlour), 1)]
public class iEggTart : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iCucumberCut), typeof(iRuneFire), typeof(iPumpkinCut), 1)]
public class iSoup : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iFlour), typeof(iFlour), 1)]
public class iPancake : Food { }

[ItemCombination(typeof(iOrangeJuice), typeof(iMilk), typeof(iFlour), typeof(iCornCut), 1)]
public class iPudding : Food { }

[ItemCombination(typeof(iMilk), typeof(iCream), 1)]
public class iIceCream : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iEgg), typeof(iFlour), typeof(iFlour), 1)]
public class iToast : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iFlour), typeof(iFlour), typeof(iFlour), 1)]
public class iNoodle : Food { }

[ItemCombination(typeof(iSalt), typeof(iTomatoCut), typeof(iTomatoCut), 1)]
public class iKetchup : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iOnionCut), 1)]
public class iOnionRing : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iCornCut), 1)]
public class iPopcorn : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iEgg), 1)]
public class iFriedEgg : Food { }

[ItemCombination(typeof(iCakeEmbryo), typeof(iChocolate), 1)]
public class iChocolateMousse : Food { }

[ItemCombination(typeof(iCakeEmbryo), typeof(iCherryCut), 1)]
public class iCherryMousse : Food { }

[ItemCombination(typeof(iPorkCooked), typeof(iRuneFire), 1)]
public class iBacon : Food { }

[ItemCombination(typeof(iBlackPepper), typeof(iSalt), typeof(iNoodle), 1)]
public class iInstantNoodle : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iSalt), typeof(iGreenBeansCut), 1)]
public class iTofu : Food { }

[ItemCombination(typeof(iBeefCooked), typeof(iRuneFire), 1)]
public class iDriedBeef : Food { }

[ItemCombination(typeof(iMilk), typeof(iSalt), 1)]
public class iButter : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iTentacle), typeof(iSalt), 1)]
public class iTakoyaki : Food { }

[ItemCombination(typeof(iBeefCooked), typeof(iChili), typeof(iPancake), typeof(iSpinachCut), 1)]
public class iArepa : Food { }

[ItemCombination(typeof(iPorkCooked), typeof(iKetchup), typeof(iPancake), typeof(iCabbageCut), 1)]
public class iTaco : Food { }

[ItemCombination(typeof(iKetchup), typeof(iNoodle), 1)]
public class iSpaghetti : Food { }

[ItemCombination(typeof(iRice), typeof(iCurry), 1)]
public class iCurryRice : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iCorianderCut), typeof(iGarlicCut), typeof(iGingerCut), 1)]
public class iCurry : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iEgg), typeof(iFlour), 1)]
public class iCakeEmbryo : Food { }

[ItemCombination(typeof(iMilk), typeof(iMilk), 1)]
public class iCream : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iRice), typeof(iLeaf), 1)]
public class iZongzi : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iTrayWood), typeof(iEgg), typeof(iFlour), 1)]
public class iMoonCake : Food { }

[ItemCombination(typeof(iRuneFire), typeof(iBlackPepper), typeof(iTofu), typeof(iChiliCut), 1)]
public class iMapoTofu : Food { }

[ItemCombination(typeof(iPorkRaw), typeof(iRuneFire), typeof(iSalt), typeof(iCarrotCut), 1)]
public class iFriedPorkInScoop : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iRunePoison), typeof(iRice), 1)]
public class iLiquor : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iLeaf), 1)]
public class iGreenTea : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iRuneWater), typeof(iRice), typeof(iLiquor), 1)]
public class iBeer : Food { }

[ItemCombination(typeof(iTentacle), typeof(iMilk), 1)]
[ItemCombination(typeof(iRuneWater), typeof(iRuneWater), typeof(iGrapePurpleCut), typeof(iLiquor), 1)]
public class iGrapeWine : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iRuneWater), typeof(iAppleRedCut), typeof(iLiquor), 1)]
public class iAppleWine : Food { }

[ItemCombination(typeof(iOrangeCut), typeof(iRuneWater), 1)]
public class iOrangeJuice : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iGrapePurpleCut), 1)]
public class iGrapeJuice : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iStrawberryCut), 1)]
public class iStrawberryJuice : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iLemonCut), 1)]
public class iLemonWater : Food { }

[ItemCombination(typeof(iRuneWater), typeof(iCoconutCut), 1)]
public class iCoconutJuice : Food { }

[ItemCombination(typeof(iLiquor), typeof(iLiquor), 1)]
public class iAlcohol : Food { }



[ItemCombination(typeof(iCorn), typeof(iCuttingBoard), 2, true, false)]
public class iCornCut : Food { }

[ItemCombination(typeof(iPumpkin), typeof(iCuttingBoard), 2, true, false)]
public class iPumpkinCut : Food { }

[ItemCombination(typeof(iRadish), typeof(iCuttingBoard), 2, true, false)]
public class iRadishCut : Food { }

[ItemCombination(typeof(iPineapple), typeof(iCuttingBoard), 2, true, false)]
public class iPineappleCut : Food { }

[ItemCombination(typeof(iBambooShoot), typeof(iCuttingBoard), 2, true, false)]
public class iBambooShootCut : Food { }

[ItemCombination(typeof(iGreenPepper), typeof(iCuttingBoard), 2, true, false)]
public class iGreenPepperCut : Food { }

[ItemCombination(typeof(iGreenBeans), typeof(iCuttingBoard), 2, true, false)]
public class iGreenBeansCut : Food { }

[ItemCombination(typeof(iGarlic), typeof(iCuttingBoard), 2, true, false)]
public class iGarlicCut : Food { }

[ItemCombination(typeof(iGinger), typeof(iCuttingBoard), 2, true, false)]
public class iGingerCut : Food { }

[ItemCombination(typeof(iScallion), typeof(iCuttingBoard), 2, true, false)]
public class iScallionCut : Food { }

[ItemCombination(typeof(iPeanut), typeof(iCuttingBoard), 2, true, false)]
public class iPeanutCut : Food { }

[ItemCombination(typeof(iLotusRoot), typeof(iCuttingBoard), 2, true, false)]
public class iLotusRootCut : Food { }

[ItemCombination(typeof(iCoriander), typeof(iCuttingBoard), 2, true, false)]
public class iCorianderCut : Food { }

[ItemCombination(typeof(iCollard), typeof(iCuttingBoard), 2, true, false)]
public class iCollardCut : Food { }

[ItemCombination(typeof(iCabbage), typeof(iCuttingBoard), 2, true, false)]
public class iCabbageCut : Food { }

[ItemCombination(typeof(iCucumber), typeof(iCuttingBoard), 2, true, false)]
public class iCucumberCut : Food { }

[ItemCombination(typeof(iChineseCabbage), typeof(iCuttingBoard), 2, true, false)]
public class iChineseCabbageCut : Food { }

[ItemCombination(typeof(iEggplant), typeof(iCuttingBoard), 2, true, false)]
public class iEggplantCut : Food { }

[ItemCombination(typeof(iSpinach), typeof(iCuttingBoard), 2, true, false)]
public class iSpinachCut : Food { }

[ItemCombination(typeof(iTomato), typeof(iCuttingBoard), 2, true, false)]
public class iTomatoCut : Food { }

[ItemCombination(typeof(iPotato), typeof(iCuttingBoard), 2, true, false)]
public class iPotatoCut : Food { }

[ItemCombination(typeof(iBroccoli), typeof(iCuttingBoard), 2, true, false)]
public class iBroccoliCut : Food { }

[ItemCombination(typeof(iCauliflower), typeof(iCuttingBoard), 2, true, false)]
public class iCauliflowerCut : Food { }

[ItemCombination(typeof(iChili), typeof(iCuttingBoard), 2, true, false)]
public class iChiliCut : Food { }

[ItemCombination(typeof(iCarrot), typeof(iCuttingBoard), 2, true, false)]
public class iCarrotCut : Food { }

[ItemCombination(typeof(iEnokiMushroom), typeof(iCuttingBoard), 2, true, false)]
public class iEnokiMushroomCut : Food { }

[ItemCombination(typeof(iMushroomRed), typeof(iCuttingBoard), 2, true, false)]
public class iMushroomRedCut : Food { }

[ItemCombination(typeof(iMushroomGreen), typeof(iCuttingBoard), 2, true, false)]
public class iMushroomGreenCut : Food { }

[ItemCombination(typeof(iMushroomBlue), typeof(iCuttingBoard), 2, true, false)]
public class iMushroomBlueCut : Food { }

[ItemCombination(typeof(iOnion), typeof(iCuttingBoard), 2, true, false)]
public class iOnionCut : Food { }

[ItemCombination(typeof(iWatermelon), typeof(iCuttingBoard), 2, true, false)]
public class iWatermelonCut : Food { }

[ItemCombination(typeof(iAvocado), typeof(iCuttingBoard), 2, true, false)]
public class iAvocadoCut : Food { }

[ItemCombination(typeof(iMulberries), typeof(iCuttingBoard), 2, true, false)]
public class iMulberriesCut : Food { }

[ItemCombination(typeof(iLychee), typeof(iCuttingBoard), 2, true, false)]
public class iLycheeCut : Food { }

[ItemCombination(typeof(iMango), typeof(iCuttingBoard), 2, true, false)]
public class iMangoCut : Food { }

[ItemCombination(typeof(iCarambola), typeof(iCuttingBoard), 2, true, false)]
public class iCarambolaCut : Food { }

[ItemCombination(typeof(iPapaya), typeof(iCuttingBoard), 2, true, false)]
public class iPapayaCut : Food { }

[ItemCombination(typeof(iPassionFruit), typeof(iCuttingBoard), 2, true, false)]
public class iPassionFruitCut : Food { }

[ItemCombination(typeof(iKiwi), typeof(iCuttingBoard), 2, true, false)]
public class iKiwiCut : Food { }

[ItemCombination(typeof(iMangosteen), typeof(iCuttingBoard), 2, true, false)]
public class iMangosteenCut : Food { }

[ItemCombination(typeof(iCoconut), typeof(iCuttingBoard), 2, true, false)]
public class iCoconutCut : Food { }

[ItemCombination(typeof(iDurian), typeof(iCuttingBoard), 2, true, false)]
public class iDurianCut : Food { }

[ItemCombination(typeof(iCherry), typeof(iCuttingBoard), 2, true, false)]
public class iCherryCut : Food { }

[ItemCombination(typeof(iPomegranate), typeof(iCuttingBoard), 2, true, false)]
public class iPomegranateCut : Food { }

[ItemCombination(typeof(iHawthorn), typeof(iCuttingBoard), 2, true, false)]
public class iHawthornCut : Food { }

[ItemCombination(typeof(iAppleRed), typeof(iCuttingBoard), 2, true, false)]
public class iAppleRedCut : Food { }

[ItemCombination(typeof(iAppleGreen), typeof(iCuttingBoard), 2, true, false)]
public class iAppleGreenCut : Food { }

[ItemCombination(typeof(iPear), typeof(iCuttingBoard), 2, true, false)]
public class iPearCut : Food { }

[ItemCombination(typeof(iPeach), typeof(iCuttingBoard), 2, true, false)]
public class iPeachCut : Food { }

[ItemCombination(typeof(iBanana), typeof(iCuttingBoard), 2, true, false)]
public class iBananaCut : Food { }

[ItemCombination(typeof(iOrange), typeof(iCuttingBoard), 2, true, false)]
public class iOrangeCut : Food { }

[ItemCombination(typeof(iGrapePurple), typeof(iCuttingBoard), 2, true, false)]
public class iGrapePurpleCut : Food { }

[ItemCombination(typeof(iGrapeGreen), typeof(iCuttingBoard), 2, true, false)]
public class iGrapeGreenCut : Food { }

[ItemCombination(typeof(iStrawberry), typeof(iCuttingBoard), 2, true, false)]
public class iStrawberryCut : Food { }

[ItemCombination(typeof(iBlueberry), typeof(iCuttingBoard), 2, true, false)]
public class iBlueberryCut : Food { }

[ItemCombination(typeof(iLemon), typeof(iCuttingBoard), 2, true, false)]
public class iLemonCut : Food { }

[ItemCombination(typeof(iLime), typeof(iCuttingBoard), 2, true, false)]
public class iLimeCut : Food { }

[ItemCombination(typeof(iPitaya), typeof(iCuttingBoard), 2, true, false)]
public class iPitayaCut : Food { }

[ItemCombination(typeof(iApricot), typeof(iCuttingBoard), 2, true, false)]
public class iApricotCut : Food { }

[ItemCombination(typeof(iHoneydew), typeof(iCuttingBoard), 2, true, false)]
public class iHoneydewCut : Food { }


[ItemCombination(typeof(iMilk), typeof(iStrawberryJuice), 1)]
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