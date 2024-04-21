using System.Collections;
using System.Collections.Generic;


namespace AngeliA;

[ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iTreeTrunk), 1)]
public class iChessPawn : Item { }

[ItemCombination(typeof(iRuneFire), typeof(iTreeTrunk), typeof(iRuneLightning), 1)]
public class iChessKnight : Item { }

[ItemCombination(typeof(iRunePoison), typeof(iRuneFire), typeof(iTreeTrunk), 1)]
public class iChessBishop : Item { }

[ItemCombination(typeof(iRuneWater), typeof(iTreeTrunk), typeof(iRuneLightning), 1)]
public class iChessRook : Item { }

[ItemCombination(typeof(iRuneWater), typeof(iRunePoison), typeof(iTreeTrunk), 1)]
public class iChessQueen : Item { }

[ItemCombination(typeof(iRunePoison), typeof(iTreeTrunk), typeof(iRuneLightning), 1)]
public class iChessKing : Item { }

[ItemCombination(typeof(iSpikeBall), typeof(iIronCage), 1)]
public class iTrap : Item { }

[ItemCombination(typeof(iCottonBall), typeof(iFabric), typeof(iNeedle), 1)]
public class iCurseDoll : Item { }

[ItemCombination(typeof(iAntimatterCookie), typeof(iCthulhuEye), 1)]
public class iTruthOfTheUniverse : Item { }

[ItemCombination(typeof(iCookie), typeof(iAntimatter), 1)]
public class iAntimatterCookie : Item { }

[ItemCombination(typeof(iTotemFire), typeof(iDice), typeof(iTotemWater), typeof(iTotemLightning), 1)]
[ItemCombination(typeof(iTotemFire), typeof(iDice), typeof(iTotemPoison), typeof(iTotemWater), 1)]
[ItemCombination(typeof(iTotemFire), typeof(iDice), typeof(iTotemPoison), typeof(iTotemLightning), 1)]
[ItemCombination(typeof(iDice), typeof(iTotemPoison), typeof(iTotemWater), typeof(iTotemLightning), 1)]
public class iRuneCube : Item { }

[ItemCombination(typeof(iTentacle), typeof(iEyeBall), typeof(iCthulhuMeat), typeof(iCursedSoul), 1)]
public class iCthulhuEye : Item { }

[ItemCombination(typeof(iBowlingBall), typeof(iBowlingBall), typeof(iIngotIron), 1)]
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

[ItemCombination(typeof(iPotionEmpty), typeof(iPotionEmpty), typeof(iItemSand), 1)]
public class iHourglass : Item { }

[ItemCombination(typeof(iPaper), typeof(iCompass), typeof(iCrayon), 1)]
public class iMap : Item { }

[ItemCombination(typeof(iTrayWood), typeof(iMagnet), 1)]
public class iCompass : Item { }

[ItemCombination(typeof(iLeather), typeof(iLeather), 1)]
public class iBasketball : Item { }

[ItemCombination(typeof(iItemWoodBoard), typeof(iItemWoodBoard), typeof(iItemWoodBoard), 1)]
public class iCuttingBoard : Item { }

[ItemCombination(typeof(iStonePolished), typeof(iCross), typeof(iCuttingBoard), 1)]
public class iTombstone : Item { }

[ItemCombination(typeof(iFabric), typeof(iTreeBranch), 1)]
public class iFlag : Item { }

[ItemCombination(typeof(iRuneWater), typeof(iPaper), 1)]
public class iToiletPaper : Item { }

[ItemCombination(typeof(iLeaf), typeof(iTreeTrunk), typeof(iRibbon), 1)]
public class iChristmasTree : Item { }

[ItemCombination(typeof(iWheel), typeof(iCrayon), 1)]
public class iDartBoard : Item { }

[ItemCombination(typeof(iCharcoal), typeof(iIngotIron), typeof(iIngotIron), typeof(iIngotIron), 1)]
public class iAnvil : Item { }

[ItemCombination(typeof(iRuneCube), typeof(iCuteGhost), typeof(iPositronScanner), typeof(iLaptop), 1)]
public class iAntimatter : Item { }

[ItemCombination(typeof(iBasketball), typeof(iIngotIron), 1)]
public class iBowlingBall : Item { }

[ItemCombination(typeof(iVolatilePotionEmpty), typeof(iLegoBlockWhite), 1)]
public class iRubberBall : Item { }

