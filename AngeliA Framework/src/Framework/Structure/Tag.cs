namespace AngeliA;


[System.Flags]
public enum Tag : int {

	None = 0,

	OnewayUp = 1 << 0,
	OnewayDown = 1 << 1,
	OnewayLeft = 1 << 2,
	OnewayRight = 1 << 3,
	//Oneway = OnewayUp | OnewayDown | OnewayLeft | OnewayRight,

	Climb = 1 << 4,
	ClimbStable = 1 << 5,

	Quicksand = 1 << 6,
	Water = 1 << 7,
	Slip = 1 << 8,
	Slide = 1 << 9,
	NoSlide = 1 << 10,
	GrabTop = 1 << 11,
	GrabSide = 1 << 12,
	Grab = 1 << 13,

	ShowLimb = 1 << 14,
	HideLimb = 1 << 15,

	LoopStart = 1 << 16,
	Random = 1 << 17,
	Palette = 1 << 18,

	GeneralDamage = 1 << 19,
	ExplosiveDamage = 1 << 20,
	MagicalDamage = 1 << 21,
	PoisonDamage = 1 << 22,

}


public static class TagUtil {

	public static Tag GetTagAt (int index) => (Tag)(1 << index);

	public static readonly Tag Damage = Tag.GeneralDamage | Tag.ExplosiveDamage | Tag.MagicalDamage | Tag.PoisonDamage;

	public static bool HasTag (this Tag self, Tag flag) => (self & flag) == flag;

	public const int TAG_COUNT = 23;

	public static readonly string[] ALL_TAG_NAMES = new string[TAG_COUNT] {

		Tag.OnewayUp.ToString(),
		Tag.OnewayDown.ToString(),
		Tag.OnewayLeft.ToString(),
		Tag.OnewayRight.ToString(),

		Tag.Climb.ToString(),
		Tag.ClimbStable.ToString(),

		Tag.Quicksand.ToString(),
		Tag.Water.ToString(),
		Tag.Slip.ToString(),
		Tag.Slide.ToString(),
		Tag.NoSlide.ToString(),
		Tag.GrabTop.ToString(),
		Tag.GrabSide.ToString(),
		Tag.Grab.ToString(),

		Tag.ShowLimb.ToString(),
		Tag.HideLimb.ToString(),

		Tag.LoopStart.ToString(),
		Tag.Random.ToString(),
		Tag.Palette.ToString(),

		Tag.GeneralDamage.ToString(),
		Tag.ExplosiveDamage.ToString(),
		Tag.MagicalDamage.ToString(),
		Tag.PoisonDamage.ToString(),

	};

}