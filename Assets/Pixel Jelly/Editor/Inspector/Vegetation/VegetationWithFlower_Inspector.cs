using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace PixelJelly.Editor {
	[CustomJellyInspector(typeof(VegetationWithFlower))]
	public class VegetationWithFlower_Inspector : JellyInspector {





		public override void OnPropertySwape (SerializedObject serializedObject) {
			var target = serializedObject.targetObject as VegetationWithFlower;
			int fCount = target.FlowerCount.y;
			var style = target.FlowerStyle;
			SwapeProperty("m_FlowerRangeX", fCount > 0 && style == VegetationWithFlower.FlowerStyles.Full ? "." : "");
			SwapeProperty("m_FlowerRangeY", fCount > 0 ? style == VegetationWithFlower.FlowerStyles.Full ? "." : "Flower Range" : "");
			SwapeProperty("m_FlowerSize", fCount > 0 ? "." : "");
			SwapeProperty("m_Petal", fCount > 0 ? "." : "");
			SwapeProperty("m_Stamen", fCount > 0 ? "." : "");
		}





	}
}
