using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


/// <summary>
/// Entities which spawns into the stage and serves as UI
/// </summary>
[EntityAttribute.ExcludeInMapEditor]
[EntityAttribute.MapEditorGroup("UI")]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.DontDrawBehind]
[EntityAttribute.DontDespawnOutOfRange]
[EntityAttribute.Capacity(1, 0)]
[EntityAttribute.Layer(EntityLayer.UI)]
public abstract class EntityUI : Entity {

	/// <summary>
	/// True if this UI blocks mouse button event
	/// </summary>
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

			int oldCursorP = Cursor.CursorPriority;
			if (Game.PauselessFrame == BlockingEventFrame) {
				Cursor.CursorPriority = int.MaxValue;
				Input.IgnoreAllInput(0);
			}

			UpdateUI();

			if (Game.PauselessFrame == BlockingEventFrame) {
				Input.CancelIgnoreMouseInput();
				Input.CancelIgnoreKeyInput();
				Cursor.CursorPriority = oldCursorP;
			}

		}

		if (BlockEvent) {
			BlockingEventFrame = Game.PauselessFrame;
		}
	}

	public virtual void UpdateUI () { }

	// API
	/// <inheritdoc cref="GUI.Unify(int)"/>
	protected static int Unify (int value) => GUI.Unify(value);

}