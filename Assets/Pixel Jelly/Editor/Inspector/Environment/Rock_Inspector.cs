using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace PixelJelly.Editor {


	[CustomJellyInspector(typeof(Rock))]
	public class Rock_Inspector : JellyInspector {



		public override void OnPropertySwape (SerializedObject serializedObject) {
			var target = serializedObject.targetObject as Rock;
			SwapeProperty("m_FrameCount", "");
			SwapeProperty("m_FrameDuration", "");
			

		}



	}
}
