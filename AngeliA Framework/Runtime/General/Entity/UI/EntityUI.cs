using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.DontDrawBehind]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.Capacity(1, 1)]
	[EntityAttribute.ForceSpawn]
	[EntityAttribute.StageOrder(1024)]
	[EntityAttribute.Layer(EntityLayer.UI)]
	public abstract class EntityUI : Entity {

		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.SetLayerToUI();
			UpdateUI();
			CellRenderer.SetLayerToDefault();
		}

		public virtual void UpdateUI () { }

		protected static int Unify (int value) => CellRendererGUI.Unify(value);
		protected static int Unify (float value) => CellRendererGUI.Unify(value);
		protected static int ReverseUnify (int value) => CellRendererGUI.ReverseUnify(value);

	}
}