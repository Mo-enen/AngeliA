using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

namespace Yaya {


	public enum PhysicsLayer {
		Level = 0,      // Ground, Water, OnewayGate...
		Environment = 1,// Barrel, Chest, EventTrigger...
		Item = 2,       // HealthPotion, Coin, BouncyBall...
		Character = 3,  // Player, Enemy, NPC...

	}



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


		public static readonly int PHYSICS_LAYER_COUNT = System.Enum.GetNames(typeof(PhysicsLayer)).Length;

		public static readonly int CLIMB_TAG = "Climb".AngeHash();
		public static readonly int ITEM_TAG = "Item".AngeHash();
		public static readonly int WATER_TAG = "Water".AngeHash();

		// Language
		public delegate string StringIntHandler (int key);
		public static StringIntHandler GetLanguage { get; set; } = null;

		private static readonly int QuitConfirmContentID = "Dialog.QuitConfirmContent".AngeHash();
		private static readonly int LabelOKID = "Dialog.Ok".AngeHash();
		private static readonly int LabelCancelID = "Dialog.Cancel".AngeHash();
		private static readonly int LabelQuitID = "Dialog.Quit".AngeHash();

		// Dialog
		public static string QuitConfirmContent => GetLanguage(QuitConfirmContentID);
		public static string LabelOK => GetLanguage(LabelOKID);
		public static string LabelCancel => GetLanguage(LabelCancelID);
		public static string LabelQuit => GetLanguage(LabelQuitID);

	}

}
