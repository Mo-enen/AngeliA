using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public interface IWindowEntityUI {
	public IRect BackgroundRect { get; }
}


[EntityAttribute.ExcludeInMapEditor]
[EntityAttribute.MapEditorGroup("UI")]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.DontDrawBehind]
[EntityAttribute.DontDestroyOutOfRange]
[EntityAttribute.Capacity(1, 1)]
[EntityAttribute.Layer(EntityLayer.UI)]
public abstract class EntityUI : Entity {

	protected virtual bool BlockEvent => false;
	private static int BlockingEventFrame = -1;
	private bool IgnoringMouseInput;
	private bool IgnoringKeyInput;

	// MSG
	public EntityUI () => Active = false;

	public override void FirstUpdate () {
		base.FirstUpdate();
		IgnoringMouseInput = Input.IgnoringMouseInput;
		IgnoringKeyInput = Input.IgnoringKeyInput;
		if (BlockEvent) Input.IgnoreAllInput(0);
	}

	public override void Update () {
		base.Update();
		if (BlockEvent) {
			if (!IgnoringMouseInput) {
				Input.CancelIgnoreMouseInput();
			}
			if (!IgnoringKeyInput) {
				Input.CancelIgnoreKeyInput();
			}
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();

		using (new UILayerScope()) {

			if (Game.PauselessFrame == BlockingEventFrame) {
				Input.IgnoreAllInput(0);
			}

			UpdateUI();

			if (Game.PauselessFrame == BlockingEventFrame) {
				Input.CancelIgnoreMouseInput();
				Input.CancelIgnoreKeyInput();
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

}