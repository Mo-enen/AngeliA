using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class GreyscaleEffect : AngeliaScreenEffect {
		public override int Order => 1;
		public override Shader GetShader () => Shader.Find("Angelia/GreyScale");
	}
}