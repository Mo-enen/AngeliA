using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



namespace PixelJelly.Editor {
	[CustomJellyInspector(typeof(Cactus))]
	public class Cactus_Inspector : JellyInspector {
		public override void OnPropertySwape (SerializedObject serializedObject) {
			var style = (serializedObject.targetObject as Cactus).Style;
			SwapeProperty("m_FrameCount", "");
			SwapeProperty("m_FrameDuration", "");
			
			SwapeProperty("m_CactusHeight", style == Cactus.CactusStyle.Hierarchy ? "" : ".");
			SwapeProperty("m_AllowOverlap", style == Cactus.CactusStyle.Hierarchy ? "." : "");
		}
	}
}
