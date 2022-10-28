using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.InputSystem.LowLevel;


namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace Yaya {
	public static class YayaConst {


		public static readonly int DEFAULT_PARTICLE_CODE = typeof(eDefaultParticle).AngeHash();
		public const int MAP_VERSION = 0;
		public const int PASSOUT_WAIT = 48;

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

		// Step
		public const int STEP_ROUTE = 0;
		public const int STEP_CUTSCENE = 1;
		public const int STEP_BG_RENDERING = 2;
		public static readonly int OPENING_ID = typeof(sOpening).AngeHash();
		public static readonly int FADEOUT_ID = typeof(sFadeOut).AngeHash();
		public static readonly int SQUAD_STEP_ID = typeof(sSetViewZStep).AngeHash();

		// View
		public const int PLAYER_VIEW_LERP_RATE = 96;
		public const int VIEW_PRIORITY_PLAYER = int.MinValue;
		public const int VIEW_PRIORITY_SYSTEM = 0;
		public const int VIEW_PRIORITY_CUTSCENE = 128;

		// Misc
		public static readonly int UI_PIXEL = "UI Pixel".AngeHash();
		public static readonly Dictionary<GamepadButton, int> GAMEPAD_CODE = new() {
			{ GamepadButton.DpadLeft, "k_Gamepad Left".AngeHash()},
			{ GamepadButton.DpadRight, "k_Gamepad Right".AngeHash()},
			{ GamepadButton.DpadUp, "k_Gamepad Up".AngeHash()},
			{ GamepadButton.DpadDown, "k_Gamepad Down".AngeHash()},
			{ GamepadButton.South, "k_Gamepad South".AngeHash()},
			{ GamepadButton.North, "k_Gamepad North".AngeHash()},
			{ GamepadButton.East, "k_Gamepad East".AngeHash()},
			{ GamepadButton.West, "k_Gamepad West".AngeHash()},
			{ GamepadButton.Select, "k_Gamepad Select".AngeHash()},
			{ GamepadButton.Start, "k_Gamepad Start".AngeHash()},
			{ GamepadButton.LeftTrigger, "k_Gamepad LeftTrigger".AngeHash()},
			{ GamepadButton.RightTrigger, "k_Gamepad RightTrigger".AngeHash()},
			{ GamepadButton.LeftShoulder, "k_Gamepad LeftShoulder".AngeHash()},
			{ GamepadButton.RightShoulder, "k_Gamepad RightShoulder".AngeHash()},
		};
	}


	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class FirstSelectedPlayerAttribute : System.Attribute { }


	public interface IDamageReceiver {
		void TakeDamage (int damage);
	}


	public enum FittingPose {
		Unknown = 0,
		Left = 1,
		Down = 1,
		Mid = 2,
		Right = 3,
		Up = 3,
		Single = 4,
	}
}