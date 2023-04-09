using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

namespace Yaya {
	[FirstSelectedPlayer]
	public class eYaya : Player {


		private static readonly int HAIR_FFL_ID = "Yaya.FrontHair.FL 0".AngeHash();
		private static readonly int HAIR_FFR_ID = "Yaya.FrontHair.FR 0".AngeHash();
		private static readonly int HAIR_BFL_ID = "Yaya.BackHair.FL 0".AngeHash();
		private static readonly int HAIR_BFR_ID = "Yaya.BackHair.FR 0".AngeHash();
		private static readonly int HAIR_FBL_ID = "Yaya.FrontHair.BL 0".AngeHash();
		private static readonly int HAIR_FBR_ID = "Yaya.FrontHair.BR 0".AngeHash();
		private static readonly int HAIR_BBL_ID = "Yaya.BackHair.BL 0".AngeHash();
		private static readonly int HAIR_BBR_ID = "Yaya.BackHair.BR 0".AngeHash();
		private static readonly int FACE_GROUP_ID = "Yaya.Face".AngeHash();
		private static readonly int FACE_BLINK_ID = "Yaya.Face.Blink".AngeHash();
		protected override CharacterRenderingSolution RenderingSolution => CharacterRenderingSolution.Pose;
		protected override int FrontHairGroupID => FacingFront ?
			FacingRight ? HAIR_FFR_ID : HAIR_FFL_ID :
			FacingRight ? HAIR_FBR_ID : HAIR_FBL_ID;
		protected override int BackHairGroupID => FacingFront ?
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
			RunAccumulation.Value = 0;
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
				Summon.CreateSummon<eGuaGua>(this, X, Y);
			}
			// Goto Bed on Start
			if (!Game.Current.SavedPlayerUnitPosition.HasValue) {
				if (Game.Current.TryGetEntityNearby<eBed>(new Vector2Int(X, Y), out var bed)) {
					bed.Invoke(this);
					SetAsFullSleep();
				}
			}
		}





	}
}