using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace PixelJelly.Editor {
	[CustomJellyInspector(typeof(Flower))]
	public class Flower_Inspector : JellyInspector {



		public override void OnPropertySwape (SerializedObject serializedObject) {

			SwapeProperty("m_FrameCount", "");
			SwapeProperty("m_FrameDuration", "");
			




		}





	}
}
