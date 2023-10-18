using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;


namespace AngeliaFramework {
	public static class Const {

		public const int CEL = 256;
		public const int HALF = CEL / 2;
		public const int MAP = 128;
		public const int ART_CEL = 16;

		// View
		public const int LEVEL_SPAWN_PADDING_UNIT = 8;
		public const int SPAWN_PADDING_UNIT = 16;
		public const int ANTI_SPAWN_PADDING_UNIT = 2;
		public const int LEVEL_SPAWN_PADDING = LEVEL_SPAWN_PADDING_UNIT * CEL;
		public const int SPAWN_PADDING = SPAWN_PADDING_UNIT * CEL;
		public const int ANTI_SPAWN_PADDING = ANTI_SPAWN_PADDING_UNIT * CEL;
		public const int SQUAD_BEHIND_PARALLAX = 1300;
		public const int DEFAULT_HEIGHT = 21 * CEL;
		public const int MIN_HEIGHT = 16 * CEL;
		public const int MAX_HEIGHT = 60 * CEL;
		public const int VIEW_RATIO = 2000; // width / height * 1000
		public const byte SQUAD_BEHIND_ALPHA = 64;

		// File
		public const string MAP_FILE_EXT = "ibb";
		public const string CONVERSATION_FILE_EXT = "txt";
		public const string LANGUAGE_FILE_EXT = "txt";

		// Entity
		public const int ENTITY_LAYER_GAME = 0;
		public const int ENTITY_LAYER_BULLET = 1;
		public const int ENTITY_LAYER_ITEM = 2;
		public const int ENTITY_LAYER_DECORATE = 3;
		public const int ENTITY_LAYER_UI = 4;
		public const int ENTITY_LAYER_COUNT = 5;

		// Physics
		public const int LAYER_LEVEL = 0;
		public const int LAYER_ENVIRONMENT = 1;
		public const int LAYER_ITEM = 2;
		public const int LAYER_CHARACTER = 3;
		public const int LAYER_DAMAGE = 4;
		public const int PHYSICS_LAYER_COUNT = 5;

		public const int MASK_NONE = 0;
		public const int MASK_LEVEL = 0b1;
		public const int MASK_ENVIRONMENT = 0b10;
		public const int MASK_ITEM = 0b100;
		public const int MASK_CHARACTER = 0b1000;
		public const int MASK_DAMAGE = 0b10000;

		public const int MASK_RIGIDBODY = MASK_ENVIRONMENT | MASK_CHARACTER;
		public const int MASK_SOLID = MASK_LEVEL | MASK_ENVIRONMENT | MASK_CHARACTER;
		public const int MASK_MAP = MASK_LEVEL | MASK_ENVIRONMENT;
		public const int MASK_ENTITY = MASK_ENVIRONMENT | MASK_ITEM | MASK_CHARACTER;

		// Tag
		public static readonly int ONEWAY_UP_TAG = "OnewayUp".AngeHash();
		public static readonly int ONEWAY_DOWN_TAG = "OnewayDown".AngeHash();
		public static readonly int ONEWAY_LEFT_TAG = "OnewayLeft".AngeHash();
		public static readonly int ONEWAY_RIGHT_TAG = "OnewayRight".AngeHash();
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
		public static readonly int SHOW_LIMB_TAG = "ShowLimb".AngeHash();
		public static readonly int HIDE_LIMB_TAG = "HideLimb".AngeHash();

		// Team
		public const int TEAM_ENVIRONMENT = 0b1;
		public const int TEAM_NEUTRAL = 0b10;
		public const int TEAM_PLAYER = 0b100;
		public const int TEAM_ENEMY = 0b1000;
		public const int TEAM_ALL = 0b1111;