[ItemCombination(typeof(iBowlingBall), typeof(iBolt), typeof(iBolt), typeof(iBolt), 1)]
public class iSpikeBall : Item { }

[ItemCombination(typeof(iIronWire), typeof(iLockIron), typeof(iCorrugatedBox), 1)]
public class iIronCage : Item { }

[ItemCombination(typeof(iBucketIron), typeof(iGunpowder), 1)]
public class iExplosiveBarrel : Item { }

[ItemCombination(typeof(iTreeStump), typeof(iLens), 1)]
public class iTelescope : Item { }

[ItemCombination(typeof(iRuneFire), typeof(iTrayWood), typeof(iMusicNote), typeof(iLegoBlockWhite), 1)]
public class iVinyl : Item { }

[ItemCombination(typeof(iRuneFire), typeof(iTrayWood), typeof(iGlass), 1)]
public class iLens : Item { }

[ItemCombination(typeof(iRuneFire), typeof(iItemSand), 1)]
public class iGlass : Item { }

[ItemCombination(typeof(iLegoBlockRed), typeof(iFlowerWhite), 1)]
public class iLegoBlockWhite : Item { }

[ItemCombination(typeof(iFlowerRed), typeof(iLegoBlockWhite), 1)]
public class iLegoBlockRed : Item { }

[ItemCombination(typeof(iCharcoal), typeof(iTreeBranch), 1)]
public class iPencil : Item { }

[ItemCombination(typeof(iRibbon), typeof(iRibbon), 1)]
public class iBowTie : Item { }

[ItemCombination(typeof(iIronWire), typeof(iBolt), 1)]
public class iSpringIron : Item { }

[ItemCombination(typeof(iIronWire), typeof(iIngotIron), 1)]
public class iIronHook : Item { }

[ItemCombination(typeof(iRuneFire), typeof(iHerb), typeof(iClay), 1)]
public class iPipeClay : Item { }

[ItemCombination(typeof(iFirecracker), typeof(iRibbon), 1)]
public class iConfetti : Item { }

[ItemCombination(typeof(iClay), typeof(iPencil), 1)]
public class iCrayon : Item { }

[ItemCombination(typeof(iIronWire), typeof(iIngotIron), typeof(iIngotIron), 1)]
public class iBucketIron : Item { }

[ItemCombination(typeof(iFabric), 4)]
public class iRibbon : Item { }

[ItemCombination(typeof(iRuneWater), typeof(iBucketIron), 1)]
public class iBottledWater : Item { }

[ItemCombination(typeof(iGourd), 4)]
public class iTrayWood : Item { }

[ItemCombination(typeof(iHerb), typeof(iLegoBlockRed), typeof(iLegoBlockWhite), 1)]
public class iCapsule : Item { }

[ItemCombination(typeof(iRubberBall), typeof(iRubberBall), 1)]
public class iRubberDuck : Item { }

[ItemCombination(typeof(iGemGreen), typeof(iEyeBall), typeof(iCuteGhost), typeof(iGemBlue), 1)]
public class iCrystalBall : Item { }

[ItemCombination(typeof(iLegoBlockRed), typeof(iLegoBlockRed), typeof(iLegoBlockWhite), typeof(iLegoBlockWhite), 1)]
public class iCone : Item { }

[ItemCombination(typeof(iItemWoodBoard), typeof(iMusicNote), typeof(iHorn), typeof(iVinyl), 1)]
public class iPhonograph : Item { }

[ItemCombination(typeof(iMusicNote), typeof(iMeatBone), 1)]
public class iHorn : Item { }

[ItemCombination(typeof(iMusicNote), typeof(iHorn), typeof(iButton), 1)]
public class iCornet : Item { }

[ItemCombination(typeof(iMusicNote), typeof(iButton), typeof(iCornet), 1)]
public class iSaxophone : Item { }

[ItemCombination(typeof(iIronWire), typeof(iMusicNote), typeof(iGourd), 1)]
public class iGuitar : Item { }

[ItemCombination(typeof(iBucketIron), typeof(iLeather), typeof(iMusicNote), 1)]
public class iDrum : Item { }

[ItemCombination(typeof(iMusicNote), typeof(iClay), 1)]
public class iOcarina : Item { }

[ItemCombination(typeof(iButton), typeof(iLegoBlockWhite), typeof(iBattery), 1)]
public class iGamepad : Item { }

