using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.DontDespawnWhenOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	public class ePauseMenu : MenuUI {


		public override RectInt Rect {
			get {
				var cameraRect = CellRenderer.CameraRect;
				return new RectInt(
					cameraRect.x + cameraRect.width / 2 - 250 * UNIT,
					cameraRect.y + cameraRect.height / 2 - 150 * UNIT,
					500 * UNIT,
					300 * UNIT
				);
			}
		}
		protected override Color32 ScreenTint => new(0, 0, 0, 128);


		protected override void DrawMenu () {



		}



	}
}