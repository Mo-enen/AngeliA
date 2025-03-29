namespace AngeliA;


[System.Flags]
public enum Tag : int {

	/// <summary>
	/// No tag
	/// </summary>
	None = 0,

	/// <summary>
	/// Oneway gate facing upward
	/// </summary>
	OnewayUp = 1 << 0,
	/// <summary>
	/// Oneway gate facing downward
	/// </summary>
	OnewayDown = 1 << 1,
	/// <summary>
	/// Oneway gate facing leftward
	/// </summary>
	OnewayLeft = 1 << 2,
	/// <summary>
	/// Oneway gate facing rightward
	/// </summary>
	OnewayRight = 1 << 3,

	/// <summary>
	/// Can be climb by characters and allow them move horizontaly
	/// </summary>
	Climb = 1 << 4,
	/// <summary>
	/// Can be climb by characters with fixed horizontal position
	/// </summary>
	ClimbStable = 1 << 5,

	/// <summary>
	/// Used in general perpose
	/// </summary>
	Mark = 1 << 6,
	/// <summary>
	/// Target is water
	/// </summary>
	Water = 1 << 7,
	/// <summary>
	/// Target is slippery
	/// </summary>
	Slip = 1 << 8,
	/// <summary>
	/// Target can be slide as wall
	/// </summary>
	Slide = 1 << 9,
	/// <summary>
	/// Target can not be slide as wall
	/// </summary>
	NoSlide = 1 << 10,
	/// <summary>
	/// Target can be grab from below
	/// </summary>
	GrabTop = 1 << 11,
	/// <summary>
	/// Target can be grab from side
	/// </summary>
	GrabSide = 1 << 12,

	/// <summary>
	/// Target can not be break or pick
	/// </summary>
	Unbreackable = 1 << 13,

	/// <summary>
	/// Target cloth require limb behind to be render
	/// </summary>
	ShowLimb = 1 << 14,
	/// <summary>
	/// Target cloth require limb behind not be render
	/// </summary>
	HideLimb = 1 << 15,

	/// <summary>
	/// Target sprite is the start frame of the loop
	/// </summary>
	LoopStart = 1 << 16,
	/// <summary>
	/// When painting sprite from this group with map editor. It randomly select one sprite from this group and paint.
	/// </summary>
	Random = 1 << 17,
	/// <summary>
	/// Target sprite is a palette for pixel editing.
	/// </summary>
	Palette = 1 << 18,

	/// <summary>
	/// General type of damage
	/// </summary>
	PhysicalDamage = 1 << 19,
	/// <summary>
	/// Damage from explosion
	/// </summary>
	ExplosiveDamage = 1 << 20,
	/// <summary>
	/// Damage from magic
	/// </summary>
	MagicalDamage = 1 << 21,
	/// <summary>
	/// Damage from poison
	/// </summary>
	PoisonDamage = 1 << 22,
	/// <summary>
	/// Damage from fire
	/// </summary>
	FireDamage = 1 << 23,
	/// <summary>
	/// Damage from ice
	/// </summary>
	IceDamage = 1 << 24,
	/// <summary>
	/// Damage from electricity
	/// </summary>
	LightenDamage = 1 << 25,

}


/// <summary>
/// Utility class for tags
/// </summary>
public static class TagUtil {

	// Const
	/// <summary>
	/// All damage tags without explosive damage
	/// </summary>
	public const Tag NonExplosiveDamage = Tag.PhysicalDamage | Tag.MagicalDamage | Tag.PoisonDamage | Tag.FireDamage | Tag.IceDamage | Tag.LightenDamage;
	/// <summary>
	/// Add damage tags
	/// </summary>
	public const Tag AllDamages = Tag.ExplosiveDamage | Tag.PhysicalDamage | Tag.MagicalDamage | Tag.PoisonDamage | Tag.FireDamage | Tag.IceDamage | Tag.LightenDamage;
	/// <summary>
	/// Total count of tags
	/// </summary>
	public const int TAG_COUNT = 26;
	/// <summary>
	/// Enum name of tags
	/// </summary>
	public static readonly string[] ALL_TAG_NAMES = [

		Tag.OnewayUp.ToString(),
		Tag.OnewayDown.ToString(),
		Tag.OnewayLeft.ToString(),
		Tag.OnewayRight.ToString(),

		Tag.Climb.ToString(),
		Tag.ClimbStable.ToString(),

		Tag.Mark.ToString(),
		Tag.Water.ToString(),
		Tag.Slip.ToString(),
		Tag.Slide.ToString(),
		Tag.NoSlide.ToString(),
		Tag.GrabTop.ToString(),
		Tag.GrabSide.ToString(),
		Tag.Unbreackable.ToString(),

		Tag.ShowLimb.ToString(),
		Tag.HideLimb.ToString(),

		Tag.LoopStart.ToString(),
		Tag.Random.ToString(),
		Tag.Palette.ToString(),

		Tag.PhysicalDamage.ToString(),
		Tag.ExplosiveDamage.ToString(),
		Tag.MagicalDamage.ToString(),
		Tag.PoisonDamage.ToString(),
		Tag.FireDamage.ToString(),
		Tag.IceDamage.ToString(),
		Tag.LightenDamage.ToString(),

	];

	// API
	/// <summary>
	/// Get tag at given index
	/// </summary>
	public static Tag GetTagAt (int index) => (Tag)(1 << index);

	/// <summary>
	/// True if source tag contains all tags from mask
	/// </summary>
	public static bool HasAll (this Tag self, Tag mask) => (self & mask) == mask;

	/// <summary>
	/// True if source tag contains any tag from mask
	/// </summary>
	public static bool HasAny (this Tag self, Tag mask) => (self & mask) != 0;


}