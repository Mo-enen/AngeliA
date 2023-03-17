using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.InputSystem.LowLevel;
using System.Data;


namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace Yaya {
	public static class YayaConst {
		

		public static readonly int DEFAULT_PARTICLE_CODE = typeof(eDefaultParticle).AngeHash();
		public const int MAP_VERSION = 0;

		// Physics
		public const int LAYER_LEVEL = 0;
		public const int LAYER_ENVIRONMENT = 1;
		public const int LAYER_ITEM = 2;
		public const int LAYER_CHARACTER = 3;
		public const int LAYER_DAMAGE = 4;
		public const int LAYER_COUNT = 5;

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

		public const int WATER_SPEED_LOSE = 400;
		public const int QUICK_SAND_JUMPOUT_SPEED = 48;
		public const int QUICK_SAND_MAX_RUN_SPEED = 4;
		public const int QUICK_SAND_SINK_SPEED = 1;
		public const int QUICK_SAND_JUMP_SPEED = 12;

		// Tag
		public static readonly int CLIMB_TAG = "Climb".AngeHash();
		public static readonly int CLIMB_STABLE_TAG = "ClimbStable".AngeHash();
		public static readonly int QUICKSAND_TAG = "Quicksand".AngeHash();
		public static readonly int WATER_TAG = "Water".AngeHash();
		public static readonly int DAMAGE_TAG = "Damage".AngeHash();
		public static readonly int SLIDE_TAG = "Slide".AngeHash();
		public static readonly int NO_SLIDE_TAG = "NoSlide".AngeHash();
		public static readonly int GRAB_TOP_TAG = "GrabTop".AngeHash();
		public static readonly int GRAB_SIDE_TAG = "GrabSide".AngeHash();
		public static readonly int GRAB_TAG = "Grab".AngeHash();

		// Task
		public const int TASK_ROUTE = 0;
		public const int TASK_MISC = 1;
		public const int TASK_LAYER_COUNT = 2;

		// View
		public const int PLAYER_VIEW_LERP_RATE = 96;
		public const int VIEW_PRIORITY_PLAYER = int.MinValue;
		public const int VIEW_PRIORITY_SYSTEM = 0;
		public const int VIEW_PRIORITY_CUTSCENE = 128;

		// Team
		public const int TEAM_NEUTRAL = -1;
		public const int TEAM_PLAYER = 0;
		public const int TEAM_ENEMY = 1;

		// Misc
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
}