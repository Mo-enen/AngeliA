namespace AngeliA;

/// <summary>
/// Interface that makes the entity react with player action. When player goes nearby, they can press action button to invoke the logic from this entity
/// </summary>
public interface IActionTarget {

	/// <summary>
	/// True if this entity lock player's input when highlighting
	/// </summary>
	public virtual bool LockInput => false;
	/// <summary>
	/// True if this entity is currently highlighting
	/// </summary>
	public bool IsHighlighted => PlayerSystem.Selecting != null && PlayerSystem.TargetActionEntity == this;
	/// <summary>
	/// True if this entity can be highlight when player is standing
	/// </summary>
	public bool AllowInvokeOnStand => true;
	/// <summary>
	/// True if this entity can be highlight when player is squatting
	/// </summary>
	public bool AllowInvokeOnSquat => false;
	/// <summary>
	/// True if this entity will be invoke when player comes nearby without pressing the action button
	/// </summary>
	public bool InvokeOnTouch => false;
	/// <summary>
	/// Invoke the logic provided by this entity.
	/// </summary>
	/// <returns>True if the logic performs successfuly</returns>
	public bool Invoke ();
	/// <summary>
	/// True if the entity can be invoke at current frame
	/// </summary>
	public bool AllowInvoke () => true;
	/// <summary>
	/// Draw the artwork sprite for given action target entity
	/// </summary>
	/// <param name="target">The action target entity</param>
	/// <param name="sprite"></param>
	/// <param name="rect">Rect position in global space</param>
	/// <param name="pivotX">Pivot X for the artwork sprite</param>
	/// <param name="pivotY">Pivot Y for the artwork sprite</param>
	/// <param name="blinkHorizontal">True if the entity blink with nearby entities together horizontaly</param>
	/// <param name="blinkVertical">True if the entity blink with nearby entities together Verticaly</param>
	public static void DrawActionTarget (IActionTarget target, AngeSprite sprite, IRect rect, float pivotX = 0.5f, float pivotY = 0f, bool blinkHorizontal = true, bool blinkVertical = true) {
		if (sprite == null) return;
		var cell = Renderer.Draw(sprite, rect);
		MakeCellAsActionTarget(target, cell, pivotX, pivotY, blinkHorizontal, blinkVertical);
	}
	/// <summary>
	/// Make the rendering cell blink like an action target entity
	/// </summary>
	/// <param name="target">The entity</param>
	/// <param name="cell">The rendering cell</param>
	/// <param name="pivotX">Pivot X for the artwork sprite</param>
	/// <param name="pivotY">Pivot Y for the artwork sprite</param>
	/// <param name="blinkHorizontal">True if the entity blink with nearby entities together horizontaly</param>
	/// <param name="blinkVertical">True if the entity blink with nearby entities together Verticaly</param>
	public static void MakeCellAsActionTarget (IActionTarget target, Cell cell, float pivotX = 0.5f, float pivotY = 0f, bool blinkHorizontal = true, bool blinkVertical = true) {
		cell.Color = new Color32(255, 255, 255, (byte)(target.AllowInvoke() ? 255 : 128));
		if (target.IsHighlighted) {
			FrameworkUtil.HighlightBlink(cell, pivotX, pivotY, blinkHorizontal, blinkVertical);
		}
	}

}