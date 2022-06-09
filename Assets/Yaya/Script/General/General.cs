using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

namespace Yaya {
	public static class YayaConst {


		// Physics
		public const int PHYSICS_LAYER_COUNT = 4;
		public const int LEVEL = 0;
		public const int ENVIRONMENT = 1;
		public const int ITEM = 2;
		public const int CHARACTER = 3;
		public const int MASK_NONE = 0;
		public const int MASK_LEVEL = 1 << LEVEL;
		public const int MASK_ENVIRONMENT = 1 << ENVIRONMENT;
		public const int MASK_ITEM = 1 << ITEM;
		public const int MASK_CHARACTER = 1 << CHARACTER;
		public const int MASK_RIGIDBODY = MASK_ENVIRONMENT | MASK_CHARACTER;
		public const int MASK_SOLID = MASK_LEVEL | MASK_ENVIRONMENT | MASK_CHARACTER;
		public const int MASK_MAP = MASK_LEVEL | MASK_ENVIRONMENT;

		public static readonly int CLIMB_TAG = "Climb".AngeHash();
		public static readonly int CLIMB_STABLE_TAG = "Climb Stable".AngeHash();
		public static readonly int ITEM_TAG = "Item".AngeHash();
		public static readonly int WATER_TAG = "Water".AngeHash();

	}


	[System.Serializable]
	public class CheckPointMeta {
		[System.Serializable]
		public struct Data {
			public int Index;
			public int X; // Global Unit Pos
			public int Y; // Global Unit Pos
			public bool IsAltar;
		}
		public Data[] CPs = null;
	}


}
