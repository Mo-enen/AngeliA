using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
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