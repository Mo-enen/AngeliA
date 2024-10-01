namespace AngeliA;
public interface IActionTarget {
	public virtual bool LockInput => false;
	public bool IsHighlighted => PlayerSystem.Selecting != null && PlayerSystem.TargetActionEntity == this;
	public bool AllowInvokeOnStand => true;
	public bool AllowInvokeOnSquat => false;
	public bool Invoke ();
	public bool AllowInvoke () => true;
	public void BlinkIfHighlight (Cell cell, float pivotX = 0.5f, float pivotY = 0f, bool horizontal = true, bool vertical = true) {
		if (!IsHighlighted) return;
		if (Game.GlobalFrame % 30 > 15) return;
		const int OFFSET = Const.CEL / 20;
		cell.ReturnPivots(pivotX, pivotY);
		if (horizontal) cell.Width += OFFSET * 2;
		if (vertical) cell.Height += OFFSET * 2;
	}
}