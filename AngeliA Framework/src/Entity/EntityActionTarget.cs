namespace AngeliA;
public interface IActionTarget {
	public virtual bool LockInput => false;
	public bool IsHighlighted => PlayerSystem.Selecting != null && PlayerSystem.TargetActionEntity == this;
	public bool AllowInvokeOnStand => true;
	public bool AllowInvokeOnSquat => false;
	public bool InvokeOnTouch => false;
	public bool Invoke ();
	public bool AllowInvoke () => true;
	public static void DrawActionTarget (IActionTarget target, AngeSprite sprite, IRect rect, float pivotX = 0.5f, float pivotY = 0f, bool blinkHorizontal = true, bool blinkVertical = true) {
		if (sprite == null) return;
		var cell = Renderer.Draw(sprite, rect);
		MakeCellAsActionTarget(target, cell, pivotX, pivotY, blinkHorizontal, blinkVertical);
	}
	public static void MakeCellAsActionTarget (IActionTarget target, Cell cell, float pivotX = 0.5f, float pivotY = 0f, bool blinkHorizontal = true, bool blinkVertical = true) {
		cell.Color = new Color32(255, 255, 255, (byte)(target.AllowInvoke() ? 255 : 128));
		if (target.IsHighlighted) {
			FrameworkUtil.HighlightBlink(cell, pivotX, pivotY, blinkHorizontal, blinkVertical);
		}
	}
}