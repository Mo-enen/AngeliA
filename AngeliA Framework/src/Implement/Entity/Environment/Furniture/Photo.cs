using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
public class Photo : Furniture, ICombustible, IActionTarget {

	private int PhotoIndex = 0;
	int ICombustible.BurnStartFrame { get; set; }

	public override void OnActivated () {
		base.OnActivated();
		PhotoIndex = Util.QuickRandom(0, int.MaxValue - 1);
	}

	public override void FirstUpdate () {
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}

	public override void LateUpdate () {
		if (Renderer.TryGetSpriteFromGroup(TypeID, PhotoIndex, out var sprite, true, false)) {
			var cell = Renderer.Draw(sprite, RenderingRect);
			if ((this as IActionTarget).IsHighlighted) {
				BlinkCellAsFurniture(cell);
			}
		}
	}

	bool IActionTarget.Invoke () {
		PhotoIndex++;
		return true;
	}

	bool IActionTarget.AllowInvoke () => true;


}
