using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

namespace AngeliaGame {
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
		private static readonly int DAMAGE_EAR_L_ID = "Yaya.CatEarL.Damage".AngeHash();
		private static readonly int DAMAGE_EAR_R_ID = "Yaya.CatEarR.Damage".AngeHash();
		private static readonly int CAT_TAIL_ID = "Yaya.CatTail".AngeHash();
		private static readonly int DAMAGING_FACE_ID = "Yaya.Face.Damage".AngeHash();
		private static readonly int PASSOUT_FACE_ID = "Yaya.Face.PassOut".AngeHash();
		private static readonly int SLEEP_FACE_ID = "Yaya.Face.Sleep".AngeHash();
		private static readonly int ATTACK_FACE_ID = "Yaya.Face.Attack".AngeHash();
		private static readonly int PROPELLER_ID = "Propeller".AngeHash();

		public override CharacterRenderingSolution RenderingSolution => CharacterRenderingSolution.Pose;
		protected override int FrontHair_FL => HAIR_FFL_ID;
		protected override int FrontHair_FR => HAIR_FFR_ID;
		protected override int FrontHair_BL => HAIR_FBL_ID;
		protected override int FrontHair_BR => HAIR_FBR_ID;
		protected override int BackHair_FL => HAIR_BFL_ID;
		protected override int BackHair_FR => HAIR_BFR_ID;
		protected override int BackHair_BL => HAIR_BBL_ID;
		protected override int BackHair_BR => HAIR_BBR_ID;
		protected override int FaceGroupID => FACE_GROUP_ID;
		protected override int FaceBlinkID => FACE_BLINK_ID;
		protected override bool SpinOnGroundPound => true;

		// Data
		private readonly YayaSuit DefalutSuit = new();
		private eGuaGua GuaGua = null;


		// MSG
		public eYaya () {

			RunAccumulation.Value = 0;
			JumpDownThoughOneway.Value = true;
			FlyAvailable.Value = true;
			SlideOnAnyBlock.Value = true;
			FlyGlideAvailable.Value = false;

			BulletName.Value = "YayaPaw";
			MinimalChargeAttackDuration.Value = 42;

			MaxHP.Value = 4;

		}


		public override void OnActivated () {
			base.OnActivated();

			// Goto Bed on Start
			if (!Game.Current.SavedPlayerUnitPosition.HasValue) {
				if (Game.Current.TryGetEntityNearby<eBed>(new Vector2Int(X, Y), out var bed)) {
					bed.Invoke(this);
					SetAsFullSleep();
				}
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			// Summon GuaGua
			if (GuaGua == null || !GuaGua.Active) {
				GuaGua = Summon.CreateSummon<eGuaGua>(this, X, Y);
			}
		}


		protected override void OnPoseCalculated () {
			base.OnPoseCalculated();

			if (Head.Tint.a > 0) {
				bool damaging = AnimatedPoseType == CharacterPoseAnimationType.TakingDamage;
				DrawAnimalEars(
					damaging ? DAMAGE_EAR_L_ID : CATEAR_L_ID,
					damaging ? DAMAGE_EAR_R_ID : CATEAR_R_ID,
					Head.GetGlobalRect(), Head.Z + (Head.FrontSide ? 33 : -33)
				);
			}

			if (
				!IsPassOut &&
				AnimatedPoseType != CharacterPoseAnimationType.Sleep &&
				AnimatedPoseType != CharacterPoseAnimationType.Dash
			) {
				if (AnimatedPoseType == CharacterPoseAnimationType.Fly) {
					// Propeller
					DrawPropeller(PROPELLER_ID, new(78, 76, 120, 255), offsetY: 2 * Const.CEL / Const.ART_CEL);
				} else {
					DrawTail(CAT_TAIL_ID, Body.Z + (Body.FrontSide ? -33 : 33));
				}
			}

			DefalutSuit.Draw(this);

		}


		protected override void CalculatePoseFace (int bounce) {
			if (!FacingFront) return;
			var headRect = Head.GetGlobalRect();
			if (AnimatedPoseType == CharacterPoseAnimationType.TakingDamage) {
				DrawFace(DAMAGING_FACE_ID, headRect, Head.Width > 0, Head.Z + 1, true);
				return;
			}
			if (AnimatedPoseType == CharacterPoseAnimationType.PassOut) {
				DrawFace(PASSOUT_FACE_ID, headRect, Head.Width > 0, Head.Z + 1, true);
				return;
			}
			if (AnimatedPoseType == CharacterPoseAnimationType.Sleep) {
				DrawFace(SLEEP_FACE_ID, headRect, Head.Width > 0, Head.Z + 1, true);
				return;
			}
			if (
				AnimatedPoseType == CharacterPoseAnimationType.Attack ||
				AnimatedPoseType == CharacterPoseAnimationType.AttackAir ||
				AnimatedPoseType == CharacterPoseAnimationType.AttackMove ||
				AnimatedPoseType == CharacterPoseAnimationType.AttackSwim
			) {
				DrawFace(ATTACK_FACE_ID, headRect, Head.Width > 0, Head.Z + 1);
				return;
			}
			base.CalculatePoseFace(bounce);
		}


	}


