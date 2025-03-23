using System.Collections;
using System.Collections.Generic;


namespace AngeliA;

/// <summary>
/// Utility class for constant values
/// </summary>
public static class Const {

	/// <summary>
	/// Global size of a block unit
	/// </summary>
	public const int CEL = 256;
	/// <summary>
	/// Global size of a half block unit
	/// </summary>
	public const int HALF = CEL / 2;
	/// <summary>
	/// Global size of a quarter block unit
	/// </summary>
	public const int QUARTER = CEL / 4;
	/// <summary>
	/// Global size of a block unit
	/// </summary>
	public static readonly Int2 CEL2 = new(CEL, CEL);
	/// <summary>
	/// Global size of a half block unit
	/// </summary>
	public static readonly Int2 HALF2 = new(HALF, HALF);
	/// <summary>
	/// Global size of a quarter block unit
	/// </summary>
	public static readonly Int2 QUARTER2 = new(QUARTER, QUARTER);
	/// <summary>
	/// Unit size of a map file
	/// </summary>
	public const int MAP = 128;
	/// <summary>
	/// Global size of a artwork pixel
	/// </summary>
	public const int ART_CEL = 16;
	/// <summary>
	/// Size ratio between global size and artwork pixel
	/// </summary>
	public const int ART_SCALE = CEL / ART_CEL;

	// View
	/// <summary>
	/// Level block expand distance from view-rect in unit (1 unit = 256 global size)
	/// </summary>
	public const int LEVEL_SPAWN_PADDING_UNIT = 8;
	/// <summary>
	/// Entity spawning expand distance from view-rect in unit (1 unit = 256 global size)
	/// </summary>
	public const int SPAWN_PADDING_UNIT = 16;
	/// <summary>
	/// Entity anti-spawning rect expand distance from view-rect in unit (1 unit = 256 global size)
	/// </summary>
	public const int ANTI_SPAWN_PADDING_UNIT = 2;
	/// <summary>
	/// Level block expand distance from view-rect in global size
	/// </summary>
	public const int LEVEL_SPAWN_PADDING = LEVEL_SPAWN_PADDING_UNIT * CEL;
	/// <summary>
	/// Entity spawning expand distance from view-rect in global size
	/// </summary>
	public const int SPAWN_PADDING = SPAWN_PADDING_UNIT * CEL;
	/// <summary>
	/// Entity anti-spawning rect expand distance from view-rect in global size
	/// </summary>
	public const int ANTI_SPAWN_PADDING = ANTI_SPAWN_PADDING_UNIT * CEL;

	// Team
	/// <summary>
	/// Attack target team for environment
	/// </summary>
	public const int TEAM_ENVIRONMENT = 0b1;
	/// <summary>
	/// Attack target team for neutral
	/// </summary>
	public const int TEAM_NEUTRAL = 0b10;
	/// <summary>
	/// Attack target team for player
	/// </summary>
	public const int TEAM_PLAYER = 0b100;
	/// <summary>
	/// Attack target team for enemy
	/// </summary>
	public const int TEAM_ENEMY = 0b1000;
	/// <summary>
	/// Attack target team for all
	/// </summary>
	public const int TEAM_ALL = 0b1111;
	/// <summary>
	/// Attack target team total count
	/// </summary>
	public const int TEAM_COUNT = 4;

	// Rendering
	public static readonly bool[] SliceIgnoreCenter = [false, false, false, false, true, false, false, false, false,];
	/// <summary>
	/// Use the original size of the sprite from artwork sheet
	/// </summary>
	public const int ORIGINAL_SIZE = int.MaxValue;
	/// <summary>
	/// Use the original pivot of the sprite from artwork sheet
	/// </summary>
	public const int ORIGINAL_PIVOT = int.MaxValue;
	/// <summary>
	/// Use the original size * -1 of the sprite from artwork sheet
	/// </summary>
	public const int ORIGINAL_SIZE_NEGATAVE = int.MinValue;
	/// <summary>
	/// Sprite ID of the 1x1 white pixel
	/// </summary>
	public const int PIXEL = 11254534;//"Pixel".AngeHash();

	// Screen Effect
	/// <summary>
	/// Screen effect index for the tint effect
	/// </summary>
	public const int SCREEN_EFFECT_TINT = 0;
	/// <summary>
	/// Screen effect index for the retro-darken effect
	/// </summary>
	public const int SCREEN_EFFECT_RETRO_DARKEN = 1;
	/// <summary>
	/// Screen effect index for the retro-lighten effect
	/// </summary>
	public const int SCREEN_EFFECT_RETRO_LIGHTEN = 2;
	/// <summary>
	/// Screen effect index for the vignette effect
	/// </summary>
	public const int SCREEN_EFFECT_VIGNETTE = 3;
	/// <summary>
	/// Screen effect index for the grey-scale effect
	/// </summary>
	public const int SCREEN_EFFECT_GREYSCALE = 4;
	/// <summary>
	/// Screen effect index for the invert effect
	/// </summary>
	public const int SCREEN_EFFECT_INVERT = 5;
	/// <summary>
	/// Screen effect total count
	/// </summary>
	public const int SCREEN_EFFECT_COUNT = 6;
	public static readonly string[] SCREEN_EFFECT_NAMES = [
		"Tint",
		"Retro Darken",
		"Retro Lighten",
		"Vignette",
		"Greyscale",
		"Invert",
	];

	// Cursor
	public const int CURSOR_NONE = -2;
	public const int CURSOR_DEFAULT = 0;
	public const int CURSOR_ARROW = 1;
	public const int CURSOR_BEAM = 2;
	public const int CURSOR_CROSSHAIR = 3;
	public const int CURSOR_HAND = 4;
	public const int CURSOR_RESIZE_HORIZONTAL = 5;
	public const int CURSOR_RESIZE_VERTICAL = 6;
	public const int CURSOR_RESIZE_TOPLEFT = 7;
	public const int CURSOR_RESIZE_TOPRIGHT = 8;
	public const int CURSOR_RESIZE_CROSS = 9;
	public const int CURSOR_PROHIBIT = 10;
	public const int CURSOR_COUNT = 11;

	// Misc
	public static void EmptyMethod () { }
	public static bool EmptyBoolMethod () => true;

	public const int MIN_CHARACTER_HEIGHT = 65;
	public const int MAX_CHARACTER_HEIGHT = 251;
	public const int RIG_BUFFER_SIZE = 5_000_000; // 5MB
	public static readonly int EQUIPMENT_TYPE_COUNT = System.Enum.GetValues(typeof(EquipmentType)).Length;
	public const int SOUND_CHANNEL_COUNT = 6;

	// GUI
	public const char CONTROL_SELECT_ALL = (char)6;
	public const char CONTROL_CUT = (char)24;
	public const char CONTROL_COPY = (char)3;
	public const char CONTROL_PASTE = (char)22;
	public const char RETURN_SIGN = '\r';

}