using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

namespace Yaya {


	public enum FurniturePose {
		Unknown = 0,
		Left = 1,
		Down = 1,
		Mid = 2,
		Right = 3,
		Up = 3,
		Single = 4,
	}


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
		public static readonly int ITEM_TAG = "Item".AngeHash();
		public static readonly int WATER_TAG = "Water".AngeHash();

	}

}
