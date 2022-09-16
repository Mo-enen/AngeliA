using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PixelJelly.Editor {
	[CustomJellyInspector(typeof(Shrub))]
	public class Shrub_Inspector : VegetationWithFlower_Inspector {



		public override void OnPropertySwape (SerializedObject serializedObject) {
			SwapeProperty("m_FrameDuration", "");
			SwapeProperty("m_FrameCount", "");
			
			base.OnPropertySwape(serializedObject);
		}




	}
}
