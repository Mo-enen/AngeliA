using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eBattleAirship : MiniGame {




		#region --- VAR ---


		private static readonly int CLOUD_CODE = "Cloud".AngeHash();


		#endregion




		#region --- MSG ---


		//[AfterGameInitialize]
		public static void Test () => Game.Current.SpawnEntity<eBattleAirship>(0, 0);


		public override void OnActivated () {
			base.OnActivated();



		}


		protected override void FrameUpdateUI () {
			base.FrameUpdateUI();




		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}