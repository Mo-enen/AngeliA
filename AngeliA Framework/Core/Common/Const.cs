using System.Collections;
using System.Collections.Generic;


namespace AngeliA {


	public static class RenderLayer {

		public const int WALLPAPER = 0;
		public const int BEHIND = 1;
		public const int SHADOW = 2;
		public const int DEFAULT = 3;
		public const int COLOR = 4;
		public const int MULT = 5;
		public const int ADD = 6;
		public const int UI = 7;

		public const int COUNT = 8;


		public static readonly string[] NAMES = new string[COUNT] {
			"Wallpaper", "Behind", "Shadow", "Default", "Color", "Mult", "Add", "UI",
		};

		public static readonly int[] CAPACITY = {
			WALLPAPER_CAPACITY,
			BEHIND_CAPACITY,
			SHADOW_CAPACITY,
			DEFAULT_CAPACITY,
			COLOR_CAPACITY,
			MULT_CAPACITY,
			ADD_CAPACITY,
			UI_CAPACITY,
			TOP_UI_CAPACITY,
		};
		public const int WALLPAPER_CAPACITY = 256;
		public const int BEHIND_CAPACITY = 8192;
		public const int SHADOW_CAPACITY = 4096;
		public const int DEFAULT_CAPACITY = 8192;
		public const int COLOR_CAPACITY = 256;
		public const int MULT_CAPACITY = 128;
		public const int ADD_CAPACITY = 128;
		public const int UI_CAPACITY = 4096;
		public const int TOP_UI_CAPACITY = 256;

	}


	public static class EntityLayer {
		public static readonly string[] LAYER_NAMES = new string[COUNT] {
			"Game", "Character", "Environment", "Bullet", "Item", "Decorate", "UI",
		};
		public const int GAME = 0;
		public const int CHARACTER = 1;
		public const int ENVIRONMENT = 2;
		public const int BULLET = 3;
		public const int ITEM = 4;
		public const int DECORATE = 5;
		public const int UI = 6;
		public const int COUNT = 7;
	}


	public static class PhysicsLayer {
		public const int LEVEL = 0;
		public const int ENVIRONMENT = 1;
		public const int ITEM = 2;
		public const int CHARACTER = 3;
		public const int DAMAGE = 4;
		public const int COUNT = 5;
	}


	public static class PhysicsMask {
		public const int NONE = 0;
		public const int LEVEL = 0b1;
		public const int ENVIRONMENT = 0b10;
		public const int ITEM = 0b100;
		public const int CHARACTER = 0b1000;
		public const int DAMAGE = 0b10000;

		public const int ENTITY = ENVIRONMENT | CHARACTER;
		public const int DYNAMIC = ENVIRONMENT | ITEM | CHARACTER;
		public const int SOLID = LEVEL | ENVIRONMENT | CHARACTER;
		public const int MAP = LEVEL | ENVIRONMENT;
	}


	public static class SpriteTag {

		public const string ONEWAY_UP_STRING = "OnewayUp";
		public const string ONEWAY_DOWN_STRING = "OnewayDown";
		public const string ONEWAY_LEFT_STRING = "OnewayLeft";
		public const string ONEWAY_RIGHT_STRING = "OnewayRight";
		public const string CLIMB_STRING = "Climb";
		public const string CLIMB_STABLE_STRING = "ClimbStable";
		public const string QUICKSAND_STRING = "Quicksand";
		public const string WATER_STRING = "Water";
		public const string SLIP_STRING = "Slip";
		public const string SLIDE_STRING = "Slide";
		public const string NO_SLIDE_STRING = "NoSlide";
		public const string GRAB_TOP_STRING = "GrabTop";
		public const string GRAB_SIDE_STRING = "GrabSide";
		public const string GRAB_STRING = "Grab";
		public const string SHOW_LIMB_STRING = "ShowLimb";
		public const string HIDE_LIMB_STRING = "HideLimb";
		public const string DAMAGE_STRING = "Damage";
		public const string DAMAGE_EXPLOSIVE_STRING = "ExplosiveDamage";
		public const string DAMAGE_MAGICAL_STRING = "MagicalDamage";
		public const string DAMAGE_POISON_STRING = "PoisonDamage";

