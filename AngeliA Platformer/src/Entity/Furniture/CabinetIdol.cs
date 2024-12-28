using System.Collections;
using System.Collections.Generic;


using AngeliA;
namespace AngeliA.Platformer;


public abstract class CabinetIdol : Furniture, IActionTarget {



	private static readonly SavingInt s_ArtworkIndex = new("CabinetIdol.ArtworkIndex", 0, SavingLocation.Slot);


	public override void LateUpdate () {
		if (Renderer.TryGetSpriteFromGroup(
			TypeID, s_ArtworkIndex.Value, out var sprite, true, true
		)) {
			IActionTarget.DrawActionTarget(this, sprite, RenderingRect);
		}
	}


	bool IActionTarget.Invoke () {
		s_ArtworkIndex.Value++;
		if (Renderer.HasSpriteGroup(TypeID, out int length)) {
			s_ArtworkIndex.Value = s_ArtworkIndex.Value.UMod(length);
		}
		return true;
	}


	bool IActionTarget.AllowInvoke () => true;


}
