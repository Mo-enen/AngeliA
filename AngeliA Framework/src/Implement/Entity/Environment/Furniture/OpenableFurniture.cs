namespace AngeliA.Framework; 
public abstract class OpenableUiFurniture : OpenableFurniture {


	protected virtual Int2 WindowSize => new(800, 600);
	private static int UiUpdateFrame = -1;

	public override void FrameUpdate () {
		// Close Check
		if (Open && (Player.Selecting == null || PlayerMenuUI.ShowingUI || FrameTask.HasTask())) {
			SetOpen(false);
		}
		// UI
		if (Open && UiUpdateFrame < Game.GlobalFrame) {
			UiUpdateFrame = Game.GlobalFrame;
			CursorSystem.RequireCursor();
			var size = WindowSize;
			CellRenderer.SetLayerToUI();
			FrameUpdateUI(new IRect(
				CellRenderer.CameraRect.CenterX() - Unify(size.x) / 2,
				CellRenderer.CameraRect.CenterY() - Unify(size.y) / 2,
				Unify(size.x), Unify(size.y)
			));
			CellRenderer.SetLayerToDefault();
		}
		base.FrameUpdate();
	}


	protected override void SetOpen (bool open) {
		if (open && !Open) {
			UiUpdateFrame = Game.GlobalFrame;
			OnUiOpen();
		} else if (!open && Open) {
			OnUiClose();
		}
		base.SetOpen(open);
	}


	protected abstract void FrameUpdateUI (IRect windowRect);
	protected virtual void OnUiOpen () { }
	protected virtual void OnUiClose () { }


	protected static int Unify (int value) => CellGUI.Unify(value);


}



public abstract class OpenableFurniture : Furniture, IActionTarget {


	public bool Open { get; private set; } = false;
	bool IActionTarget.LockInput => Open;
	bool IActionTarget.IsHighlighted => !Open && GetIsHighlighted();


	public override void FrameUpdate () {

		var act = this as IActionTarget;
		if (CellRenderer.TryGetSpriteFromGroup(TypeID, Open ? 1 : 0, out var sprite)) {
			var cell = CellRenderer.Draw(sprite, RenderingRect, act.AllowInvoke() ? Color32.WHITE : Color32.WHITE_96);
			if (act.IsHighlighted) {
				BlinkCellAsFurniture(cell);
			}
		}

		if (Open) {
			// Cancel
			if (FrameInput.GameKeyUp(Gamekey.Select)) {
				SetOpen(false);
				FrameInput.UseGameKey(Gamekey.Select);
			}
			ControlHintUI.AddHint(Gamekey.Select, BuiltInText.UI_CANCEL, int.MinValue + 1);
		}
	}


	void IActionTarget.Invoke () {
		if (!Open) SetOpen(true);
	}


	bool IActionTarget.AllowInvoke () => true;


	protected virtual void SetOpen (bool open) {
		Open = open;
		for (Furniture i = FurnitureLeftOrDown; i != null; i = i.FurnitureLeftOrDown) {
			if (i is OpenableFurniture oFur) oFur.Open = open;
		}
		for (Furniture i = FurnitureRightOrUp; i != null; i = i.FurnitureRightOrUp) {
			if (i is OpenableFurniture oFur) oFur.Open = open;
		}
	}


}