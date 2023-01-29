using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eGomokuUI : eMiniGame {




		#region --- VAR ---


		private static readonly int CIRCLE_CODE = "Circle16".AngeHash();


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();



		}


		protected override void FrameUpdateUI () {
			base.FrameUpdateUI();
			if (Game.Current.State != GameState.Play) return;
			Update_HotKey();
			Update_Hint();
		}


		private void Update_HotKey () {
			// Quit
			if (FrameInput.GameKeyDown(GameKey.Start)) {
				FrameInput.UseGameKey(GameKey.Start);


			}
		}


		private void Update_Hint () {
			eControlHintUI.DrawHint(GameKey.Left, GameKey.Right, WORD.HINT_MOVE);
			eControlHintUI.DrawHint(GameKey.Down, GameKey.Up, WORD.HINT_MOVE);

		}


		#endregion




		#region --- API ---



		#endregion




		#region --- LGC ---



		#endregion




	}
}