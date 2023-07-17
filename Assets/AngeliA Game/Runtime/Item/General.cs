using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaGame {


	public class iItemCoin : Item {
		public override bool TouchToCollect => true;
		public override bool ConsumeOnCollect => true;
		public override void OnCollect (Entity target) {
			base.OnCollect(target);
			if (target == null || !Inventory.HasInventory(target.TypeID)) return;
			Inventory.AddCoin(target.TypeID, 1);
		}
	}

	public class iGoldBag : Item {
		public override bool TouchToCollect => true;
		public override bool ConsumeOnCollect => true;
		public override void OnCollect (Entity target) {
			base.OnCollect(target);
			if (target == null || !Inventory.HasInventory(target.TypeID)) return;
			Inventory.AddCoin(target.TypeID, 10);
		}
	}


	public class iGunpowder : Item { }

	public class iConfetti : Item { }

	public class iCrayon : Item { }

	public class iMusicNote : Item { }

	public class iBucket : Item { }

	public class iNose : Item { }

	public class iTooth : Item { }

	public class iRibbon : Item { }

	public class iBottledWater : Item { }

	public class iTrayWood : Item { }

	public class iCapsule : Item { }

	public class iLeather : Item { }

	public class iCrystalBall : Item { }

	public class iCones : Item { }

	public class iTrophy : Item { }

	public class iDartBoard : Item { }

	public class iCornet : Item { }

	public class iSaxophone : Item { }

	public class iGuitar : Item { }

	public class iDrum : Item { }

	public class iOcarina : Item { }

	public class iGamepad : Item { }

	public class iKeyboard : Item { }

	public class iMonitor : Item { }

	public class iPropeller : Item { }

	public class iButton : Item { }

	public class iBattery : Item { }

	public class iYarn : Item { }

	public class iTeddyBearDoll : Item { }

	public class iBarbieDoll : Item { }

	public class iCottonBall : Item { }

	public class iLove : Item { }

	public class iFabric : Item { }

	public class iTorch : Item { }

	public class iDrill : Item { }

	public class iSoupCan : Item { }

	public class iWheel : Item { }

	public class iMagnet : Item { }

	public class iEyeBall : Item { }

	public class iCorrugatedBox : Item { }

	public class iPillow : Item { }

	public class iMirror : Item { }

	public class iGameConsole : Item { }

	public class iWatch : Item { }

	public class iElectricDrill : Item { }

	public class iSlotMachine : Item { }

	public class iBass : Item { }

	public class iAlarmClock : Item { }

	public class iFan : Item { }

	public class iRadio : Item { }

	public class iPianoKeyboard : Item { }

	public class iCamera : Item { }

	public class iVideoRecorder : Item { }

	public class iPositronScanner : Item { }

	public class iLighter : Item { }

	public class iLaptop : Item { }

	public class iPhone : Item { }

	public class iGoldenWatch : Item { }

	public class iGoldenTelescope : Item { }

	public class iGoldenHourglass : Item { }

	public class iGoldenBegleri : Item { }

	public class iGoldenSpring : Item { }

	public class iGoldenGameboy : Item { }

	public class iGoldenGamepad : Item { }

	public class iGoldenPen : Item { }

	public class iGoldenNotepad : Item { }

	public class iGoldenHook : Item { }

	public class iGoldenFan : Item { }

	public class iGoldenKey : Item { }

	public class iGoldenPipe : Item { }

	public class iGoldenCross : Item { }

	public class iGoldenFeather : Item { }

	public class iBookRed : Item { }

	public class iBookBlue : Item { }

	public class iBookYellow : Item { }

	public class iBookGreen : Item { }

	public class iBook : Item { }

	public class iMedalGold : Item { }

	public class iMedalSilver : Item { }

	public class iMedalClay : Item { }

	public class iBell : Item { }

	public class iFloppyDisk : Item { }

	public class iCrown : Item { }

	public class iScroll : Item { }

	public class iGourd : Item { }

	public class iGiftBox : Item { }

	public class iCandle : Item { }

	public class iGemRed : Item { }

	public class iGemBlue : Item { }

	public class iGemOrange : Item { }

	public class iGemGreen : Item { }

	public class iGem : Item { }

	public class iCuteGhost : Item { }

	public class iTentacle : Item { }

	public class iTakodachi : Item { }

	public class iFlowerRed : Item { }

	public class iFlowerPink : Item { }

	public class iFlowerYellow : Item { }

	public class iFlowerCyan : Item { }

	public class iFlowerWhite : Item { }

	public class iFlowerpot : Item { }

	public class iPottedPlant : Item { }

	public class iRubyRed : Item { }

	public class iRubyBlue : Item { }

	public class iRubyOrange : Item { }

	public class iRubyGreen : Item { }

	public class iRuby : Item { }

	public class iEar : Item { }

	public class iSpoonFork : Item { }

	public class iLockIron : Item { }

	public class iLockGold : Item { }

	public class iTeapot : Item { }

	public class iTeacup : Item { }

	public class iClay : Item { }

	public class iPokerCard : Item { }

	public class iGamblingChip : Item { }

	public class iLeaf : Item { }

	public class iTotemFire : Item { }

	public class iTotemWater : Item { }

	public class iTotemLightning : Item { }

	public class iTotemPoison : Item { }

	public class iSkull : Item { }

	public class iABC : Item { }

	public class iDialogBox : Item { }

	public class iPaper : Item { }

	public class iIngotIron : Item { }

	public class iIngotGold : Item { }

	public class iProcessor : Item { }

	public class iElectricWire : Item { }

	public class iComb : Item { }

	public class iVolatilePotionRed : Item { }

	public class iVolatilePotionBlue : Item { }

	public class iVolatilePotionOrange : Item { }

	public class iVolatilePotionGreen : Item { }

	public class iVolatilePotionEmpty : Item { }

	public class iRope : Item { }

	public class iIronWire : Item { }

	public class iTreeBranch : Item { }

	public class iTreeBranchBundle : Item { }

	public class iTreeTrunk : Item { }

	public class iTreeStump : Item { }

	public class iWoodBoard : Item { }

	public class iHorn : Item { }

	public class iLantern : Item { }

	public class iScales : Item { }

	public class iPotionRed : Item { }

	public class iPotionBlue : Item { }

	public class iPotionOrange : Item { }

	public class iPotionGreen : Item { }

	public class iPotionEmpty : Item { }

	public class iBrokenBone : Item { }

	public class iMeatBone : Item { }

	public class iFishBone : Item { }

	public class iCatPaw : Item { }

	public class iBearPaw : Item { }

	public class iLetter : Item { }

	public class iMachineBeetle : Item { }

	public class iBulb : Item { }

	public class iBomb : Item { }

	public class iFirecracker : Item { }

	public class iRuneFire : Item { }

	public class iRuneWater : Item { }

	public class iRuneLightning : Item { }

	public class iRunePoison : Item { }

	public class iDice : Item { }

	public class iKeyWood : Item { }

	public class iKeyIron : Item { }

	public class iKeyGold : Item { }

	public class iStone : Item { }

	public class iStonePolished : Item { }

	public class iFlint : Item { }

	public class iFlintPolished : Item { }

	public class iBolt : Item { }

	public class iHandbag : Item { }

	public class iFirstAidKit : Item { }



}