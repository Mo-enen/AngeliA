using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public class iChessPawn : Item { }


public class iChessKnight : Item { }


public class iChessBishop : Item { }


public class iChessRook : Item { }


public class iChessQueen : Item { }


public class iChessKing : Item { }


public class iTrap : Item { }


public class iCurseDoll : Item { }


public class iTruthOfTheUniverse : Item { }


public class iAntimatterCookie : Item { }





public class iRuneCube : Item { }


public class iCthulhuEye : Item { }


public class iDumbbell : Item { }

public class iBadgeGold : Item {
	private static readonly int TYPE_ID = typeof(iBadgeGold).AngeHash();
	[OnGameInitializeLater]
	public static void OnGameInitialize () {
		MiniGame.OnBadgeSpawn -= OnBadgeSpawn;
		MiniGame.OnBadgeSpawn += OnBadgeSpawn;
		static void OnBadgeSpawn (int quality) {
			if (quality >= 2) {
				ItemSystem.GiveItemTo(Player.Selecting.TypeID, TYPE_ID, 1);
			}
		}
	}
}

public class iBadgeIron : Item {
	private static readonly int TYPE_ID = typeof(iBadgeIron).AngeHash();
	[OnGameInitializeLater]
	public static void OnGameInitialize () {
		MiniGame.OnBadgeSpawn -= OnBadgeSpawn;
		MiniGame.OnBadgeSpawn += OnBadgeSpawn;
		static void OnBadgeSpawn (int quality) {
			if (quality <= 1) {
				ItemSystem.GiveItemTo(Player.Selecting.TypeID, TYPE_ID, 1);
			}
		}
	}
}


public class iHourglass : Item { }


public class iMap : Item { }


public class iCompass : Item { }


public class iBasketball : Item { }


public class iCuttingBoard : Item { }


public class iTombstone : Item { }


public class iFlag : Item { }


public class iToiletPaper : Item { }


public class iChristmasTree : Item { }


public class iDartBoard : Item { }


public class iAnvil : Item { }


public class iAntimatter : Item { }


public class iBowlingBall : Item { }


public class iRubberBall : Item { }


public class iSpikeBall : Item { }


public class iIronCage : Item { }


public class iExplosiveBarrel : Item { }


public class iTelescope : Item { }


public class iVinyl : Item { }


public class iLens : Item { }


public class iGlass : Item { }


public class iLegoBlockWhite : Item { }


public class iLegoBlockRed : Item { }


public class iPencil : Item { }


public class iBowTie : Item { }


public class iSpringIron : Item { }


public class iIronHook : Item { }


public class iPipeClay : Item { }


public class iConfetti : Item { }


public class iCrayon : Item { }


public class iBucketIron : Item { }


public class iRibbon : Item { }


public class iBottledWater : Item { }


public class iTrayWood : Item { }


public class iCapsule : Item { }


public class iRubberDuck : Item { }


public class iCrystalBall : Item { }


public class iCone : Item { }


public class iPhonograph : Item { }


public class iHorn : Item { }


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


public class iFabric : Item { }


public class iTorch : Item { }


public class iDrill : Item { }






public class iSoupCan : Item { }


public class iWheel : Item { }


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


public class iWatchLegend : Item { }


public class iTelescopeLegend : Item { }


public class iHourglassLegend : Item { }


public class iBegleriLegend : Item { }


public class iSpringLegend : Item { }


public class iGameboyLegend : Item { }


public class iGamepadLegend : Item { }


public class iPenLegend : Item { }


public class iNotepadLegend : Item { }


public class iHookLegend : Item { }


public class iHandFanLegend : Item { }


public class iKeyLegend : Item { }


public class iPipeLegend : Item { }


public class iCrossLegend : Item { }


public class iLeafLegend : Item { }


public class iBookRed : Item { }


public class iBookBlue : Item { }


public class iBookYellow : Item { }


public class iBookGreen : Item { }


public class iBook : Item { }


public class iMedalGold : Item { }


public class iMedalIron : Item { }


public class iCross : Item { }


public class iBell : Item { }


