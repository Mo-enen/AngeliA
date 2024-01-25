using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public class HomeScreen : WindowUI {




		#region --- VAR ---


		// Const
		public static readonly int TYPE_ID = typeof(HomeScreen).AngeHash();

		// Api
		public new static HomeScreen Instance => WindowUI.Instance as HomeScreen;
		public static bool IsActived => Instance != null && Instance.Active;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			Game.StopGame();
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			CursorSystem.RequireCursor();
			ControlHintUI.ForceHideGamepad();
			Skybox.ForceSkyboxTint(new Byte4(32, 33, 37, 255), new Byte4(32, 33, 37, 255));
		}


		public override void UpdateUI () {
			base.UpdateUI();





		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}