[ItemCombination(typeof(iButton), typeof(iButton), typeof(iLegoBlockWhite), 1)]
public class iKeyboard : Item { }

[ItemCombination(typeof(iGlass), typeof(iGlass), typeof(iLegoBlockWhite), typeof(iLegoBlockWhite), 1)]
public class iMonitor : Item { }

[ItemCombination(typeof(iWheel), typeof(iIngotIron), typeof(iIngotIron), typeof(iIngotIron), 1)]
public class iPropeller : Item { }

[ItemCombination(typeof(iLegoBlockRed), typeof(iLegoBlockWhite), 1)]
public class iButton : Item { }

[ItemCombination(typeof(iElectricWire), typeof(iPotatoCut), 1)]
[ItemCombination(typeof(iElectricWire), typeof(iLemonCut), 1)]
[ItemCombination(typeof(iElectricWire), typeof(iLimeCut), 1)]
public class iBattery : Item { }

[ItemCombination(typeof(iCottonBall), typeof(iRope), 1)]
public class iYarn : Item { }

[ItemCombination(typeof(iCottonBall), typeof(iPaw), typeof(iLove), typeof(iFabric), 1)]
public class iTeddyBearDoll : Item { }

[ItemCombination(typeof(iLove), typeof(iBowTie), typeof(iLegoBlockWhite), 1)]
public class iBarbieDoll : Item { }

[ItemCombination(typeof(iYarn), typeof(iYarn), typeof(iYarn), typeof(iYarn), 1)]
public class iFabric : Item { }

[ItemCombination(typeof(iRuneFire), typeof(iTreeBranch), 1)]
public class iTorch : Item { }

[ItemCombination(typeof(iBolt), typeof(iIngotIron), 1)]
public class iDrill : Item { }

[ItemCombination(typeof(iBeefCooked), typeof(iIngotIron), 1)]
[ItemCombination(typeof(iIngotIron), typeof(iChickenLegCooked), 1)]
[ItemCombination(typeof(iChickenWingCooked), typeof(iIngotIron), 1)]
[ItemCombination(typeof(iPorkCooked), typeof(iIngotIron), 1)]
[ItemCombination(typeof(iIngotIron), typeof(iMeatballCooked), 1)]
public class iSoupCan : Item { }

[ItemCombination(typeof(iTreeStump), typeof(iRubberBall), typeof(iIngotIron), 1)]
public class iWheel : Item { }

[ItemCombination(typeof(iItemWoodBoard), typeof(iItemWoodBoard), typeof(iPaper), typeof(iPaper), 1)]
public class iCorrugatedBox : Item { }

[ItemCombination(typeof(iCottonBall), typeof(iFabric), 1)]
public class iPillow : Item { }

[ItemCombination(typeof(iGlass), typeof(iIngotGold), 1)]
public class iMirror : Item { }

[ItemCombination(typeof(iMonitor), typeof(iGamepad), typeof(iProcessor), 1)]
public class iGameConsole : Item { }

[ItemCombination(typeof(iHourglass), typeof(iLegoBlockWhite), typeof(iBattery), 1)]
public class iWatch : Item { }

[ItemCombination(typeof(iDrill), typeof(iLegoBlockWhite), typeof(iBattery), 1)]
public class iElectricDrill : Item { }

[ItemCombination(typeof(iPokerCard), typeof(iGamepad), typeof(iDice), 1)]
public class iSlotMachine : Item { }

[ItemCombination(typeof(iGuitar), typeof(iBattery), 1)]
public class iBass : Item { }

[ItemCombination(typeof(iBell), typeof(iBell), typeof(iWatch), 1)]
public class iAlarmClock : Item { }

[ItemCombination(typeof(iElectricDrill), typeof(iPropeller), 1)]
public class iFan : Item { }

[ItemCombination(typeof(iMagnet), typeof(iLegoBlockWhite), typeof(iElectricWire), typeof(iBattery), 1)]
public class iRadio : Item { }

[ItemCombination(typeof(iMusicNote), typeof(iBattery), typeof(iKeyboard), 1)]
public class iPianoKeyboard : Item { }

[ItemCombination(typeof(iBulb), typeof(iLens), typeof(iButton), typeof(iLegoBlockWhite), 1)]
public class iCamera : Item { }

[ItemCombination(typeof(iWheel), typeof(iWheel), typeof(iCamera), typeof(iBattery), 1)]
public class iVideoRecorder : Item { }