		public static readonly int ONEWAY_UP_TAG = ONEWAY_UP_STRING.AngeHash();
		public static readonly int ONEWAY_DOWN_TAG = ONEWAY_DOWN_STRING.AngeHash();
		public static readonly int ONEWAY_LEFT_TAG = ONEWAY_LEFT_STRING.AngeHash();
		public static readonly int ONEWAY_RIGHT_TAG = ONEWAY_RIGHT_STRING.AngeHash();
		public static readonly int CLIMB_TAG = CLIMB_STRING.AngeHash();
		public static readonly int CLIMB_STABLE_TAG = CLIMB_STABLE_STRING.AngeHash();
		public static readonly int QUICKSAND_TAG = QUICKSAND_STRING.AngeHash();
		public static readonly int WATER_TAG = WATER_STRING.AngeHash();
		public static readonly int SLIP_TAG = SLIP_STRING.AngeHash();
		public static readonly int SLIDE_TAG = SLIDE_STRING.AngeHash();
		public static readonly int NO_SLIDE_TAG = NO_SLIDE_STRING.AngeHash();
		public static readonly int GRAB_TOP_TAG = GRAB_TOP_STRING.AngeHash();
		public static readonly int GRAB_SIDE_TAG = GRAB_SIDE_STRING.AngeHash();
		public static readonly int GRAB_TAG = GRAB_STRING.AngeHash();
		public static readonly int SHOW_LIMB_TAG = SHOW_LIMB_STRING.AngeHash();
		public static readonly int HIDE_LIMB_TAG = HIDE_LIMB_STRING.AngeHash();
		public static readonly int DAMAGE_TAG = DAMAGE_STRING.AngeHash();
		public static readonly int DAMAGE_EXPLOSIVE_TAG = DAMAGE_EXPLOSIVE_STRING.AngeHash();
		public static readonly int DAMAGE_MAGICAL_TAG = DAMAGE_MAGICAL_STRING.AngeHash();
		public static readonly int DAMAGE_POISON_TAG = DAMAGE_POISON_STRING.AngeHash();

		public static readonly string[] ALL_TAGS_STRING = new string[20]{
			ONEWAY_UP_STRING,
			ONEWAY_DOWN_STRING,
			ONEWAY_LEFT_STRING,
			ONEWAY_RIGHT_STRING,
			CLIMB_STRING,
			CLIMB_STABLE_STRING,
			QUICKSAND_STRING,
			WATER_STRING,
			SLIP_STRING,
			SLIDE_STRING,
			NO_SLIDE_STRING,
			GRAB_TOP_STRING,
			GRAB_SIDE_STRING,
			GRAB_STRING,
			SHOW_LIMB_STRING,
			HIDE_LIMB_STRING,
			DAMAGE_STRING,
			DAMAGE_EXPLOSIVE_STRING,
			DAMAGE_MAGICAL_STRING,
			DAMAGE_POISON_STRING,
		};
		public static readonly int[] ALL_TAGS = new int[20]{
			ONEWAY_UP_TAG,
			ONEWAY_DOWN_TAG,
			ONEWAY_LEFT_TAG,
			ONEWAY_RIGHT_TAG,
			CLIMB_TAG,
			CLIMB_STABLE_TAG,
			QUICKSAND_TAG,
			WATER_TAG,
			SLIP_TAG,
			SLIDE_TAG,
			NO_SLIDE_TAG,
			GRAB_TOP_TAG,
			GRAB_SIDE_TAG,
			GRAB_TAG,
			SHOW_LIMB_TAG,
			HIDE_LIMB_TAG,
			DAMAGE_TAG,
			DAMAGE_EXPLOSIVE_TAG,
			DAMAGE_MAGICAL_TAG,
			DAMAGE_POISON_TAG,
		};

	}


	public static class Const {

		public const int CEL = 256;
		public const int HALF = CEL / 2;
		public const int MAP = 128;
		public const int ART_CEL = 16;
		public const int ART_SCALE = CEL / ART_CEL;

