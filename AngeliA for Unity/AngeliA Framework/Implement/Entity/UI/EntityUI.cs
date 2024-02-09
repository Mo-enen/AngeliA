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
				if (ui.TextCellEndIndex != null) {
					for (int j = 0; j < CellRenderer.TextLayerCount; j++) {
						CellRenderer.ExcludeTextCells(j, window.BackgroundRect, ui.TextCellEndIndex[j]);
					}
				}
			}
		}
	}

	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.MapEditorGroup("UI")]
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.DontDrawBehind]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.Capacity(1, 1)]
	[EntityAttribute.ForceSpawn]
	[EntityAttribute.Layer(EntityLayer.UI)]
	public abstract class EntityUI : Entity {

		protected virtual bool BlockEvent => false;
		public int TextCellStartIndex { get; private set; }
		public int[] TextCellEndIndex { get; private set; }
		private static int BlockingEventFrame = -1;

		public override void FrameUpdate () {
			base.FrameUpdate();

			CellRenderer.SetLayerToUI();

			TextCellStartIndex = CellRenderer.GetTextUsedCellCount();

			if (Game.PauselessFrame == BlockingEventFrame) {
				FrameInput.IgnoreInput();
			}
			UpdateUI();
			if (Game.PauselessFrame == BlockingEventFrame) {
				FrameInput.CancelIgnoreInput();
			}

			TextCellEndIndex ??= new int[CellRenderer.TextLayerCount];
			for (int i = 0; i < CellRenderer.TextLayerCount; i++) {
				TextCellEndIndex[i] = CellRenderer.GetTextUsedCellCount(i);
			}

			CellRenderer.SetLayerToDefault();

			CellRenderer.SortLayer(RenderLayer.UI);

			if (BlockEvent) {
				CursorSystem.SetCursorPriority(int.MaxValue);
				BlockingEventFrame = Game.PauselessFrame;
			}
		}

		public virtual void UpdateUI () { }

		protected static int Unify (int value) => CellGUI.Unify(value);
		protected static int Unify (float value) => CellGUI.Unify(value);
		protected static int ReverseUnify (int value) => CellGUI.ReverseUnify(value);

	}
}