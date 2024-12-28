using AngeliA;

namespace AngeliA.Platformer;

public abstract class OpenableFurniture : ActionFurniture, IActionTarget {


	public bool Open { get; private set; } = false;
	bool IActionTarget.LockInput => Open;
	bool IActionTarget.IsHighlighted => !Open && GetIsHighlighted();


	public override void OnActivated () {
		base.OnActivated();
		Open = false;
	}


	public override void LateUpdate () {

		var act = this as IActionTarget;
		if (Renderer.TryGetSpriteFromGroup(TypeID, Open ? 1 : 0, out var sprite)) {
			var cell = Renderer.Draw(sprite, RenderingRect, act.AllowInvoke() ? Color32.WHITE : Color32.WHITE_96);
			IActionTarget.MakeCellAsActionTarget(this, cell);
		}

		if (Open) {
			// Cancel
			if (Input.GameKeyUp(Gamekey.Select)) {
				SetOpen(false);
				Input.UseGameKey(Gamekey.Select);
			}

			ControlHintUI.AddHint(Gamekey.Select, BuiltInText.UI_CANCEL, int.MinValue + 1);
		}
	}


	public override bool Invoke () {
		if (!Open) {
			SetOpen(true);
		}
		return true;
	}


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