using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.Capacity(1, 0)]
public abstract class WindowUI : EntityUI, IWindowEntityUI {


	// Api
	/// <summary>
	/// Rect position of the background part in global space
	/// </summary>
	public virtual IRect BackgroundRect => Rect;
	/// <summary>
	/// Failback display name of this type of window
	/// </summary>
	public virtual string DefaultWindowName => "";
	/// <summary>
	/// Rect position for the root boundary
	/// </summary>
	public static IRect WindowRect { get; private set; }
	/// <summary>
	/// True if the content of the window have unsaved changes
	/// </summary>
	public bool IsDirty { get; private set; } = false;
	/// <summary>
	/// Rect position in global space for the tooltip this window require to display
	/// </summary>
	public IRect RequiringTooltipRect { get; private set; } = default;
	/// <summary>
	/// Content data for the tooltip this window require to display
	/// </summary>
	public string RequiringTooltipContent { get; set; } = null;
	/// <summary>
	/// Content data for the notification this window require to display
	/// </summary>
	public string NotificationContent { get; set; } = null;
	/// <summary>
	/// Secondary content data for the notification this window require to display
	/// </summary>
	public string NotificationSubContent { get; set; } = null;
	/// <inheritdoc cref="GUI.Skin"/>
	protected GUISkin Skin => GUI.Skin;

	// Data
	private static int UpdatedFrame = -1;
	private static int WindowRectOverrideFrame = -1;


	// MSG
	[OnGameQuitting]
	internal static void OnGameQuitting () {
		if (!Stage.Enable) return;
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
				Renderer.DrawPixel(Renderer.CameraRect, new Color32(0, 0, 0, 200), int.MaxValue);
			} catch (System.Exception ex) { Debug.LogException(ex); }
			if (!oldIgnoreM) Input.CancelIgnoreMouseInput();
			if (!oldIgnoreK) Input.CancelIgnoreKeyInput();
		}
	}

	// API
	/// <summary>
	/// Set window rect for all window UI
	/// </summary>
	public static void ForceWindowRect (IRect newRect) {
		WindowRectOverrideFrame = Game.PauselessFrame;
		WindowRect = newRect;
	}

	/// <summary>
	/// Mark this window as dirty (contains unsaved changes)
	/// </summary>
	public virtual void SetDirty () => IsDirty = true;

	/// <summary>
	/// Mark this window as not dirty (do not contains unsaved changes)
	/// </summary>
	public virtual void CleanDirty () => IsDirty = false;

	/// <summary>
	/// Require save the data
	/// </summary>
	/// <param name="forceSave">True if this save performs without dirty checks</param>
	public virtual void Save (bool forceSave = false) { }

	/// <summary>
	/// Require display tooltip for given range. Call this function every frame no matter the tooltip should be currently display or not.
	/// </summary>
	protected void RequireTooltip (IRect rect, string content) {
		if (!rect.MouseInside()) return;
		RequiringTooltipRect = rect.Shift(-Input.MousePositionShift.x, -Input.MousePositionShift.y);
		RequiringTooltipContent = content;
	}

	/// <summary>
	/// Require a notification.
	/// </summary>
	/// <param name="content"></param>
	/// <param name="subContent"></param>
	protected void RequireNotification (string content, string subContent = null) {
		NotificationContent = content;
		NotificationSubContent = subContent;
	}

}