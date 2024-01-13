using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {

	public interface IWindowEntityUI {
		public IRect BackgroundRect { get; }
		[OnGameUpdatePauseless(31)]
		public static void OnGameUpdatePauseless () {
			int count = Stage.EntityCounts[EntityLayer.UI];
			var entities = Stage.Entities[EntityLayer.UI];
			for (int i = 0; i < count; i++) {
				var e = entities[i];
				if (e is not IWindowEntityUI window) continue;
				if (e is not EntityUI ui) continue;
				CellRenderer.ExcludeTextCellsForAllLayers(window.BackgroundRect, ui.TextCellEndIndex);
			}

		}
	}

	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.DontDrawBehind]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.Capacity(1, 1)]
	[EntityAttribute.ForceSpawn]
	[EntityAttribute.Layer(EntityLayer.UI)]
	public abstract class EntityUI : Entity {

		protected virtual bool BlockMouseEvent => false;
		protected virtual bool BlockKeyboardEvent => false;
		public int TextCellStartIndex { get; private set; }
		public int TextCellEndIndex { get; private set; }

		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.SetLayerToUI();
			TextCellStartIndex = CellRenderer.GetTextUsedCellCount();
			UpdateUI();
			TextCellEndIndex = CellRenderer.GetTextUsedCellCount();
			CellRenderer.SetLayerToDefault();
			CellRenderer.SortLayer(RenderLayer.UI);
			if (BlockMouseEvent) {
				FrameInput.UseMouseKey(0);
				FrameInput.UseMouseKey(1);
				FrameInput.UseMouseKey(2);
			}
			if (BlockKeyboardEvent) {
				FrameInput.UseAllHoldingKeys(true);
			}
		}

		public virtual void UpdateUI () { }

		protected static int Unify (int value) => CellRendererGUI.Unify(value);
		protected static int Unify (float value) => CellRendererGUI.Unify(value);
		protected static int ReverseUnify (int value) => CellRendererGUI.ReverseUnify(value);

	}
}