using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.DontDespawnWhenOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.ForceUpdate]
	public abstract class ScreenUI : Entity {


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.SetLayer(YayaConst.SHADER_UI);
			UpdateForUI();
			CellRenderer.SetLayerToDefault();
		}


		protected abstract void UpdateForUI ();


	}


	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.ForceUpdate]
	public abstract class WorldUI : Entity {


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.SetLayer(YayaConst.SHADER_UI);
			UpdateForUI();
			CellRenderer.SetLayerToDefault();
		}


		protected abstract void UpdateForUI ();


	}
}