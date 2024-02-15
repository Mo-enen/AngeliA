using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
public class CabinetIdolWood : CabinetIdol, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}

public abstract class CabinetIdol : Furniture, IActionTarget {



	private static readonly SavingInt s_ArtworkIndex = new("CabinetIdol.ArtworkIndex", 0);


	public override void FrameUpdate () {
		if (CellRenderer.TryGetSpriteFromGroup(
			TypeID, s_ArtworkIndex.Value, out var sprite, true, true
		)) {
			var cell = CellRenderer.Draw(sprite, RenderingRect);
			if ((this as IActionTarget).IsHighlighted) {
				IActionTarget.HighlightBlink(cell);
			}
		}
	}


	void IActionTarget.Invoke () {
		s_ArtworkIndex.Value++;
		if (CellRenderer.HasSpriteGroup(TypeID, out int length)) {
			s_ArtworkIndex.Value = s_ArtworkIndex.Value.UMod(length);
		}
	}


	bool IActionTarget.AllowInvoke () => true;


}