	public class YayaSuit : AngeliaFramework.Cloth {


		private static readonly int BODY_CODE = "YayaSuit.Body".AngeHash();
		private static readonly int BODY_CODE_LEFT = "YayaSuit.Body.Left".AngeHash();
		private static readonly int BODY_CODE_RIGHT = "YayaSuit.Body.Right".AngeHash();
		private static readonly int BODY_CODE_BACK = "YayaSuit.BodyBack".AngeHash();
		private static readonly int SOCK_CODE = "YayaSuit.Sock".AngeHash();
		private static readonly int SHOE_CODE = "YayaSuit.Shoe".AngeHash();
		private static readonly int SKIRT_CODE = "YayaSuit.Skirt".AngeHash();
		private static readonly Color32 TINT = new(39, 38, 60, 255);


		public override void Draw (Character character) {

			if (character is null) return;

			// Body
			if (character.Body.Tint.a != 0) {
				character.Body.Tint = TINT;
				character.UpperArmL.Tint = TINT;
				character.UpperArmR.Tint = TINT;
				character.LowerArmL.Tint = TINT;
				character.LowerArmR.Tint = TINT;
				// Skirt
				DrawSpriteAsSkirt(
					character, SKIRT_CODE,
					GetSkirtOffsetY(character),
					GetSkirtShiftWidth(character)
				);
				// Body Cloth
				if (character.Body.FrontSide) {
					DrawSpriteAsBody(character.Body, BODY_CODE_LEFT, BODY_CODE, BODY_CODE_RIGHT, character.PoseRootTwist);
				} else {
					DrawSpriteAsBody(character.Body, BODY_CODE_BACK);
				}
			}

			// Leg
			if (character.LowerLegL.Tint.a != 0) {
				DrawSprite(character.LowerLegL, SOCK_CODE);
			}
			if (character.LowerLegR.Tint.a != 0) {
				DrawSprite(character.LowerLegR, SOCK_CODE);
			}

			// Shoes
			if (character.FootL.Tint.a != 0) {
				DrawSpriteAsShoe(character.FootL, SHOE_CODE);
			}
			if (character.FootR.Tint.a != 0) {
				DrawSpriteAsShoe(character.FootR, SHOE_CODE);
			}

		}


		private static int GetSkirtOffsetY (Character character) {
			return character.MovementState == CharacterMovementState.Run ? Const.CEL / Const.ART_CEL / 2 : 0;
		}


		private static int GetSkirtShiftWidth (Character character) => character.MovementState switch {
			CharacterMovementState.JumpUp or CharacterMovementState.JumpDown => 2 * Const.CEL / Const.ART_CEL,
			CharacterMovementState.Run => Const.CEL / Const.ART_CEL,
			_ => 0,
		};


	}
}