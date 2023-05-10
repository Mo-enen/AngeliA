using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

namespace AngeliaGame {
	public class eYaya : Player {

		// Pose ID
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

		// Api
		protected override bool SpinOnGroundPound => true;

		// Data
		private eGuaGua GuaGua = null;


		// MSG
		public eYaya () {

			WalkToRunAccumulation.Value = 0;
			JumpDownThoughOneway.Value = true;
			FlyAvailable.Value = true;
			SlideAvailable.Value = true;
			SlideOnAnyBlock.Value = true;
			FlyGlideAvailable.Value = false;

			BulletName.Value = "YayaPaw";
			MinimalChargeAttackDuration.Value = 42;

			MaxHP.Value = 4;

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

			// Ear
			if (Head.Tint.a > 0) {
				bool damaging = AnimatedPoseType == CharacterPoseAnimationType.TakingDamage;
				DrawAnimalEars(
					damaging ? DAMAGE_EAR_L_ID : CATEAR_L_ID,
					damaging ? DAMAGE_EAR_R_ID : CATEAR_R_ID,
					Head.GetGlobalRect(), Head.Z + (Head.FrontSide ? 33 : -33)
				);
			}

			// Tail
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

		}


		protected override void DrawPoseFace (int bounce) {
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
			base.DrawPoseFace(bounce);
		}


	}

}