using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eAntiAttack : Zone {

		private static int UpdateFrame = int.MinValue;

		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			if (Game.GlobalFrame <= UpdateFrame) return;
			if (!TargetRect.HasValue) return;
			var player = ePlayer.Current;
			if (player == null || !player.Active) return;
			var targetRect = TargetRect.Value;
			if (!targetRect.Contains(player.X, player.Y)) return;
			player.Attackness.IgnoreAttack();
			UpdateFrame = Game.GlobalFrame;
		}

	}
}