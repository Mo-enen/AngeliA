using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

namespace Yaya {


	public interface IDamageReceiver {
		void TakeDamage (int damage);
	}


	public static class YayaConst {


		// Render
		public const int SHADER_LERP = 0;
		public const int SHADER_CELL = 1;
		public const int SHADER_ADD = 2;
		public const int SHADER_MULT = 3;
		public const int SHADER_UI = 4;

		// Physics
		public const int PHYSICS_LAYER_COUNT = 5;

		public const int LAYER_LEVEL = 0;
		public const int LAYER_ENVIRONMENT = 1;
		public const int LAYER_ITEM = 2;
		public const int LAYER_CHARACTER = 3;
		public const int LAYER_DAMAGE = 4;

		public const int MASK_NONE = 0;
		public const int MASK_LEVEL = 1 << LAYER_LEVEL;
		public const int MASK_ENVIRONMENT = 1 << LAYER_ENVIRONMENT;
		public const int MASK_ITEM = 1 << LAYER_ITEM;
		public const int MASK_CHARACTER = 1 << LAYER_CHARACTER;
		public const int MASK_DAMAGE = 1 << LAYER_DAMAGE;

		public const int MASK_RIGIDBODY = MASK_ENVIRONMENT | MASK_CHARACTER;
		public const int MASK_SOLID = MASK_LEVEL | MASK_ENVIRONMENT | MASK_CHARACTER;
		public const int MASK_MAP = MASK_LEVEL | MASK_ENVIRONMENT;
		public const int MASK_ENTITY = MASK_ENVIRONMENT | MASK_ITEM | MASK_CHARACTER;

		// Tag
		public static readonly int CLIMB_TAG = "Climb".AngeHash();
		public static readonly int CLIMB_STABLE_TAG = "Climb Stable".AngeHash();
		public static readonly int ITEM_TAG = "Item".AngeHash();
		public static readonly int WATER_TAG = "Water".AngeHash();
		public static readonly int QUICKSAND_TAG = "Quicksand".AngeHash();
		public static readonly int DAMAGE_TAG = "Damage".AngeHash();

	}



	// Data
	[System.Serializable]
	public class YayaAsset {
		public AnimationCurve SquadTransitionCurve = null;
		public AnimationCurve UiPopCurve = null;
	}



	// Meta
	[System.Serializable]
	public class YayaMeta {

		public int WaterSpeedLose = 400;
		public int QuickSandJumpoutSpeed = 48;
		public int QuickSandMaxRunSpeed = 4;
		public int QuickSandSinkSpeed = 1;
		public int QuickSandJumpSpeed = 12;

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


	// Attr
	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class FirstSelectedPlayerAttribute : System.Attribute { }



}
