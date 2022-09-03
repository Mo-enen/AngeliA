using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.DontDespawnWhenOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.ForceUpdate]
	public abstract class EntityUI : Entity {



		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.SetLayer(YayaConst.SHADER_UI);
			UpdateForUI(CellRenderer.CameraRect);
			CellRenderer.SetLayerToDefault();
		}


		protected abstract void UpdateForUI (RectInt screenRect);


	}
}