using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Furniture that allows the user to perform given logic as IActionTarget
/// </summary>
public abstract class ActionFurniture : Furniture, IActionTarget {

	bool IActionTarget.IsHighlighted => GetIsHighlighted();

	public override void LateUpdate () {
		//base.LateUpdate();
		if (Pose == FittingPose.Unknown) return;
		var sprite = GetSpriteFromPose();
		float pivotX = 0.5f;
		if (ModuleType == Direction3.Horizontal) {
			if (Pose == FittingPose.Left) {
				pivotX = 1f;
			} else if (Pose == FittingPose.Right) {
				pivotX = 0f;
			}
		}
		bool useHorizontal = ModuleType != Direction3.Horizontal || Pose != FittingPose.Mid;
		bool useVertical = ModuleType != Direction3.Vertical || Pose == FittingPose.Up;
		IActionTarget.DrawActionTarget(this, sprite, Rect, pivotX, 0f, useHorizontal, useVertical);
	}

	/// <summary>
	/// Perform the internal logic
	/// </summary>
	/// <returns></returns>
	public abstract bool Invoke ();

	/// <summary>
	/// True if the user can perform the logic currently
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowInvoke () => PlayerSystem.Selecting != null && !PlayerSystem.IgnoringInput && !PlayerSystem.IgnoringAction;

}
