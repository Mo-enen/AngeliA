using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PixelJelly;


namespace PixelJelly.Editor {
	[CustomJellyInspector(typeof(ItemPile))]
	public class ItemPile_Inspector : JellyInspector {


		public override void OnPropertySwape (SerializedObject serializedObject) {
			SwapeProperty("m_FrameCount", "");
			SwapeProperty("m_FrameDuration", "");
		}


	}
}
