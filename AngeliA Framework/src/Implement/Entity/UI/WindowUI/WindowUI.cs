using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
[EntityAttribute.DontDestroyOnSquadTransition]
[EntityAttribute.Capacity(1, 0)]
public abstract class WindowUI : EntityUI, IWindowEntityUI {


	// Api
	public virtual IRect BackgroundRect => Rect;

	// Data
	private static int UpdatedFrame = -1;

	// MSG
	[OnGameQuitting]
	public static void OnGameQuitting () {
		int len = Stage.EntityCounts[EntityLayer.UI];
		var entities = Stage.Entities[EntityLayer.UI];
		for (int i = 0; i < len; i++) {
			var e = entities[i];
			if (e is WindowUI window && e.Active) {
				e.OnInactivated();
			}
		}
	}

	public abstract void UpdateWindowUI ();

	public sealed override void UpdateUI () {
		base.UpdateUI();
		if (Game.PauselessFrame > UpdatedFrame) {
			// First
			UpdatedFrame = Game.PauselessFrame;
			UpdateWindowUI();
		} else {
			// Subsequent
			bool oldIgnore = FrameInput.IgnoringInput;
			if (!oldIgnore) FrameInput.IgnoreInput();
			try {
				int oldP = CursorSystem.CursorPriority;
				CursorSystem.CursorPriority = int.MaxValue;
				UpdateWindowUI();
				CursorSystem.CursorPriority = oldP;
				CellRenderer.Draw(Const.PIXEL, CellRenderer.CameraRect, new Color32(0, 0, 0, 200), int.MaxValue);
			} catch (System.Exception ex) { Game.LogException(ex); }
			if (!oldIgnore) FrameInput.CancelIgnoreInput();
		}
	}

	// API
	public static void OpenWindow (int typeID) => Stage.SpawnEntity(typeID, 0, 0);

	public static void CloseWindow (int typeID) {
		int len = Stage.EntityCounts[EntityLayer.UI];
		var entities = Stage.Entities[EntityLayer.UI];
		for (int i = 0; i < len; i++) {
			var e = entities[i];
			if (e.TypeID == typeID) e.Active = false;
		}
	}

}