[ItemCombination(typeof(iVideoRecorder), typeof(iSlotMachine), typeof(iPhone), typeof(iRadio), 1)]
public class iPositronScanner : Item { }

[ItemCombination(typeof(iRuneFire), typeof(iButton), typeof(iLegoBlockWhite), 1)]
public class iLighter : Item { }

[ItemCombination(typeof(iMonitor), typeof(iProcessor), typeof(iBattery), typeof(iKeyboard), 1)]
public class iLaptop : Item { }

[ItemCombination(typeof(iMonitor), typeof(iProcessor), typeof(iBattery), 1)]
public class iPhone : Item { }

[ItemCombination(typeof(iWatch), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
public class iWatchLegend : Item { }

[ItemCombination(typeof(iTelescope), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
public class iTelescopeLegend : Item { }

[ItemCombination(typeof(iHourglass), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
public class iHourglassLegend : Item { }

[ItemCombination(typeof(iRope), typeof(iBell), typeof(iBell), typeof(iIngotGold), 1)]
public class iBegleriLegend : Item { }

[ItemCombination(typeof(iSpringIron), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
public class iSpringLegend : Item { }

[ItemCombination(typeof(iGameConsole), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
public class iGameboyLegend : Item { }

[ItemCombination(typeof(iGamepad), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
public class iGamepadLegend : Item { }

[ItemCombination(typeof(iPencil), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
public class iPenLegend : Item { }

[ItemCombination(typeof(iBook), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
public class iNotepadLegend : Item { }

[ItemCombination(typeof(iIronHook), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
public class iHookLegend : Item { }

[ItemCombination(typeof(iHandFan), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
public class iHandFanLegend : Item { }

[ItemCombination(typeof(iGemGreen), typeof(iKeyGold), typeof(iIngotGold), typeof(iGemBlue), 1)]
public class iKeyLegend : Item { }

[ItemCombination(typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), typeof(iPipeClay), 1)]
public class iPipeLegend : Item { }

[ItemCombination(typeof(iCross), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
public class iCrossLegend : Item { }

[ItemCombination(typeof(iLeaf), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
public class iLeafLegend : Item { }

[ItemCombination(typeof(iRuneFire), typeof(iBook), 1)]
public class iBookRed : Item { }

[ItemCombination(typeof(iRuneWater), typeof(iBook), 1)]
public class iBookBlue : Item { }

[ItemCombination(typeof(iBook), typeof(iRuneLightning), 1)]
public class iBookYellow : Item { }

[ItemCombination(typeof(iRunePoison), typeof(iBook), 1)]
public class iBookGreen : Item { }

[ItemCombination(typeof(iPaper), typeof(iPaper), typeof(iPaper), typeof(iPaper), 1)]
public class iBook : Item { }

[ItemCombination(typeof(iRibbon), typeof(iIngotGold), 1)]
public class iMedalGold : Item { }

[ItemCombination(typeof(iRibbon), typeof(iIngotIron), 1)]
public class iMedalIron : Item { }

[ItemCombination(typeof(iItemWoodBoard), typeof(iItemWoodBoard), 1)]
public class iCross : Item { }

[ItemCombination(typeof(iMusicNote), typeof(iIngotGold), 1)]
public class iBell : Item { }

[ItemCombination(typeof(iLetter), typeof(iMagnet), typeof(iElectricWire), 1)]
public class iFloppyDisk : Item { }

[ItemCombination(typeof(iMusicNote), typeof(iRibbon), typeof(iMagnet), 1)]
public class iTape : Item { }

[ItemCombination(typeof(iPaper), typeof(iPaper), typeof(iPaper), typeof(iTreeBranch), 1)]
public class iScroll : Item { }

[ItemCombination(typeof(iCorrugatedBox), typeof(iRibbon), 1)]
public class iGiftBox : Item { }

[ItemCombination(typeof(iRuneFire), typeof(iGem), 1)]
[ItemCombination(typeof(iRubyRed), typeof(iRubyRed), typeof(iRubyRed), typeof(iRubyRed), 1)]
public class iGemRed : Item { }

[ItemCombination(typeof(iRuneWater), typeof(iGem), 1)]
[ItemCombination(typeof(iRubyBlue), typeof(iRubyBlue), typeof(iRubyBlue), typeof(iRubyBlue), 1)]
public class iGemBlue : Item { }

[ItemCombination(typeof(iGem), typeof(iRuneLightning), 1)]
[ItemCombination(typeof(iRubyOrange), typeof(iRubyOrange), typeof(iRubyOrange), typeof(iRubyOrange), 1)]
public class iGemOrange : Item { }

[ItemCombination(typeof(iRunePoison), typeof(iGem), 1)]
[ItemCombination(typeof(iRubyGreen), typeof(iRubyGreen), typeof(iRubyGreen), typeof(iRubyGreen), 1)]
public class iGemGreen : Item { }

[ItemCombination(typeof(iRuby), typeof(iRuby), typeof(iRuby), typeof(iRuby), 1)]
public class iGem : Item { }

[ItemCombination(typeof(iIronWire), typeof(iIronWire), typeof(iIronWire), typeof(iIronWire), 1)]
public class iChain : Item { }

[ItemCombination(typeof(iTreeBranch), typeof(iTreeBranch), 1)]
public class iChopsticks : Item { }

[ItemCombination(typeof(iRuneFire), typeof(iRuby), 1)]
public class iRubyRed : Item { }

[ItemCombination(typeof(iRuneWater), typeof(iRuby), 1)]
public class iRubyBlue : Item { }

[ItemCombination(typeof(iRuby), typeof(iRuneLightning), 1)]
public class iRubyOrange : Item { }

[ItemCombination(typeof(iRunePoison), typeof(iRuby), 1)]
public class iRubyGreen : Item { }

[ItemCombination(typeof(iMeatBone), typeof(iIngotIron), 1)]
public class iSpoonFork : Item { }

[ItemCombination(typeof(iKeyIron), typeof(iIngotIron), 1)]
public class iLockIron : Item { }

[ItemCombination(typeof(iKeyGold), typeof(iIngotGold), 1)]
public class iLockGold : Item { }

[ItemCombination(typeof(iRuneWater), typeof(iLeaf), typeof(iClay), 1)]
public class iTeapot : Item { }

[ItemCombination(typeof(iIngotIron), 4)]
public class iBolt : Item { }

[ItemCombination(typeof(iDice), typeof(iPaper), 1)]
public class iPokerCard : Item { }

[ItemCombination(typeof(iItemCoin), typeof(iDice), 1)]
public class iGamblingChip : Item { }

[ItemCombination(typeof(iRuneFire), typeof(iSkull), 1)]
public class iTotemFire : Item { }

[ItemCombination(typeof(iRuneWater), typeof(iSkull), 1)]
public class iTotemWater : Item { }

[ItemCombination(typeof(iSkull), typeof(iRuneLightning), 1)]
public class iTotemLightning : Item { }

[ItemCombination(typeof(iRunePoison), typeof(iSkull), 1)]
public class iTotemPoison : Item { }

[ItemCombination(typeof(iLetter), typeof(iElectricWire), 1)]
public class iDialogBox : Item { }

[ItemCombination(typeof(iRuneWater), typeof(iTreeBranchBundle), 1)]
public class iPaper : Item { }

[ItemCombination(typeof(iIngotGold), 100)]
public class iItemCoin : Item { public override int MaxStackCount => 100; }

[ItemCombination(typeof(iRunePoison), typeof(iLeaf), 1)]
public class iHerb : Item { }

[ItemCombination(typeof(iTrayWood), typeof(iTrayWood), typeof(iRope), typeof(iTreeBranch), 1)]
public class iScales : Item { }

[ItemCombination(typeof(iVolatilePotionEmpty), typeof(iRuneFire), 1)]
public class iVolatilePotionRed : Item { }

[ItemCombination(typeof(iRuneWater), typeof(iVolatilePotionEmpty), 1)]
public class iVolatilePotionBlue : Item { }

[ItemCombination(typeof(iVolatilePotionEmpty), typeof(iRuneLightning), 1)]
public class iVolatilePotionOrange : Item { }

[ItemCombination(typeof(iRunePoison), typeof(iVolatilePotionEmpty), 1)]
public class iVolatilePotionGreen : Item { }

[ItemCombination(typeof(iGlass), typeof(iGlass), 1)]
public class iVolatilePotionEmpty : Item { }

[ItemCombination(typeof(iRuneWater), typeof(iTreeBranch), 4)]
public class iRope : Item { }

[ItemCombination(typeof(iRope), typeof(iIngotIron), 1)]
public class iIronWire : Item { }

[ItemCombination(typeof(iIronWire), typeof(iIngotGold), 1)]
public class GoldWire : Item { }

[ItemCombination(typeof(iIronWire), typeof(iRuneLightning), 1)]
public class iElectricWire : Item { }

[ItemCombination(typeof(iRope), typeof(iTreeBranch), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
public class iTreeBranchBundle : Item { }

[ItemCombination(typeof(iTreeTrunk), typeof(iTreeTrunk), typeof(iTreeTrunk), 1)]
[ItemCombination(typeof(iTreeStump), typeof(iTreeStump), 1)]
public class iItemWoodBoard : Item { }

[ItemCombination(typeof(iMedalGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
public class iTrophy : Item { }

[ItemCombination(typeof(iPotionEmpty), typeof(iRuneFire), typeof(iIronWire), 1)]
public class iLantern : Item { }

[ItemCombination(typeof(iPotionEmpty), typeof(iRuneFire), 1)]
public class iPotionRed : Item { }

[ItemCombination(typeof(iRuneWater), typeof(iPotionEmpty), 1)]
public class iPotionBlue : Item { }

[ItemCombination(typeof(iPotionEmpty), typeof(iRuneLightning), 1)]
public class iPotionOrange : Item { }

[ItemCombination(typeof(iPotionEmpty), typeof(iRunePoison), 1)]
public class iPotionGreen : Item { }

[ItemCombination(typeof(iGlass), typeof(iGlass), typeof(iTreeBranch), 1)]
public class iPotionEmpty : Item { }

[ItemCombination(typeof(iMeatBone), 2)]
public class iBrokenBone : Item { }

[ItemCombination(typeof(iPaper), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
public class iHandFan : Item { }

[ItemCombination(typeof(iWord), typeof(iPaper), 1)]
[ItemCombination(typeof(iABC), typeof(iABC), typeof(iPaper), 1)]
public class iLetter : Item { }

[ItemCombination(typeof(iProcessor), typeof(iBeetle), typeof(iElectricWire), typeof(iBattery), 1)]
public class iMachineBeetle : Item { }

[ItemCombination(typeof(iPotionEmpty), typeof(iElectricWire), typeof(iRuneLightning), 1)]
public class iBulb : Item { }

[ItemCombination(typeof(iPaper), typeof(iGunpowder), 1)]
public class iFirecracker : Item { }

[ItemCombination(typeof(iPencil), typeof(iLegoBlockWhite), 1)]
public class iDice : Item { }

[ItemCombination(typeof(iTreeBranchBundle), typeof(iRuneFire), 3)]
[ItemCombination(typeof(iRuneFire), typeof(iTreeTrunk), 6)]
[ItemCombination(typeof(iTreeStump), typeof(iRuneFire), 9)]
[ItemCombination(typeof(iItemWoodBoard), typeof(iRuneFire), 12)]
public class iCharcoal : Item { }

[ItemCombination(typeof(iKeyIron), typeof(iIngotGold), 1)]
public class iKeyGold : Item { }

[ItemCombination(typeof(iStone), typeof(iStone), 1)]
public class iStonePolished : Item { }

[ItemCombination(typeof(iFlint), typeof(iFlint), 1)]
public class iFlintPolished : Item { }

[ItemCombination(typeof(iFishBone), typeof(iTreeBranch), 1)]
public class iComb : Item { }

[ItemCombination(typeof(iLeather), typeof(iLeather), typeof(iLeather), 1)]
public class iHandbag : Item { }

[ItemCombination(typeof(iHandbag), typeof(iCapsule), 1)]
public class iFirstAidKit : Item { }

[ItemCombination(typeof(iCuteGhost), 4)]
[ItemCombination(typeof(iBoringGhost), 2)]
public class iSoul : Item { }

[ItemCombination(typeof(iElectronicChip), typeof(iBookBlue), typeof(iElectricWire), typeof(iIngotIron), 1)]
public class iProcessor : Item { }

[ItemCombination(typeof(iBookYellow), typeof(iIngotIron), typeof(iElectricWire), typeof(iFloppyDisk), 1)]
public class iElectronicChip : Item { }

[ItemCombination(typeof(iHandbag), typeof(iBolt), 1)]
public class iToolbox : Item { }

[ItemCombination(typeof(iPencil), typeof(iCottonBall), 1)]
public class iPaintBrush : Item { }

[ItemCombination(typeof(iABC), typeof(iABC), 1)]
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