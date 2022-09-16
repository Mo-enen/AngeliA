using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace PixelJelly.Editor {
	[CustomJellyInspector(typeof(Grass))]
	public class Grass_Inspector : VegetationWithFlower_Inspector {
		public override void OnPropertySwape (SerializedObject serializedObject) {
			var style = (serializedObject.targetObject as Grass).Style;
			bool isWave = style == Grass.GrassStyle.Wave;
			bool isStraight = style == Grass.GrassStyle.Straight;
			SwapeProperty("m_FrameCount", "");
			SwapeProperty("m_FrameDuration", "");
			
			SwapeProperty("m_EdgeSmooth", isWave ? "." : "");
			SwapeProperty("m_Iteration", isWave ? "." : "");
			SwapeProperty("m_IterationRadius", isWave ? "." : "");
			SwapeProperty("m_LayerCount", isWave ? "." : "");
			SwapeProperty("m_LightDirection", isWave ? "." : "");
			SwapeProperty("m_Amount", isWave ? "" : ".");
			SwapeProperty("m_ColorA", isWave || isStraight ? "Light" : ".");
			SwapeProperty("m_ColorB", isWave || isStraight ? "Basic" : ".");
			SwapeProperty("m_ColorC", isWave || isStraight ? "Dark" : ".");
			base.OnPropertySwape(serializedObject);
		}
	}
}