		// Rendering
		public const int ORIGINAL_SIZE = int.MaxValue;
		public const int ORIGINAL_SIZE_NEGATAVE = int.MinValue;
		public const int PIXEL = 11254534;//"Pixel".AngeHash();
		public static readonly int MENU_SELECTION_CODE = "Menu Selection Mark".AngeHash();
		public static readonly int MENU_MORE_CODE = "Menu More Mark".AngeHash();
		public static readonly int MENU_ARROW_MARK = "Menu Arrow Mark".AngeHash();
		public static readonly int CIRCLE_16 = "Circle16".AngeHash();
		public static readonly Color32 WHITE = new(255, 255, 255, 255);
		public static readonly Color32 WHITE_128 = new(255, 255, 255, 128);
		public static readonly Color32 WHITE_96 = new(255, 255, 255, 96);
		public static readonly Color32 WHITE_64 = new(255, 255, 255, 64);
		public static readonly Color32 ORANGE = new(255, 128, 0, 255);
		public static readonly Color32 ORANGE_BETTER = new(255, 165, 50, 255);
		public static readonly Color32 BLACK = new(0, 0, 0, 255);
		public static readonly Color32 RED = new(255, 0, 0, 255);
		public static readonly Color32 RED_BETTER = new(255, 64, 64, 255);
		public static readonly Color32 GREEN = new(0, 255, 0, 255);
		public static readonly Color32 CYAN = new(0, 255, 255, 255);
		public static readonly Color32 BLUE = new(0, 0, 255, 255);
		public static readonly Color32 CLEAR = new(0, 0, 0, 0);
		public static readonly Color32 GREY_196 = new(196, 196, 196, 255);
		public static readonly Color32 GREY_128 = new(128, 128, 128, 255);
		public static readonly Color32 GREY_96 = new(96, 96, 96, 255);
		public static readonly Color32 GREY_64 = new(64, 64, 64, 255);
		public static readonly Color32 GREY_56 = new(56, 56, 56, 255);
		public static readonly Color32 GREY_42 = new(42, 42, 42, 255);
		public static readonly Color32 GREY_32 = new(32, 32, 32, 255);
		public static readonly Color32 GREY_12 = new(12, 12, 12, 255);
		public const int SHADER_WALLPAPER = 0;
		public const int SHADER_BEHIND = 1;
		public const int SHADER_CELL = 2;
		public const int SHADER_MULT = 3;
		public const int SHADER_ADD = 4;
		public const int SHADER_UI = 5;
		public const int SHADER_TOP_UI = 6;

		// Cursor
		public const int CURSOR_HAND = 0;
		public const int CURSOR_MOVE = 1;
		public const int CURSOR_BEAM = 2;

		// Language
		public static readonly int UI_OK = "UI.OK".AngeHash();
		public static readonly int UI_YES = "UI.Yes".AngeHash();
		public static readonly int UI_CANCEL = "UI.Cancel".AngeHash();
		public static readonly int UI_BACK = "UI.Back".AngeHash();
		public static readonly int UI_QUIT = "UI.Quit".AngeHash();
		public static readonly int UI_RESTART = "UI.Restart".AngeHash();
		public static readonly int UI_GAMEOVER = "UI.GameOver".AngeHash();
		public static readonly int UI_LEFT = "UI.Left".AngeHash();
		public static readonly int UI_RIGHT = "UI.Right".AngeHash();
		public static readonly int UI_DOWN = "UI.Down".AngeHash();
		public static readonly int UI_UP = "UI.Up".AngeHash();
		public static readonly int UI_NONE = "UI.None".AngeHash();

		// Gamepad Code
		public static int GAMEPAD_JUMP_HINT_CODE => GAMEPAD_CODE.TryGetValue(FrameInput.GetGamepadMap(Gamekey.Jump), out int _value0) ? _value0 : 0;
		internal static readonly Dictionary<GamepadButton, int> GAMEPAD_CODE = new() {
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

		// Misc
		public static void EmptyMethod () { }
		public const int MIN_CHARACTER_HEIGHT = 65;
		public const int MAX_CHARACTER_HEIGHT = 251;

		// Path
		public const string UNIVERSE_NAME = "Universe";
		public const string MANIFEST_NAME = "MANIFEST";

		public static string UniverseRoot => _UniverseRoot ??= Util.CombinePaths(
			Application.platform == RuntimePlatform.Android ? Application.persistentDataPath : Application.streamingAssetsPath,
			UNIVERSE_NAME
		);
		private static string _UniverseRoot = null;

		public static string SheetRoot => _SheetRoot ??= Util.CombinePaths(UniverseRoot, "Sheet");
		private static string _SheetRoot = null;

		public static string MetaRoot => _MetaRoot ??= Util.CombinePaths(UniverseRoot, "Meta");
		private static string _MetaRoot = null;

		public static string BuiltInMapRoot => _BuiltInMapRoot ??= Util.CombinePaths(UniverseRoot, "Map");
		private static string _BuiltInMapRoot = null;

		public static string UserMapRoot => _UserMapRoot ??= Util.CombinePaths(Application.persistentDataPath, "User Map");
		private static string _UserMapRoot = null;

		public static string ProcedureMapRoot => _ProcedureMapRoot ??= Util.CombinePaths(Application.persistentDataPath, "Procedure Map");
		private static string _ProcedureMapRoot = null;

		public static string DownloadMapRoot => _DownloadMapRoot ??= Util.CombinePaths(Application.persistentDataPath, "Download Map");
		private static string _DownloadMapRoot = null;

		public static string LanguageRoot => _LanguageRoot ??= Util.CombinePaths(UniverseRoot, "Language");
		private static string _LanguageRoot = null;

		public static string PlayerDataRoot => _PlayerDataRoot ??= Util.CombinePaths(Application.persistentDataPath, "Player Data");
		private static string _PlayerDataRoot = null;

	}
}