using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class InvertEffect : AngeliaScreenEffect {
		public override Shader GetShader () => Shader.Find("Angelia/Invert");
		public override int Order => int.MaxValue;
	}
}