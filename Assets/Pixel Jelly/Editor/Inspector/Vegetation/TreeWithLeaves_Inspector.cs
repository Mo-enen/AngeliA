using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PixelJelly;
using PixelJelly.Editor;


namespace PixelJelly.Editor {
	[CustomJellyInspector(typeof(TreeWithLeaves))]
	public class TreeWithLeaves_Inspector : TreeTrunk_Inspector {


		public override void OnPropertySwape (SerializedObject serializedObject) {
			base.OnPropertySwape(serializedObject);
			var sMode = (serializedObject.targetObject as TreeWithLeaves).ScaleMode;
			SwapeProperty("m_LeafWidth", sMode == SpriteScaleMode.Original ? "" : ".");
			SwapeProperty("m_LeafHeight", sMode == SpriteScaleMode.Original ? "" : ".");


		}


	}
}
