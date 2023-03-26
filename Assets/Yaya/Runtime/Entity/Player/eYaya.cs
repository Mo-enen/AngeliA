using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using System;

namespace Yaya {
	[FirstSelectedPlayer]
	public class eYaya : eYayaPlayer {


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
			if (!Game.Current.TryGetEntity<eGuaGua>(out _)) {
				Summon.CreateSummon<eGuaGua>(this, X, Y);
			}
		}


	}
}