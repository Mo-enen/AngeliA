using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.DontDespawnWhenOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	public abstract class ScreenUI : UiEntity { }



	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.ForceUpdate]
	public abstract class UiEntity : Entity {


		public int LocalFrame => Game.GlobalFrame - ActiveFrame;
		private int ActiveFrame = 0;


		public override void OnActived () {
			base.OnActived();
			ActiveFrame = Game.GlobalFrame;
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.SetLayer(YayaConst.SHADER_UI);
			UpdateForUI();
			CellRenderer.SetLayerToDefault();
		}


		public void ResetLocalFrame () => ActiveFrame = Game.GlobalFrame;


		protected abstract void UpdateForUI ();


	}
}