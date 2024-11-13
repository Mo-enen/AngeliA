namespace AngeliA;
public interface IActionTarget {
	public virtual bool LockInput => false;
	public bool IsHighlighted => PlayerSystem.Selecting != null && PlayerSystem.TargetActionEntity == this;
	public bool AllowInvokeOnStand => true;
	public bool AllowInvokeOnSquat => false;
	public bool InvokeOnTouch => false;
	public bool Invoke ();
	public bool AllowInvoke () => true;
	public void BlinkIfHighlight (Cell cell, float pivotX = 0.5f, float pivotY = 0f, bool horizontal = true, bool vertical = true) {
		if (!IsHighlighted) return;
		FrameworkUtil.HighlightBlink(cell, pivotX, pivotY, horizontal, vertical);
	}
}