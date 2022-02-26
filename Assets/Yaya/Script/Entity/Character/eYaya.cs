using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public class YayaRenderer : CharacterRenderer {
		public YayaRenderer (eCharacter ch) : base(ch) { }
	}


	public class eYaya : ePlayer {

		public override CharacterMovement Movement => _Movement ??= new(this) {
			Width = 150,
			Height = 150,
		};
		public override CharacterRenderer Renderer => _Renderer ??= new(this) {
			
		};

		[AngeliaInspector] CharacterMovement _Movement = null;
		[AngeliaInspector] YayaRenderer _Renderer = null;

	}
}
