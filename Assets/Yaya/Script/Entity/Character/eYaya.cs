using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eYaya : ePlayer {

		public override CharacterMovement Movement => _Movement ??= new(this) {
			//SwimInFreeStyle = true,
		};
		public override CharacterRenderer Renderer => _Renderer ??= new(this) {

		};

		[AngeliaInspector] CharacterMovement _Movement = null;
		[AngeliaInspector] CharacterRenderer _Renderer = null;

	}
}
