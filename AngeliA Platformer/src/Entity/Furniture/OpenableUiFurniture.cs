using AngeliA;

namespace AngeliA.Platformer;

public abstract class OpenableUiFurniture : OpenableFurniture {


	protected virtual Int2 WindowSize => new(800, 600);
	private static int UiUpdateFrame = -1;

	public override void LateUpdate () {
		// Close Check
		if (Open && (PlayerSystem.Selecting == null || PlayerMenuUI.ShowingUI || TaskSystem.HasTask())) {
			SetOpen(false);
		}
		// UI
		if (Open && UiUpdateFrame < Game.GlobalFrame) {
			UiUpdateFrame = Game.GlobalFrame;
			Cursor.RequireCursor();
			var size = WindowSize;
			using (new UILayerScope()) {
				UpdateUI(new IRect(
					Renderer.CameraRect.CenterX() - Unify(size.x) / 2,
					Renderer.CameraRect.CenterY() - Unify(size.y) / 2,
					Unify(size.x), Unify(size.y)
				));
			}
		}
		base.LateUpdate();
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

	protected abstract void UpdateUI (IRect windowRect);
	protected virtual void OnUiOpen () { }
	protected virtual void OnUiClose () { }

	protected static int Unify (int value) => GUI.Unify(value);

}