public class iFloppyDisk : Item { }


public class iTape : Item { }


public class iScroll : Item { }


public class iGiftBox : Item { }



public class iGemRed : Item { }



public class iGemBlue : Item { }



public class iGemOrange : Item { }



public class iGemGreen : Item { }


public class iGem : Item { }


public class iChain : Item { }


public class iChopsticks : Item { }


public class iRubyRed : Item { }


public class iRubyBlue : Item { }


public class iRubyOrange : Item { }


public class iRubyGreen : Item { }


public class iSpoonFork : Item { }


public class iLockIron : Item { }


public class iLockGold : Item { }


public class iTeapot : Item { }


public class iBolt : Item { }


public class iPokerCard : Item { }


public class iGamblingChip : Item { }


public class iTotemFire : Item { }


public class iTotemWater : Item { }


public class iTotemLightning : Item { }


public class iTotemPoison : Item { }


public class iDialogBox : Item { }


public class iPaper : Item { }


public class iItemCoin : Item { public override int MaxStackCount => 100; }


public class iHerb : Item { }


public class iScales : Item { }


public class iVolatilePotionRed : Item { }


public class iVolatilePotionBlue : Item { }


public class iVolatilePotionOrange : Item { }


public class iVolatilePotionGreen : Item { }


public class iVolatilePotionEmpty : Item { }


public class iRope : Item { }


public class iIronWire : Item { }


public class GoldWire : Item { }


public class iElectricWire : Item { }


public class iTreeBranchBundle : Item { }



public class iItemWoodBoard : Item { }


public class iTrophy : Item { }


public class iLantern : Item { }


public class iPotionRed : Item { }


public class iPotionBlue : Item { }


public class iPotionOrange : Item { }


public class iPotionGreen : Item { }


public class iPotionEmpty : Item { }


public class iBrokenBone : Item { }


public class iHandFan : Item { }



public class iLetter : Item { }


public class iMachineBeetle : Item { }


public class iBulb : Item { }


public class iFirecracker : Item { }


public class iDice : Item { }





public class iCharcoal : Item { }


public class iKeyGold : Item { }


public class iStonePolished : Item { }


public class iFlintPolished : Item { }


public class iComb : Item { }


public class iHandbag : Item { }


public class iFirstAidKit : Item { }



public class iSoul : Item { }


public class iProcessor : Item { }


public class iElectronicChip : Item { }


public class iToolbox : Item { }


public class iPaintBrush : Item { }


public class iWord : Item { }

public class iCursedSoul : Item { }
public class iFist : Item { }
public class iBeetle : Item { }
public class iGunpowder : Item { }
public class iMusicNote : Item { }
public class iNose : Item { }
public class iTooth : Item { }
public class iLeather : Item { }
public class iCottonBall : Item { }
public class iLove : Item { }
public class iMagnet : Item { }
public class iEyeBall : Item { }
public class iGourd : Item { }
public class iStar : Item { }
public class iCuteGhost : Item { }
public class iBoringGhost : Item { }
public class iTentacle : Item { }
public class iFlowerRed : Item { }
public class iFlowerPink : Item { }
public class iFlowerYellow : Item { }
public class iFlowerCyan : Item { }
public class iFlowerWhite : Item { }
public class iRuby : Item { }
public class iEar : Item { }
public class iClay : Item { }
public class iLeaf : Item {
	public override int MaxStackCount => 4096;
}
public class iSkull : Item { }
public class iABC : Item { }
public class iGoblinHead : Item { }
public class iIngotIron : Item { }
public class iIngotGold : Item { }
public class iTreeBranch : Item { }
public class iTreeTrunk : Item { }
public class iTreeStump : Item { }
public class iMeatBone : Item { }
public class iFishBone : Item { }
public class iPaw : Item { }
public class iItemSand : Item { }
public class iRuneFire : Item { }
public class iRuneWater : Item { }
public class iRuneLightning : Item { }
public class iRunePoison : Item { }
public class iKeyIron : Item { }
public class iStone : Item { }
public class iFlint : Item { }
public class iCthulhuMeat : Item { }