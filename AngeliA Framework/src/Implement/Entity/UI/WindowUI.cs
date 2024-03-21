using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework;

[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.Capacity(1, 0)]
public abstract class WindowUI : EntityUI, IWindowEntityUI {


	// Api
	public virtual IRect BackgroundRect => Rect;
	public static IRect WindowRect { get; private set; }

	// Data
	private static int UpdatedFrame = -1;
	private static int WindowRectOverrideFrame = -1;


	// MSG
	[OnGameQuitting]
	public static void OnGameQuitting () {
		int len = Stage.EntityCounts[EntityLayer.UI];
		var entities = Stage.Entities[EntityLayer.UI];
		for (int i = 0; i < len; i++) {
			var e = entities[i];
			if (e.Active) {
				e.OnInactivated();
			}
		}
	}

	public abstract void UpdateWindowUI ();

	public sealed override void UpdateUI () {
		base.UpdateUI();
		WindowRect = Game.PauselessFrame > WindowRectOverrideFrame ? Renderer.CameraRect : WindowRect;
		if (Game.PauselessFrame > UpdatedFrame) {
			// First
			UpdatedFrame = Game.PauselessFrame;
			UpdateWindowUI();
		} else {
			// Subsequent
			bool oldIgnoreM = Input.IgnoringMouseInput;
			bool oldIgnoreK = Input.IgnoringKeyInput;
			if (!oldIgnoreM) Input.IgnoreMouseInput();
			if (!oldIgnoreK) Input.IgnoreKeyInput();
			try {
				int oldP = Cursor.CursorPriority;
				Cursor.CursorPriority = int.MaxValue;
				UpdateWindowUI();
				Cursor.CursorPriority = oldP;
				Renderer.Draw(Const.PIXEL, Renderer.CameraRect, new Color32(0, 0, 0, 200), int.MaxValue);
			} catch (System.Exception ex) { Debug.LogException(ex); }
			if (!oldIgnoreM) Input.CancelIgnoreMouseInput();
			if (!oldIgnoreK) Input.CancelIgnoreKeyInput();
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

	public static void ForceWindowRect (IRect newRect) {
		WindowRectOverrideFrame = Game.PauselessFrame;
		WindowRect = newRect;
	}

}