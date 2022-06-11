using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[ForceUpdate]
	public class ePlayerBed : eFurniture {

		private static readonly int CODE_LEFT = "Bed Left".AngeHash();
		private static readonly int CODE_MID = "Bed Mid".AngeHash();
		private static readonly int CODE_RIGHT = "Bed Right".AngeHash();
		private static readonly int CODE_SINGLE = "Bed Single".AngeHash();

		protected override Direction3 ModuleType => Direction3.Horizontal;
		protected override int ArtworkCode_LeftDown => CODE_LEFT;
		protected override int ArtworkCode_Mid => CODE_MID;
		protected override int ArtworkCode_RightUp => CODE_RIGHT;
		protected override int ArtworkCode_Single => CODE_SINGLE;

		// Data
		private Game Game = null;
		private bool PlayerSpawned = false;


		// MSG
		public override void OnInitialize (Game game) {
			base.OnInitialize(game);
			Game = game;
			PlayerSpawned = false;
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (
				!HasSameFurnitureOnLeftOrDown &&
				!PlayerSpawned &&
				Game != null &&
				Game.FirstEntityOfType<ePlayer>() == null
			) {
				PlayerSpawned = true;
				if (Game.AddEntity(typeof(ePlayer).AngeHash(), X, Y) is ePlayer player) {
					player.Sleep();
				}
			}
		}


	}
}
