using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eCommonBurner : Burner<eCommonFire> {
		protected override int FireFrameOffset => 0;
	}
	public class eCommonBurnerAlt : Burner<eCommonFire> {
		protected override int FireFrameOffset => FireFrequency / 2;
	}
}
