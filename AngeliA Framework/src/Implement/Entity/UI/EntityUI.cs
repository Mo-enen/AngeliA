using System.Collections;
using System.Collections.Generic;


namespace AngeliA;

public interface IWindowEntityUI {

	public IRect BackgroundRect { get; }
	private static int[] EndIndexCache = null;

	[OnGameUpdatePauseless(31)]
	internal static void OnGameUpdatePauseless () {
		if (!Stage.Enable) return;
		ClipTextForAllUI(Stage.Entities[EntityLayer.UI], Stage.EntityCounts[EntityLayer.UI]);
	}


	public static void ClipTextForAllUI (IEnumerable<Entity> entities, int count) {
		EndIndexCache ??= new int[Renderer.TextLayerCount];
		for (int i = 0; i < Renderer.TextLayerCount; i++) {
			EndIndexCache[i] = Renderer.GetTextUsedCellCount(i);
		}
		int index = -1;
		foreach (var e in entities) {
			index++;
			if (index >= count) break;
			if (!e.Active) continue;
			if (e is not IWindowEntityUI window) continue;
			if (e is not EntityUI ui) continue;
			if (ui.TextCellEndIndex == null) continue;
			for (int j = 0; j < Renderer.TextLayerCount; j++) {
				Renderer.ExcludeTextCells(
					j,
					window.BackgroundRect,
					ui.TextCellEndIndex[j],
					EndIndexCache[j]
				);
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

	// MSG
	public override void FirstUpdate () {
		base.FirstUpdate();
		if (BlockEvent) Input.IgnoreAllInput(0);
	}

	public override void Update () {
		base.Update();
		if (BlockEvent) {
			Input.CancelIgnoreMouseInput();
			Input.CancelIgnoreKeyInput();
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();

		using (Scope.RendererLayerUI()) {

			TextCellStartIndex = Renderer.GetTextUsedCellCount();

			if (Game.PauselessFrame == BlockingEventFrame) {
				Input.IgnoreAllInput(0);
			}

			UpdateUI();

			if (Game.PauselessFrame == BlockingEventFrame) {
				Input.CancelIgnoreMouseInput();
				Input.CancelIgnoreKeyInput();
			}

			TextCellEndIndex ??= new int[Renderer.TextLayerCount];
			for (int i = 0; i < Renderer.TextLayerCount; i++) {
				TextCellEndIndex[i] = Renderer.GetTextUsedCellCount(i);
			}
		}

		if (BlockEvent) {
			Cursor.CursorPriority = int.MaxValue;
			BlockingEventFrame = Game.PauselessFrame;
		}
	}

	public virtual void UpdateUI () { }

	// API
	protected static int Unify (int value) => GUI.Unify(value);
	protected static int Unify (float value) => GUI.Unify(value);

}