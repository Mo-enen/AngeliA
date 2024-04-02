namespace AngeliA;
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

	public const int COUNT = 20;

	public static readonly string[] ALL_TAGS_STRING = new string[COUNT]{
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
	public static readonly int[] ALL_TAGS = new int[COUNT]{
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