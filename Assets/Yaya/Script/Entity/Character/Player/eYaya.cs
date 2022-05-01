using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityBounds(-Const.CELL_SIZE / 2, 0, Const.CELL_SIZE, Const.CELL_SIZE * 2)]
	public class eYaya : ePlayer {

		public override CharacterMovement Movement => _Movement;
		public override CharacterRenderer Renderer => _Renderer;

		private CharacterMovement _Movement = null;
		private YayaRenderer _Renderer = null;

		public override void OnInitialize () {
			base.OnInitialize();
			_Movement = new(this) {

			};
			_Renderer = new(this) {

			};
		}

	}
}