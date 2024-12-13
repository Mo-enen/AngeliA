using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public partial class GameEditor {


	// MSG
	private void DrawItemPanel (ref IRect panelRect) {

		int minWidth = Unify(396);
		if (panelRect.width < minWidth) {
			panelRect.xMin -= minWidth - panelRect.width;
		}
		panelRect.height = Unify(618);
		panelRect.y -= panelRect.height;

		// Content





	}


}
