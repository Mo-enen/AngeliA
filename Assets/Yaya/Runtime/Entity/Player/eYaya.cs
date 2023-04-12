using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

namespace Yaya {
	[FirstSelectedPlayer]
	public class eYaya : Player {


		private static readonly int HAIR_FFL_ID = "Yaya.FrontHair.FL".AngeHash();
		private static readonly int HAIR_FFR_ID = "Yaya.FrontHair.FR".AngeHash();
		private static readonly int HAIR_BFL_ID = "Yaya.BackHair.FL".AngeHash();
		private static readonly int HAIR_BFR_ID = "Yaya.BackHair.FR".AngeHash();
		private static readonly int HAIR_FBL_ID = "Yaya.FrontHair.BL".AngeHash();
		private static readonly int HAIR_FBR_ID = "Yaya.FrontHair.BR".AngeHash();
		private static readonly int HAIR_BBL_ID = "Yaya.BackHair.BL".AngeHash();
		private static readonly int HAIR_BBR_ID = "Yaya.BackHair.BR".AngeHash();
		private static readonly int FACE_GROUP_ID = "Yaya.Face".AngeHash();
		private static readonly int FACE_BLINK_ID = "Yaya.Face.Blink".AngeHash();
		private static readonly int CATEAR_L_ID = "Yaya.CatEarL".AngeHash();
		private static readonly int CATEAR_R_ID = "Yaya.CatEarR".AngeHash();
		private static readonly int CAT_TAIL_ID = "Yaya.CatTail".AngeHash();

		protected override CharacterRenderingSolution RenderingSolution => CharacterRenderingSolution.Pose;
		protected override int FrontHairID => FacingFront ?
			FacingRight ? HAIR_FFR_ID : HAIR_FFL_ID :
			FacingRight ? HAIR_FBR_ID : HAIR_FBL_ID;
		protected override int BackHairID => FacingFront ?
			FacingRight ? HAIR_BFR_ID : HAIR_BFL_ID :
			FacingRight ? HAIR_BBR_ID : HAIR_BBL_ID;
		protected override int FaceGroupID => FACE_GROUP_ID;
		protected override int FaceBlinkID => FACE_BLINK_ID;


		public eYaya () {

			// Config
			MovementWidth.Value = 150;
			MovementHeight.Value = 384;
			SquatHeight.Value = 200;
			DashDuration.Value = 20;
			RunAccumulation.Value = 256;
			JumpSpeed.Value = 73;
			SwimInFreeStyle.Value = false;
			JumpWithRoll.Value = false;
			SecondJumpWithRoll.Value = true;
			JumpCount.Value = 2;
			JumpThoughOneway.Value = true;
			FlyAvailable.Value = true;
			SlideAvailable.Value = true;
			SlideOnAnyBlock.Value = true;
			GrabTopAvailable.Value = true;
			GrabSideAvailable.Value = true;
			GrabFlipThroughDownAvailable.Value = true;
			GrabFlipThroughUp.Value = true;
			FlyMoveSpeed.Value = 32;
			FlyGlideAvailable.Value = false;

			BulletName.Value = "YayaPaw";
			AttackDuration.Value = 12;
			KeepAttackWhenHold.Value = false;
			MinimalChargeAttackDuration.Value = 42;

			MaxHP.Value = 4;

		}


		public override void OnActivated () {
			base.OnActivated();
			// Summon GuaGua
			if (!Game.Current.TryGetEntity<eGuaGua>(out _)) {
				//Summon.CreateSummon<eGuaGua>(this, X, Y);
			}
			// Goto Bed on Start
			if (!Game.Current.SavedPlayerUnitPosition.HasValue) {
				if (Game.Current.TryGetEntityNearby<eBed>(new Vector2Int(X, Y), out var bed)) {
					//bed.Invoke(this);
					//SetAsFullSleep();
				}
			}
		}


		private readonly YayaSuit TestSuit = new();
		protected override int CalculatePose () {
			int result = base.CalculatePose();

			DrawAnimalEars(CATEAR_L_ID, CATEAR_R_ID, HeadTransform.GetGlobalRect(this), HeadTransform.Z + 33);
			DrawTail(CAT_TAIL_ID, BodyTransform.GetGlobalRect(this), BodyTransform.Z - 1);



			///////////////////// Test ////////////////////
			TestSuit.Draw(this);
			///////////////////// Test ////////////////////


			return result;
		}


	}


	public class YayaSuit : AngeliaFramework.Cloth {

		private static readonly int BODY_CODE = "YayaSuit.Body".AngeHash();
		private static readonly int BODY_BACK_CODE = "YayaSuit.BodyBack".AngeHash();
		private static readonly int SOCK_CODE = "YayaSuit.Sock".AngeHash();
		private static readonly int SHOE_CODE = "YayaSuit.Shoe".AngeHash();
		private static readonly Color32 TINT = new(39, 38, 60, 255);

		public override void Draw (Character character) {
			if (character is null) return;
			character.BodyTransform.Tint = TINT;
			character.UpperArmLTransform.Tint = TINT;
			character.UpperArmRTransform.Tint = TINT;
			character.LowerArmLTransform.Tint = TINT;
			character.LowerArmRTransform.Tint = TINT;
			character.LowerLegLTransform.Tint = Const.CLEAR;
			character.LowerLegRTransform.Tint = Const.CLEAR;
			// Body
			DrawSpriteAsBody(
				character,
				character.BodyTransform,
				character.FacingFront ? BODY_CODE : BODY_BACK_CODE
			);
			// Socks
			DrawSprite(character, character.LowerLegLTransform, SOCK_CODE);
			DrawSprite(character, character.LowerLegRTransform, SOCK_CODE);
			// Shoes
			DrawSpriteAsShoe(character, character.FootLTransform, SHOE_CODE);
			DrawSpriteAsShoe(character, character.FootRTransform, SHOE_CODE);
			// Skirt




		}



	}
}