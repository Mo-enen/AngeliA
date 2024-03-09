using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
public class ChairWoodA : Chair, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}
public class ChairWoodB : Chair, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}
public class ChairWoodC : Chair, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}
public class ChairWoodD : Chair, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}

public abstract class Chair : Furniture {

	protected sealed override Direction3 ModuleType => Direction3.None;

	private bool? DockedToRight = null;

	public override void OnActivated () {
		base.OnActivated();
		DockedToRight = null;
	}

	public override void LateUpdate () {
		if (!DockedToRight.HasValue) {
			DockedToRight = !Physics.HasEntity<Table>(
				Rect.Expand(ColliderBorder).Shift(-Const.CEL, 0).Shrink(1),
				PhysicsMask.ENVIRONMENT, this, OperationMode.ColliderAndTrigger
			);
		}
		// Render
		if (Renderer.TryGetSpriteFromGroup(TypeID, 0, out var sprite, false, true)) {
			var rect = Rect.Expand(ColliderBorder);
			if (DockedToRight.HasValue && DockedToRight.Value) {
				Renderer.Draw(sprite, rect);
			} else {
				Renderer.Draw(sprite, rect.CenterX(), rect.y, 500, 0, 0, -rect.width, rect.height);
			}
		}
	}

}
