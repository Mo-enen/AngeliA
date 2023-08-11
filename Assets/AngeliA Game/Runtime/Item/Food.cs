using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaGame {

	public abstract class iFood : Item {
		public sealed override int MaxStackCount => 16;
	}
	public abstract class iMeat : iFood { }
	public abstract class iSnack : iFood { }
	public abstract class iFruit : iFood { }
	public abstract class iVegetable : iFood { }




	public class iBeefRaw : iMeat { }
	public class iChickenLegRaw : iMeat { }
	public class iChickenWingRaw : iMeat { }
	public class iSteakRaw : iMeat { }
	public class iMeatballRaw : iMeat { }
	public class iMeatSkewersRaw : iMeat { }
	public class iSausageRaw : iMeat { }
	public class iTurkeyRaw : iMeat { }
	public class iFishRaw : iMeat { }
	public class iPterophyllum : iMeat { }
	public class iFlounder : iMeat { }
	public class iSquidRaw : iMeat { }
	public class iLobsterRaw : iMeat { }
	public class iCrabRaw : iMeat { }
	public class iSnailRaw : iMeat { }
	public class iBeefCooked : iMeat { }
	public class iChickenLegCooked : iMeat { }
	public class iChickenWingCooked : iMeat { }
	public class iSteakCooked : iMeat { }
	public class iMeatballCooked : iMeat { }
	public class iMeatSkewersCooked : iMeat { }
	public class iSausageCooked : iMeat { }
	public class iTurkeyCooked : iMeat { }
	public class iFishCooked : iMeat { }
	public class iPterophyllumCooked : iMeat { }
	public class iFlounderCooked : iMeat { }
	public class iSquidCooked : iMeat { }
	public class iLobsterCooked : iMeat { }
	public class iCrabCooked : iMeat { }
	public class iSnailCooked : iMeat { }


	public class iCreamHorn : iSnack { }
	public class iCroissant : iSnack { }
	public class iHotdog : iSnack { }
	public class iBreadKnot : iSnack { }
	public class iLyeRoll : iSnack { }
	public class iCakeCream : iSnack { }
	public class iCakeRoll : iSnack { }
	public class iHamburger : iSnack { }
	public class iBreadPork : iSnack { }
	public class iDonut : iSnack { }
	public class iFries : iSnack { }
	public class iBaoZi : iSnack { }
	public class iDumpling : iSnack { }
	public class iSushi : iSnack { }
	public class iChocolate : iSnack { }
	public class iCheese : iSnack { }
	public class iOnigiri : iSnack { }
	public class iCookie : iSnack { }
	public class iPizza : iSnack { }
	public class iCandy : iSnack { }
	public class iEgg : iSnack { }
	public class iSandwichBiscuit : iSnack { }
	public class iPotatoChips : iSnack { }
	public class iEggTart : iSnack { }
	public class iSoup : iSnack { }
	public class iLollipop : iSnack { }
	public class iPudding : iSnack { }
	public class iIceCream : iSnack { }
	public class iToast : iSnack { }
	public class iNoodle : iSnack { }
	public class iRice : iSnack { }
	public class iHoney : iSnack { }
	public class iKetchup : iSnack { }
	public class iOnionRing : iSnack { }
	public class iPopcorn : iSnack { }
	public class iFriedEgg : iSnack { }
	public class iChocolateMousse : iSnack { }
	public class iCherryMousse : iSnack { }
	public class iBacon : iSnack { }
	public class iInstantNoodle : iSnack { }
	public class iSalt : iSnack { }
	public class iBlackPepper : iSnack { }
	public class iSugarCube : iSnack { }
	public class iDriedBeef : iSnack { }
	public class iButter : iSnack { }
	public class iTakoyaki : iSnack { }
	public class iArepa : iSnack { }
	public class iTaco : iSnack { }
	public class iSpaghetti : iSnack { }
	public class iCurryRice : iSnack { }
	public class iCurry : iSnack { }
	public class iCakeEmbryo : iSnack { }
	public class iCream : iSnack { }
	public class iFlour : iSnack { }
	public class iZongzi : iSnack { }
	public class iMoonCake : iSnack { }
	public class iMapoTofu : iSnack { }
	public class iFriedPorkInScoop : iSnack { }
	public class iAlcohol : iSnack { }
	public class iLiquor : iSnack { }
	public class iColaRed : iSnack { }
	public class iColaBlue : iSnack { }
	public class iCoffee : iSnack { }
	public class iGreenTea : iSnack { }
	public class iBeer : iSnack { }
	public class iMilk : iSnack { }
	public class iTakoMilk : iSnack { }
	public class iGrapeWine : iSnack { }
	public class iAppleWine : iSnack { }
	public class iOrangeJuice : iSnack { }
	public class iGrapeJuice : iSnack { }
	public class iStrawberryJuice : iSnack { }
	public class iLemonWater : iSnack { }
	public class iCoconutJuice : iSnack { }
	public class iEnergyDrink : iSnack { }




	public class iCorn : iVegetable { }
	public class iPumpkin : iVegetable { }
	public class iRadish : iVegetable { }
	public class iPineapple : iVegetable { }
	public class iBambooShoot : iVegetable { }
	public class iGreenPepper : iVegetable { }
	public class iGreenBeans : iVegetable { }
	public class iGarlic : iVegetable { }
	public class iGinger : iVegetable { }
	public class iScallion : iVegetable { }
	public class iPeanut : iVegetable { }
	public class iLotusRoot : iVegetable { }
	public class iCoriander : iVegetable { }
	public class iCollard : iVegetable { }
	public class iCabbage : iVegetable { }
	public class iCornCut : iVegetable { }
	public class iPumpkinCut : iVegetable { }
	public class iRadishCut : iVegetable { }
	public class iPineappleCut : iVegetable { }
	public class iBambooShootCut : iVegetable { }
	public class iGreenPepperCut : iVegetable { }
	public class iGreenBeansCut : iVegetable { }
	public class iGarlicCut : iVegetable { }
	public class iGingerCut : iVegetable { }
	public class iScallionCut : iVegetable { }
	public class iPeanutCut : iVegetable { }
	public class iLotusRootCut : iVegetable { }
	public class iCorianderCut : iVegetable { }
	public class iCollardCut : iVegetable { }
	public class iCabbageCut : iVegetable { }
	public class iCucumber : iVegetable { }
	public class iChineseCabbage : iVegetable { }
	public class iEggplant : iVegetable { }
	public class iSpinach : iVegetable { }
	public class iTomato : iVegetable { }
	public class iPotato : iVegetable { }
	public class iBroccoli : iVegetable { }
	public class iCauliflower : iVegetable { }
	public class iChili : iVegetable { }
	public class iCarrot : iVegetable { }
	public class iEnokiMushroom : iVegetable { }
	public class iMushroomRed : iVegetable { }
	public class iMushroomGreen : iVegetable { }
	public class iMushroomBlue : iVegetable { }
	public class iMushroomOrange : iVegetable { }
	public class iCucumberCut : iVegetable { }
	public class iChineseCabbageCut : iVegetable { }
	public class iEggplantCut : iVegetable { }
	public class iSpinachCut : iVegetable { }
	public class iTomatoCut : iVegetable { }
	public class iPotatoCut : iVegetable { }
	public class iBroccoliCut : iVegetable { }
	public class iCauliflowerCut : iVegetable { }
	public class iChiliCut : iVegetable { }
	public class iCarrotCut : iVegetable { }
	public class iEnokiMushroomCut : iVegetable { }
	public class iMushroomRedCut : iVegetable { }
	public class iMushroomGreenCut : iVegetable { }
	public class iMushroomBlueCut : iVegetable { }
	public class iMushroomOrangeCut : iVegetable { }





	public class iWatermelon : iFruit { }
	public class iAvocado : iFruit { }
	public class iMulberries : iFruit { }
	public class iLychee : iFruit { }
	public class iMango : iFruit { }
	public class iCarambola : iFruit { }
	public class iPapaya : iFruit { }
	public class iPassionFruit : iFruit { }
	public class iKiwi : iFruit { }
	public class iMangosteen : iFruit { }
	public class iCoconut : iFruit { }
	public class iDurian : iFruit { }
	public class iCherry : iFruit { }
	public class iPomegranate : iFruit { }
	public class iHawthorn : iFruit { }
	public class iWatermelonCut : iFruit { }
	public class iAvocadoCut : iFruit { }
	public class iMulberriesCut : iFruit { }
	public class iLycheeCut : iFruit { }
	public class iMangoCut : iFruit { }
	public class iCarambolaCut : iFruit { }
	public class iPapayaCut : iFruit { }
	public class iPassionFruitCut : iFruit { }
	public class iKiwiCut : iFruit { }
	public class iMangosteenCut : iFruit { }
	public class iCoconutCut : iFruit { }
	public class iDurianCut : iFruit { }
	public class iCherryCut : iFruit { }
	public class iPomegranateCut : iFruit { }
	public class iHawthornCut : iFruit { }
	public class iAppleRed : iFruit { }
	public class iAppleGreen : iFruit { }
	public class iPear : iFruit { }
	public class iPeach : iFruit { }
	public class iBanana : iFruit { }
	public class iOrange : iFruit { }
	public class iGrapePurple : iFruit { }
	public class iGrapeGreen : iFruit { }
	public class iStrawberry : iFruit { }
	public class iBlueberry : iFruit { }
	public class iLemon : iFruit { }
	public class iLime : iFruit { }
	public class iPitaya : iFruit { }
	public class iApricot : iFruit { }
	public class iHoneydew : iFruit { }
	public class iAppleRedCut : iFruit { }
	public class iAppleGreenCut : iFruit { }
	public class iPearCut : iFruit { }
	public class iPeachCut : iFruit { }
	public class iBananaCut : iFruit { }
	public class iOrangeCut : iFruit { }
	public class iGrapePurpleCut : iFruit { }
	public class iGrapeGreenCut : iFruit { }
	public class iStrawberryCut : iFruit { }
	public class iBlueberryCut : iFruit { }
	public class iLemonCut : iFruit { }
	public class iLimeCut : iFruit { }
	public class iPitayaCut : iFruit { }
	public class iApricotCut : iFruit { }
	public class iHoneydewCut : iFruit { }






}