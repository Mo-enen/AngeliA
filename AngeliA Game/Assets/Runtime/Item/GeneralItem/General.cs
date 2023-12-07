using System.Collections;
using System.Collections.Generic;
using System.Text;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iRuneFire), typeof(iTreeTrunk), 1)]
	public class iChessPawn : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iTreeTrunk), typeof(iRuneLightning), 1)]
	public class iChessKnight : Item { }

	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iRuneFire), typeof(iTreeTrunk), 1)]
	public class iChessBishop : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iTreeTrunk), typeof(iRuneLightning), 1)]
	public class iChessRook : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iRunePoison), typeof(iTreeTrunk), 1)]
	public class iChessQueen : Item { }

	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iTreeTrunk), typeof(iRuneLightning), 1)]
	public class iChessKing : Item { }

	[EntityAttribute.ItemCombination(typeof(iSpikeBall), typeof(iIronCage), 1)]
	public class iTrap : Item { }

	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iFabric), typeof(iNeedle), 1)]
	public class iCurseDoll : Item { }

	[EntityAttribute.ItemCombination(typeof(iAntimatterCookie), typeof(iCthulhuEye), typeof(iCthulhuMeat), 1)]
	public class iTruthOfTheUniverse : Item { }

	[EntityAttribute.ItemCombination(typeof(iCookie), typeof(iAntimatter), 1)]
	public class iAntimatterCookie : Item { }

	[EntityAttribute.ItemCombination(typeof(iTotemFire), typeof(iDice), typeof(iTotemWater), typeof(iTotemLightning), 1)]
	[EntityAttribute.ItemCombination(typeof(iTotemFire), typeof(iDice), typeof(iTotemPoison), typeof(iTotemWater), 1)]
	[EntityAttribute.ItemCombination(typeof(iTotemFire), typeof(iDice), typeof(iTotemPoison), typeof(iTotemLightning), 1)]
	[EntityAttribute.ItemCombination(typeof(iDice), typeof(iTotemPoison), typeof(iTotemWater), typeof(iTotemLightning), 1)]
	public class iRuneCube : Item { }

	[EntityAttribute.ItemCombination(typeof(iTentacle), typeof(iEyeBall), typeof(iCthulhuMeat), typeof(iBookGreen), 1)]
	public class iCthulhuEye : Item { }

	[EntityAttribute.ItemCombination(typeof(iBowlingBall), typeof(iBowlingBall), typeof(iIngotIron), 1)]
	public class iDumbbell : Item { }

	public class iBadgeGold : Item {
		private static readonly int TYPE_ID = typeof(iBadgeGold).AngeHash();
		[OnGameInitialize]
		public static void OnGameInitialize () {
			MiniGame.OnBadgeSpawn -= OnBadgeSpawn;
			MiniGame.OnBadgeSpawn += OnBadgeSpawn;
			static void OnBadgeSpawn (int quality) {
				if (quality >= 2) {
					ItemSystem.GiveItemToPlayer(TYPE_ID, 1);
				}
			}
		}
	}

	public class iBadgeIron : Item {
		private static readonly int TYPE_ID = typeof(iBadgeIron).AngeHash();
		[OnGameInitialize]
		public static void OnGameInitialize () {
			MiniGame.OnBadgeSpawn -= OnBadgeSpawn;
			MiniGame.OnBadgeSpawn += OnBadgeSpawn;
			static void OnBadgeSpawn (int quality) {
				if (quality <= 1) {
					ItemSystem.GiveItemToPlayer(TYPE_ID, 1);
				}
			}
		}
	}

	[EntityAttribute.ItemCombination(typeof(iPotionEmpty), typeof(iPotionEmpty), typeof(iItemSand), 1)]
	public class iHourglass : Item { }

	[EntityAttribute.ItemCombination(typeof(iPaper), typeof(iCompass), typeof(iCrayon), 1)]
	public class iMap : Item { }

	[EntityAttribute.ItemCombination(typeof(iTrayWood), typeof(iMagnet), 1)]
	public class iCompass : Item { }

	[EntityAttribute.ItemCombination(typeof(iLeather), typeof(iLeather), 1)]
	public class iBasketball : Item { }

	[EntityAttribute.ItemCombination(typeof(iItemWoodBoard), typeof(iItemWoodBoard), typeof(iItemWoodBoard), 1)]
	public class iCuttingBoard : Item { }

	[EntityAttribute.ItemCombination(typeof(iStonePolished), typeof(iCross), typeof(iCuttingBoard), 1)]
	public class iTombstone : Item { }

	[EntityAttribute.ItemCombination(typeof(iFabric), typeof(iTreeBranch), 1)]
	public class iFlag : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iPaper), 1)]
	public class iToiletPaper : Item { }

	[EntityAttribute.ItemCombination(typeof(iLeaf), typeof(iTreeTrunk), typeof(iRibbon), 1)]
	public class iChristmasTree : Item { }

	[EntityAttribute.ItemCombination(typeof(iWheel), typeof(iCrayon), 1)]
	public class iDartBoard : Item { }

	[EntityAttribute.ItemCombination(typeof(iCharcoal), typeof(iIngotIron), typeof(iIngotIron), typeof(iIngotIron), 1)]
	public class iAnvil : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneCube), typeof(iCuteGhost), typeof(iPositronScanner), typeof(iLaptop), 1)]
	public class iAntimatter : Item { }

	[EntityAttribute.ItemCombination(typeof(iBasketball), typeof(iIngotIron), 1)]
	public class iBowlingBall : Item { }

	[EntityAttribute.ItemCombination(typeof(iVolatilePotionEmpty), typeof(iLegoBlockWhite), 1)]
	public class iRubberBall : Item { }

	[EntityAttribute.ItemCombination(typeof(iBowlingBall), typeof(iBolt), typeof(iBolt), typeof(iBolt), 1)]
	public class iSpikeBall : Item { }

	[EntityAttribute.ItemCombination(typeof(iIronWire), typeof(iLockIron), typeof(iCorrugatedBox), 1)]
	public class iIronCage : Item { }

	[EntityAttribute.ItemCombination(typeof(iBucketIron), typeof(iGunpowder), 1)]
	public class iExplosiveBarrel : Item { }

	[EntityAttribute.ItemCombination(typeof(iTreeStump), typeof(iLens), 1)]
	public class iTelescope : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iTrayWood), typeof(iMusicNote), typeof(iLegoBlockWhite), 1)]
	public class iVinyl : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iTrayWood), typeof(iGlass), 1)]
	public class iLens : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iItemSand), 1)]
	public class iGlass : Item { }

	[EntityAttribute.ItemCombination(typeof(iLegoBlockRed), typeof(iFlowerWhite), 1)]
	public class iLegoBlockWhite : Item { }

	[EntityAttribute.ItemCombination(typeof(iFlowerRed), typeof(iLegoBlockWhite), 1)]
	public class iLegoBlockRed : Item { }

	[EntityAttribute.ItemCombination(typeof(iCharcoal), typeof(iTreeBranch), 1)]
	public class iPencil : Item { }

	[EntityAttribute.ItemCombination(typeof(iRibbon), typeof(iRibbon), 1)]
	public class iBowTie : Item { }

	[EntityAttribute.ItemCombination(typeof(iIronWire), typeof(iBolt), 1)]
	public class iSpringIron : Item { }

	[EntityAttribute.ItemCombination(typeof(iIronWire), typeof(iIngotIron), 1)]
	public class iIronHook : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iHerb), typeof(iClay), 1)]
	public class iPipeClay : Item { }

	[EntityAttribute.ItemCombination(typeof(iFirecracker), typeof(iRibbon), 1)]
	public class iConfetti : Item { }

	[EntityAttribute.ItemCombination(typeof(iClay), typeof(iPencil), 1)]
	public class iCrayon : Item { }

	[EntityAttribute.ItemCombination(typeof(iIronWire), typeof(iIngotIron), typeof(iIngotIron), 1)]
	public class iBucketIron : Item { }

	[EntityAttribute.ItemCombination(typeof(iFabric), 4)]
	public class iRibbon : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iBucketIron), 1)]
	public class iBottledWater : Item { }

	[EntityAttribute.ItemCombination(typeof(iGourd), 4)]
	public class iTrayWood : Item { }

	[EntityAttribute.ItemCombination(typeof(iHerb), typeof(iLegoBlockRed), typeof(iLegoBlockWhite), 1)]
	public class iCapsule : Item { }

	[EntityAttribute.ItemCombination(typeof(iRubberBall), typeof(iRubberBall), 1)]
	public class iRubberDuck : Item { }

	[EntityAttribute.ItemCombination(typeof(iGemGreen), typeof(iEyeBall), typeof(iCuteGhost), typeof(iGemBlue), 1)]
	public class iCrystalBall : Item { }

	[EntityAttribute.ItemCombination(typeof(iLegoBlockRed), typeof(iLegoBlockRed), typeof(iLegoBlockWhite), typeof(iLegoBlockWhite), 1)]
	public class iCone : Item { }

	[EntityAttribute.ItemCombination(typeof(iItemWoodBoard), typeof(iMusicNote), typeof(iHorn), typeof(iVinyl), 1)]
	public class iPhonograph : Item { }

	[EntityAttribute.ItemCombination(typeof(iMusicNote), typeof(iMeatBone), 1)]
	public class iHorn : Item { }

	[EntityAttribute.ItemCombination(typeof(iMusicNote), typeof(iHorn), typeof(iButton), 1)]
	public class iCornet : Item { }

	[EntityAttribute.ItemCombination(typeof(iMusicNote), typeof(iButton), typeof(iCornet), 1)]
	public class iSaxophone : Item { }

	[EntityAttribute.ItemCombination(typeof(iIronWire), typeof(iMusicNote), typeof(iGourd), 1)]
	public class iGuitar : Item { }

	[EntityAttribute.ItemCombination(typeof(iBucketIron), typeof(iLeather), typeof(iMusicNote), 1)]
	public class iDrum : Item { }

	[EntityAttribute.ItemCombination(typeof(iMusicNote), typeof(iClay), 1)]
	public class iOcarina : Item { }

	[EntityAttribute.ItemCombination(typeof(iButton), typeof(iLegoBlockWhite), typeof(iBattery), 1)]
	public class iGamepad : Item { }

	[EntityAttribute.ItemCombination(typeof(iButton), typeof(iButton), typeof(iLegoBlockWhite), 1)]
	public class iKeyboard : Item { }

	[EntityAttribute.ItemCombination(typeof(iGlass), typeof(iGlass), typeof(iLegoBlockWhite), typeof(iLegoBlockWhite), 1)]
	public class iMonitor : Item { }

	[EntityAttribute.ItemCombination(typeof(iWheel), typeof(iIngotIron), typeof(iIngotIron), typeof(iIngotIron), 1)]
	public class iPropeller : Item { }

	[EntityAttribute.ItemCombination(typeof(iLegoBlockRed), typeof(iLegoBlockWhite), 1)]
	public class iButton : Item { }

	[EntityAttribute.ItemCombination(typeof(iElectricWire), typeof(iPotatoCut), 1)]
	[EntityAttribute.ItemCombination(typeof(iElectricWire), typeof(iLemonCut), 1)]
	[EntityAttribute.ItemCombination(typeof(iElectricWire), typeof(iLimeCut), 1)]
	public class iBattery : Item { }

	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iRope), 1)]
	public class iYarn : Item { }

	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iPaw), typeof(iLove), typeof(iFabric), 1)]
	public class iTeddyBearDoll : Item { }

	[EntityAttribute.ItemCombination(typeof(iLove), typeof(iBowTie), typeof(iLegoBlockWhite), 1)]
	public class iBarbieDoll : Item { }

	[EntityAttribute.ItemCombination(typeof(iYarn), typeof(iYarn), typeof(iYarn), typeof(iYarn), 1)]
	public class iFabric : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iTreeBranch), 1)]
	public class iTorch : Item { }

	[EntityAttribute.ItemCombination(typeof(iBolt), typeof(iIngotIron), 1)]
	public class iDrill : Item { }

	[EntityAttribute.ItemCombination(typeof(iBeefCooked), typeof(iIngotIron), 1)]
	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iChickenLegCooked), 1)]
	[EntityAttribute.ItemCombination(typeof(iChickenWingCooked), typeof(iIngotIron), 1)]
	[EntityAttribute.ItemCombination(typeof(iPorkCooked), typeof(iIngotIron), 1)]
	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iMeatballCooked), 1)]
	public class iSoupCan : Item { }

	[EntityAttribute.ItemCombination(typeof(iTreeStump), typeof(iRubberBall), typeof(iIngotIron), 1)]
	public class iWheel : Item { }

	[EntityAttribute.ItemCombination(typeof(iItemWoodBoard), typeof(iItemWoodBoard), typeof(iPaper), typeof(iPaper), 1)]
	public class iCorrugatedBox : Item { }

	[EntityAttribute.ItemCombination(typeof(iCottonBall), typeof(iFabric), 1)]
	public class iPillow : Item { }

	[EntityAttribute.ItemCombination(typeof(iGlass), typeof(iIngotGold), 1)]
	public class iMirror : Item { }

	[EntityAttribute.ItemCombination(typeof(iMonitor), typeof(iGamepad), typeof(iProcessor), 1)]
	public class iGameConsole : Item { }

	[EntityAttribute.ItemCombination(typeof(iHourglass), typeof(iLegoBlockWhite), typeof(iBattery), 1)]
	public class iWatch : Item { }

	[EntityAttribute.ItemCombination(typeof(iDrill), typeof(iLegoBlockWhite), typeof(iBattery), 1)]
	public class iElectricDrill : Item { }

	[EntityAttribute.ItemCombination(typeof(iPokerCard), typeof(iGamepad), typeof(iDice), 1)]
	public class iSlotMachine : Item { }

	[EntityAttribute.ItemCombination(typeof(iGuitar), typeof(iBattery), 1)]
	public class iBass : Item { }

	[EntityAttribute.ItemCombination(typeof(iBell), typeof(iBell), typeof(iWatch), 1)]
	public class iAlarmClock : Item { }

	[EntityAttribute.ItemCombination(typeof(iElectricDrill), typeof(iPropeller), 1)]
	public class iFan : Item { }

	[EntityAttribute.ItemCombination(typeof(iMagnet), typeof(iLegoBlockWhite), typeof(iElectricWire), typeof(iBattery), 1)]
	public class iRadio : Item { }

	[EntityAttribute.ItemCombination(typeof(iMusicNote), typeof(iBattery), typeof(iKeyboard), 1)]
	public class iPianoKeyboard : Item { }

	[EntityAttribute.ItemCombination(typeof(iBulb), typeof(iLens), typeof(iButton), typeof(iLegoBlockWhite), 1)]
	public class iCamera : Item { }

	[EntityAttribute.ItemCombination(typeof(iWheel), typeof(iWheel), typeof(iCamera), typeof(iBattery), 1)]
	public class iVideoRecorder : Item { }

	[EntityAttribute.ItemCombination(typeof(iVideoRecorder), typeof(iSlotMachine), typeof(iPhone), typeof(iRadio), 1)]
	public class iPositronScanner : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iButton), typeof(iLegoBlockWhite), 1)]
	public class iLighter : Item { }

	[EntityAttribute.ItemCombination(typeof(iMonitor), typeof(iProcessor), typeof(iBattery), typeof(iKeyboard), 1)]
	public class iLaptop : Item { }

	[EntityAttribute.ItemCombination(typeof(iMonitor), typeof(iProcessor), typeof(iBattery), 1)]
	public class iPhone : Item { }

	[EntityAttribute.ItemCombination(typeof(iWatch), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
	public class iWatchLegend : Item { }

	[EntityAttribute.ItemCombination(typeof(iTelescope), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
	public class iTelescopeLegend : Item { }

	[EntityAttribute.ItemCombination(typeof(iHourglass), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
	public class iHourglassLegend : Item { }

	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iBell), typeof(iBell), typeof(iIngotGold), 1)]
	public class iBegleriLegend : Item { }

	[EntityAttribute.ItemCombination(typeof(iSpringIron), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
	public class iSpringLegend : Item { }

	[EntityAttribute.ItemCombination(typeof(iGameConsole), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
	public class iGameboyLegend : Item { }

	[EntityAttribute.ItemCombination(typeof(iGamepad), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
	public class iGamepadLegend : Item { }

	[EntityAttribute.ItemCombination(typeof(iPencil), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
	public class iPenLegend : Item { }

	[EntityAttribute.ItemCombination(typeof(iBook), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
	public class iNotepadLegend : Item { }

	[EntityAttribute.ItemCombination(typeof(iIronHook), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
	public class iHookLegend : Item { }

	[EntityAttribute.ItemCombination(typeof(iHandFan), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
	public class iHandFanLegend : Item { }

	[EntityAttribute.ItemCombination(typeof(iGemGreen), typeof(iKeyGold), typeof(iIngotGold), typeof(iGemBlue), 1)]
	public class iKeyLegend : Item { }

	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), typeof(iPipeClay), 1)]
	public class iPipeLegend : Item { }

	[EntityAttribute.ItemCombination(typeof(iCross), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
	public class iCrossLegend : Item { }

	[EntityAttribute.ItemCombination(typeof(iLeaf), typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
	public class iLeafLegend : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iBook), 1)]
	public class iBookRed : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iBook), 1)]
	public class iBookBlue : Item { }

	[EntityAttribute.ItemCombination(typeof(iBook), typeof(iRuneLightning), 1)]
	public class iBookYellow : Item { }

	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iBook), 1)]
	public class iBookGreen : Item { }

	[EntityAttribute.ItemCombination(typeof(iPaper), typeof(iPaper), typeof(iPaper), typeof(iPaper), 1)]
	public class iBook : Item { }

	[EntityAttribute.ItemCombination(typeof(iRibbon), typeof(iIngotGold), 1)]
	public class iMedalGold : Item { }

	[EntityAttribute.ItemCombination(typeof(iRibbon), typeof(iIngotIron), 1)]
	public class iMedalIron : Item { }

	[EntityAttribute.ItemCombination(typeof(iItemWoodBoard), typeof(iItemWoodBoard), 1)]
	public class iCross : Item { }

	[EntityAttribute.ItemCombination(typeof(iMusicNote), typeof(iIngotGold), 1)]
	public class iBell : Item { }

	[EntityAttribute.ItemCombination(typeof(iLetter), typeof(iMagnet), typeof(iElectricWire), 1)]
	public class iFloppyDisk : Item { }

	[EntityAttribute.ItemCombination(typeof(iMusicNote), typeof(iRibbon), typeof(iMagnet), 1)]
	public class iTape : Item { }

	[EntityAttribute.ItemCombination(typeof(iPaper), typeof(iPaper), typeof(iPaper), typeof(iTreeBranch), 1)]
	public class iScroll : Item { }

	[EntityAttribute.ItemCombination(typeof(iCorrugatedBox), typeof(iRibbon), 1)]
	public class iGiftBox : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iGem), 1)]
	[EntityAttribute.ItemCombination(typeof(iRubyRed), typeof(iRubyRed), typeof(iRubyRed), typeof(iRubyRed), 1)]
	public class iGemRed : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iGem), 1)]
	[EntityAttribute.ItemCombination(typeof(iRubyBlue), typeof(iRubyBlue), typeof(iRubyBlue), typeof(iRubyBlue), 1)]
	public class iGemBlue : Item { }

	[EntityAttribute.ItemCombination(typeof(iGem), typeof(iRuneLightning), 1)]
	[EntityAttribute.ItemCombination(typeof(iRubyOrange), typeof(iRubyOrange), typeof(iRubyOrange), typeof(iRubyOrange), 1)]
	public class iGemOrange : Item { }

	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iGem), 1)]
	[EntityAttribute.ItemCombination(typeof(iRubyGreen), typeof(iRubyGreen), typeof(iRubyGreen), typeof(iRubyGreen), 1)]
	public class iGemGreen : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuby), typeof(iRuby), typeof(iRuby), typeof(iRuby), 1)]
	public class iGem : Item { }

	[EntityAttribute.ItemCombination(typeof(iIronWire), typeof(iIronWire), typeof(iIronWire), typeof(iIronWire), 1)]
	public class iChain : Item { }

	[EntityAttribute.ItemCombination(typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iChopsticks : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iRuby), 1)]
	public class iRubyRed : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iRuby), 1)]
	public class iRubyBlue : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuby), typeof(iRuneLightning), 1)]
	public class iRubyOrange : Item { }

	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iRuby), 1)]
	public class iRubyGreen : Item { }

	[EntityAttribute.ItemCombination(typeof(iMeatBone), typeof(iIngotIron), 1)]
	public class iSpoonFork : Item { }

	[EntityAttribute.ItemCombination(typeof(iKeyIron), typeof(iIngotIron), 1)]
	public class iLockIron : Item { }

	[EntityAttribute.ItemCombination(typeof(iKeyGold), typeof(iIngotGold), 1)]
	public class iLockGold : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iLeaf), typeof(iClay), 1)]
	public class iTeapot : Item { }

	[EntityAttribute.ItemCombination(typeof(iIngotIron), 4)]
	public class iBolt : Item { }

	[EntityAttribute.ItemCombination(typeof(iDice), typeof(iPaper), 1)]
	public class iPokerCard : Item { }

	[EntityAttribute.ItemCombination(typeof(iItemCoin), typeof(iDice), 1)]
	public class iGamblingChip : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iSkull), 1)]
	public class iTotemFire : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iSkull), 1)]
	public class iTotemWater : Item { }

	[EntityAttribute.ItemCombination(typeof(iSkull), typeof(iRuneLightning), 1)]
	public class iTotemLightning : Item { }

	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iSkull), 1)]
	public class iTotemPoison : Item { }

	[EntityAttribute.ItemCombination(typeof(iLetter), typeof(iElectricWire), 1)]
	public class iDialogBox : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iTreeBranchBundle), 1)]
	public class iPaper : Item { }

	[EntityAttribute.ItemCombination(typeof(iIngotGold), 100)]
	public class iItemCoin : Item { public override int MaxStackCount => 100; }

	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iLeaf), 1)]
	public class iHerb : Item { }

	[EntityAttribute.ItemCombination(typeof(iTrayWood), typeof(iTrayWood), typeof(iRope), typeof(iTreeBranch), 1)]
	public class iScales : Item { }

	[EntityAttribute.ItemCombination(typeof(iVolatilePotionEmpty), typeof(iRuneFire), 1)]
	public class iVolatilePotionRed : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iVolatilePotionEmpty), 1)]
	public class iVolatilePotionBlue : Item { }

	[EntityAttribute.ItemCombination(typeof(iVolatilePotionEmpty), typeof(iRuneLightning), 1)]
	public class iVolatilePotionOrange : Item { }

	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iVolatilePotionEmpty), 1)]
	public class iVolatilePotionGreen : Item { }

	[EntityAttribute.ItemCombination(typeof(iGlass), typeof(iGlass), 1)]
	public class iVolatilePotionEmpty : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iTreeBranch), 4)]
	public class iRope : Item { }

	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iIngotIron), 1)]
	public class iIronWire : Item { }

	[EntityAttribute.ItemCombination(typeof(iIronWire), typeof(iRuneLightning), 1)]
	public class iElectricWire : Item { }

	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iTreeBranch), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iTreeBranchBundle : Item { }

	[EntityAttribute.ItemCombination(typeof(iTreeTrunk), typeof(iTreeTrunk), typeof(iTreeTrunk), 1)]
	[EntityAttribute.ItemCombination(typeof(iTreeStump), typeof(iTreeStump), 1)]
	public class iItemWoodBoard : Item { }

	[EntityAttribute.ItemCombination(typeof(iMedalGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
	public class iTrophy : Item { }

	[EntityAttribute.ItemCombination(typeof(iPotionEmpty), typeof(iRuneFire), typeof(iIronWire), 1)]
	public class iLantern : Item { }

	[EntityAttribute.ItemCombination(typeof(iPotionEmpty), typeof(iRuneFire), 1)]
	public class iPotionRed : Item { }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iPotionEmpty), 1)]
	public class iPotionBlue : Item { }

	[EntityAttribute.ItemCombination(typeof(iPotionEmpty), typeof(iRuneLightning), 1)]
	public class iPotionOrange : Item { }

	[EntityAttribute.ItemCombination(typeof(iPotionEmpty), typeof(iRunePoison), 1)]
	public class iPotionGreen : Item { }

	[EntityAttribute.ItemCombination(typeof(iGlass), typeof(iGlass), typeof(iTreeBranch), 1)]
	public class iPotionEmpty : Item { }

	[EntityAttribute.ItemCombination(typeof(iMeatBone), 2)]
	public class iBrokenBone : Item { }

	[EntityAttribute.ItemCombination(typeof(iPaper), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iHandFan : Item { }

	[EntityAttribute.ItemCombination(typeof(iABC), typeof(iPaper), 1)]
	public class iLetter : Item { }

	[EntityAttribute.ItemCombination(typeof(iProcessor), typeof(iBeetle), typeof(iElectricWire), typeof(iBattery), 1)]
	public class iMachineBeetle : Item { }

	[EntityAttribute.ItemCombination(typeof(iPotionEmpty), typeof(iElectricWire), typeof(iRuneLightning), 1)]
	public class iBulb : Item { }

	[EntityAttribute.ItemCombination(typeof(iPaper), typeof(iGunpowder), 1)]
	public class iFirecracker : Item { }

	[EntityAttribute.ItemCombination(typeof(iPencil), typeof(iLegoBlockWhite), 1)]
	public class iDice : Item { }

	[EntityAttribute.ItemCombination(typeof(iTreeBranchBundle), typeof(iRuneFire), 3)]
	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iTreeTrunk), 6)]
	[EntityAttribute.ItemCombination(typeof(iTreeStump), typeof(iRuneFire), 9)]
	[EntityAttribute.ItemCombination(typeof(iItemWoodBoard), typeof(iRuneFire), 12)]
	public class iCharcoal : Item { }

	[EntityAttribute.ItemCombination(typeof(iKeyIron), typeof(iIngotGold), 1)]
	public class iKeyGold : Item { }

	[EntityAttribute.ItemCombination(typeof(iStone), typeof(iStone), 1)]
	public class iStonePolished : Item { }

	[EntityAttribute.ItemCombination(typeof(iFlint), typeof(iFlint), 1)]
	public class iFlintPolished : Item { }

	[EntityAttribute.ItemCombination(typeof(iFishBone), typeof(iTreeBranch), 1)]
	public class iComb : Item { }

	[EntityAttribute.ItemCombination(typeof(iLeather), typeof(iLeather), typeof(iLeather), 1)]
	public class iHandbag : Item { }

	[EntityAttribute.ItemCombination(typeof(iHandbag), typeof(iCapsule), 1)]
	public class iFirstAidKit : Item { }

	[EntityAttribute.ItemCombination(typeof(iCuteGhost), 4)]
	[EntityAttribute.ItemCombination(typeof(iBoringGhost), 2)]
	public class iSoul : Item { }

	[EntityAttribute.ItemCombination(typeof(iElectronicChip), typeof(iBookBlue), typeof(iElectricWire), typeof(iIngotIron), 1)]
	public class iProcessor : Item { }

	[EntityAttribute.ItemCombination(typeof(iBookYellow), typeof(iIngotIron), typeof(iElectricWire), typeof(iFloppyDisk), 1)]
	public class iElectronicChip : Item { }

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
	public class iLeaf : Item { }
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



}