using AngeliA;
using AngeliA.Platformer;

namespace MarioClone;


public class ConveyorWoodLeft : ConveyorWood { protected override int MoveSpeed => -12; }

public class ConveyorWoodRight : ConveyorWood { protected override int MoveSpeed => 12; }


[NoItemCombination]
public abstract class ConveyorWood : Conveyor, ICombustible {

	private static readonly SpriteCode CODE_L = "Conveyor Left";
	private static readonly SpriteCode CODE_M = "Conveyor Mid";
	private static readonly SpriteCode CODE_R = "Conveyor Right";
	private static readonly SpriteCode CODE_S = "Conveyor Single";

	int ICombustible.BurnStartFrame { get; set; }

	protected override int ArtCodeLeft => CODE_L;
	protected override int ArtCodeMid => CODE_M;
	protected override int ArtCodeRight => CODE_R;
	protected override int ArtCodeSingle => CODE_S;

}



public class ConveyorOnLeft : ConveyorOn { protected override int MoveSpeed => -22; }

public class ConveyorOnRight : ConveyorOn { protected override int MoveSpeed => 22; }


[NoItemCombination]
public abstract class ConveyorOn : Conveyor {

	private static readonly SpriteCode CODE_L = "ConveyorOn Left";
	private static readonly SpriteCode CODE_M = "ConveyorOn Mid";
	private static readonly SpriteCode CODE_R = "ConveyorOn Right";
	private static readonly SpriteCode CODE_S = "ConveyorOn Single";

	protected override int ArtCodeLeft => CODE_L;
	protected override int ArtCodeMid => CODE_M;
	protected override int ArtCodeRight => CODE_R;
	protected override int ArtCodeSingle => CODE_S;

}