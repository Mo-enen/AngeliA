using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaForUnity {
	public static class ExtensionUnity {

		public static Float2 ToAngelia (this Vector2 v) => new(v.x, v.y);
		public static Float3 ToAngelia (this Vector3 v) => new(v.x, v.y, v.z);
		public static Float4 ToAngelia (this Vector4 v) => new(v.x, v.y, v.z, v.w);
		public static Int2 ToAngelia (this Vector2Int v) => new(v.x, v.y);
		public static Int3 ToAngelia (this Vector3Int v) => new(v.x, v.y, v.z);

		public static void DestroyAllChildrenImmediate (this Transform target) {
			int childCount = target.childCount;
			for (int i = 0; i < childCount; i++) {
				Object.DestroyImmediate(target.GetChild(0).gameObject, false);
			}
		}

	}
}