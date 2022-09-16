using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PixelJelly;


namespace PixelJelly.Editor {
	[CustomJellyInspector(typeof(Paspalum))]
	public class Paspalum_Inspector : JellyInspector {


		public override void OnPropertySwape (SerializedObject serializedObject) {
			SwapeProperty("m_FrameCount", "");
			SwapeProperty("m_FrameDuration", "");
			SwapeProperty("m_LightDirection", "Light");
			
		}


	}
}