		// View
		public const int LEVEL_SPAWN_PADDING_UNIT = 8;
		public const int SPAWN_PADDING_UNIT = 16;
		public const int ANTI_SPAWN_PADDING_UNIT = 2;
		public const int LEVEL_SPAWN_PADDING = LEVEL_SPAWN_PADDING_UNIT * CEL;
		public const int SPAWN_PADDING = SPAWN_PADDING_UNIT * CEL;
		public const int ANTI_SPAWN_PADDING = ANTI_SPAWN_PADDING_UNIT * CEL;
		public const int VIEW_RATIO = 2000; // width / height * 1000

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
		public static readonly Byte4 WHITE = new(255, 255, 255, 255);
		public static readonly Byte4 WHITE_128 = new(255, 255, 255, 128);
		public static readonly Byte4 WHITE_96 = new(255, 255, 255, 96);
		public static readonly Byte4 WHITE_64 = new(255, 255, 255, 64);
		public static readonly Byte4 WHITE_12 = new(255, 255, 255, 12);
		public static readonly Byte4 WHITE_0 = new(255, 255, 255, 0);
		public static readonly Byte4 ORANGE = new(255, 128, 0, 255);
		public static readonly Byte4 ORANGE_BETTER = new(255, 200, 100, 255);
		public static readonly Byte4 YELLOW = new(255, 255, 0, 255);
		public static readonly Byte4 BLACK = new(0, 0, 0, 255);
		public static readonly Byte4 BLACK_128 = new(0, 0, 0, 128);
		public static readonly Byte4 BLACK_12 = new(0, 0, 0, 12);
		public static readonly Byte4 RED = new(255, 0, 0, 255);
		public static readonly Byte4 RED_BETTER = new(255, 64, 64, 255);
		public static readonly Byte4 GREEN = new(0, 255, 0, 255);
		public static readonly Byte4 CYAN = new(0, 255, 255, 255);
		public static readonly Byte4 CYAN_BETTER = new(32, 232, 255, 255);
		public static readonly Byte4 BLUE = new(0, 0, 255, 255);
		public static readonly Byte4 BLUE_BETTER = new(12, 24, 255, 255);
		public static readonly Byte4 PURPLE = new(128, 0, 255, 255);
		public static readonly Byte4 PINK = new(255, 0, 255, 255);
		public static readonly Byte4 PURPLE_BETTER = new(176, 94, 196, 255);
		public static readonly Byte4 CLEAR = new(0, 0, 0, 0);
		public static readonly Byte4 GREY_230 = new(230, 230, 230, 255);
		public static readonly Byte4 GREY_216 = new(216, 216, 216, 255);
		public static readonly Byte4 GREY_196 = new(196, 196, 196, 255);
		public static readonly Byte4 GREY_128 = new(128, 128, 128, 255);
		public static readonly Byte4 GREY_96 = new(96, 96, 96, 255);
		public static readonly Byte4 GREY_64 = new(64, 64, 64, 255);
		public static readonly Byte4 GREY_56 = new(56, 56, 56, 255);
		public static readonly Byte4 GREY_42 = new(42, 42, 42, 255);
		public static readonly Byte4 GREY_32 = new(32, 32, 32, 255);
		public static readonly Byte4 GREY_20 = new(20, 20, 20, 255);
		public static readonly Byte4 GREY_12 = new(12, 12, 12, 255);
		public static readonly Byte4 SKIN_YELLOW = new(245, 217, 196, 255);
		public static readonly bool[] SliceIgnoreCenter = { false, false, false, false, true, false, false, false, false, };

		// Screen Effect
		public const int SCREEN_EFFECT_CHROMATIC_ABERRATION = 0;
		public const int SCREEN_EFFECT_TINT = 1;
		public const int SCREEN_EFFECT_RETRO_DARKEN = 2;
		public const int SCREEN_EFFECT_RETRO_LIGHTEN = 3;
		public const int SCREEN_EFFECT_VIGNETTE = 4;
		public const int SCREEN_EFFECT_GREYSCALE = 5;
		public const int SCREEN_EFFECT_INVERT = 6;
		public const int SCREEN_EFFECT_COUNT = 7;
		public static readonly string[] SCREEN_EFFECT_NAMES = new string[SCREEN_EFFECT_COUNT] {
			"Chromatic Aberration",
			"Tint",
			"Retro Darken",
			"Retro Lighten",
			"Vignette",
			"Greyscale",
			"Invert",
		};

		// Cursor
		public const int CURSOR_HAND = 0;
		public const int CURSOR_MOVE = 1;
		public const int CURSOR_BEAM = 2;
		public const int CURSOR_COUNT = 3;

		// Misc
		public static void EmptyMethod () { }
		public const int MIN_CHARACTER_HEIGHT = 65;
		public const int MAX_CHARACTER_HEIGHT = 251;

		// GUI
		public const char CONTROL_CUT = (char)24;
		public const char CONTROL_COPY = (char)3;
		public const char CONTROL_PASTE = (char)22;
		public const char RETURN_SIGN = '\r';
		public const char BACKSPACE_SIGN = '\b';

	}
}