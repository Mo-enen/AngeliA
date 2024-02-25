using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 

public interface IWindowEntityUI {

	public IRect BackgroundRect { get; }
	private static int[] EndIndexCache = null;

	[OnGameUpdatePauseless(31)]
	public static void OnGameUpdatePauseless () {
		int count = Stage.EntityCounts[EntityLayer.UI];
		var entities = Stage.Entities[EntityLayer.UI];
		EndIndexCache ??= new int[Renderer.TextLayerCount];
		for (int i = 0; i < Renderer.TextLayerCount; i++) {
			EndIndexCache[i] = Renderer.GetTextUsedCellCount(i);
		}
		for (int i = 0; i < count; i++) {
			var e = entities[i];
			if (e is not IWindowEntityUI window) continue;
			if (e is not EntityUI ui) continue;
			if (ui.TextCellEndIndex != null) {
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

		Renderer.SetLayerToUI();

		TextCellStartIndex = Renderer.GetTextUsedCellCount();

		if (Game.PauselessFrame == BlockingEventFrame) {
			Input.IgnoreInput();
		}
		UpdateUI();
		if (Game.PauselessFrame == BlockingEventFrame) {
			Input.CancelIgnoreInput();
		}

		TextCellEndIndex ??= new int[Renderer.TextLayerCount];
		for (int i = 0; i < Renderer.TextLayerCount; i++) {
			TextCellEndIndex[i] = Renderer.GetTextUsedCellCount(i);
		}

		Renderer.SetLayerToDefault();

		Renderer.SortLayer(RenderLayer.UI);

		if (BlockEvent) {
			Cursor.CursorPriority = int.MaxValue;
			BlockingEventFrame = Game.PauselessFrame;
		}
	}

	public virtual void UpdateUI () { }

	protected static int Unify (int value) => GUI.Unify(value);
	protected static int Unify (float value) => GUI.Unify(value);
	protected static int ReverseUnify (int value) => GUI.ReverseUnify(value);

}