using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public static class PlatformerUtil {

	
	public static Int2 NavigationFreeWandering (Int2 aimPosition, Entity target, out bool grounded, int frequency, int maxDistance) {

		int insIndex = target.InstanceOrder;
		int freeShiftX = Util.QuickRandomWithSeed(
			target.TypeID + (insIndex + (Game.GlobalFrame / frequency)) * target.TypeID
		) % maxDistance;

		// Find Available Ground
		int offsetX = freeShiftX + Const.CEL * ((insIndex % 12) / 2 + 2) * (insIndex % 2 == 0 ? -1 : 1);
		if (Navigation.ExpandTo(
			Game.GlobalFrame, Stage.ViewRect,
			aimPosition.x, aimPosition.y,
			aimPosition.x + offsetX, aimPosition.y + Const.HALF,
			maxIteration: 12,
			out int groundX, out int groundY
		)) {
			aimPosition.x = groundX;
			aimPosition.y = groundY;
			grounded = true;
		} else {
			aimPosition.x += offsetX;
			grounded = false;
		}

		return aimPosition;
	}


}
