using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using System;

namespace Yaya {
	[EntityBounds(-Const.CELL_SIZE / 2, 0, Const.CELL_SIZE, Const.CELL_SIZE * 2)]
	public class eYaya : ePlayer {

		protected override Type RendererType => typeof(YayaRenderer);
		protected override Type MovementType => typeof(YayaMovement);


	}
}