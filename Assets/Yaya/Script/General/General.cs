using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

namespace Yaya {



	[System.Flags]
	public enum PhysicsMask {
		None = 0,
		Level = 1 << 0,      // Ground, Water, OnewayGate...
		Environment = 1 << 1,// Barrel, Chest, EventTrigger...
		Item = 1 << 2,       // HealthPotion, Coin, BouncyBall...
		Character = 1 << 3,  // Player, Enemy, NPC...

		Rigidbody = Environment | Character,
		Solid = Level | Environment | Character,
		Map = Level | Environment,

		Count = 4,

	}


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


		public const int LEVEL = 0;
		public const int ENVIRONMENT = 1;
		public const int ITEM = 2;
		public const int CHARACTER = 3;
		public const int PHYSICS_LAYER_COUNT = (int)PhysicsMask.Count;


		public static readonly int CLIMB_TAG = "Climb".AngeHash();
		public static readonly int ITEM_TAG = "Item".AngeHash();
		public static readonly int WATER_TAG = "Water".AngeHash();

	}

}
