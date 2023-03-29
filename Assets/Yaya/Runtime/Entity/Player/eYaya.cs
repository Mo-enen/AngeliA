using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

namespace Yaya {
	[FirstSelectedPlayer]
	public class eYaya : Player {


		protected override CharacterRenderingSolution RenderingSolution => CharacterRenderingSolution.Pose;

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


		public override void OnActived () {
			base.OnActived();
			// Summon GuaGua
			if (!Game.Current.TryGetEntity<eGuaGua>(out _)) {
				Summon.CreateSummon<eGuaGua>(this, X, Y);
			}
			// Goto Bed on Start
			if (!Game.Current.SavedPlayerUnitPosition.HasValue) {
				if (Game.Current.TryGetEntityNearby<eBed>(new Vector2Int(X, Y), out var bed)) {
					bed.Invoke(this);
					FullSleep();
				}
			}
		}


	}
}