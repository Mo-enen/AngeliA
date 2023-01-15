using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[FirstSelectedPlayer]
	public class eYaya : ePlayer {


		// Const
		private static readonly int FOOTSTEP_CODE = "YayaFootstep".AngeHash();

		// Api
		public static eYaya CurrentYaya { get; private set; } = null;
		public override eMascot Mascot => eGuaGua.Current;

		// Data
		private int LastStartRunFrame = int.MinValue;


		// MSG
		public eYaya () {

			CurrentYaya = this;

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
			GrabFlipThroughDown.Value = true;
			GrabFlipThroughUp.Value = true;

			BulletName.Value = "YayaPaw";
			AttackDuration.Value = 12;
			KeepAttackWhenHold.Value = false;
			MinimalChargeAttackDuration.Value = 42;

			MaxHP.Value = 4;


		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			Update_Run();
		}


		private void Update_Run () {
			// Last Start Run Frame
			if (MoveState == MovementState.Run) {
				if (LastStartRunFrame < 0) LastStartRunFrame = Game.GlobalFrame;
			} else if (LastStartRunFrame >= 0) {
				LastStartRunFrame = int.MinValue;
			}

			// Run Particle
			if (LastStartRunFrame >= 0 && (Game.GlobalFrame - LastStartRunFrame) % 20 == 19) {
				if (Game.Current.TrySpawnEntity(FOOTSTEP_CODE, X, Y, out var entity) && entity is eYayaFootstep step) {
					if (CellRenderer.TryGetSprite(GroundedID, out var sprite)) {
						step.Tint = sprite.SummaryTint;
					} else {
						step.Tint = Const.WHITE;
					}
				}
			}
		}


	}
}