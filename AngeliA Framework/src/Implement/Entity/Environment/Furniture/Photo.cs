using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
public class Photo : Furniture, ICombustible, IActionTarget {

	private int PhotoIndex = 0;
	int ICombustible.BurnStartFrame { get; set; }

	public override void OnActivated () {
		base.OnActivated();
		PhotoIndex = Util.RandomInt(0, int.MaxValue);
	}

	public override void FillPhysics () {
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}

	public override void FrameUpdate () {
		if (Renderer.TryGetSpriteFromGroup(TypeID, PhotoIndex, out var sprite, true, false)) {
			var cell = Renderer.Draw(sprite, RenderingRect);
			if ((this as IActionTarget).IsHighlighted) {
				BlinkCellAsFurniture(cell);
			}
		}
	}

	void IActionTarget.Invoke () => PhotoIndex++;

	bool IActionTarget.AllowInvoke () => true;


}
