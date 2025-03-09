using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


// General
public class ConveyorWoodLeft : ConveyorWood { protected override int MoveSpeed => PSwitch.Triggering ? 0 : -12; }

public class ConveyorWoodRight : ConveyorWood { protected override int MoveSpeed => PSwitch.Triggering ? 0 : 12; }


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



// On Off
public class ConveyorOnLeft : ConveyorOnOff {
	protected override int MoveSpeed => PSwitch.Triggering ? 0 : OnOffSwitch.CurrentOn ? -22 : 22;
	protected override bool ReverseOnOff => false;
}


public class ConveyorOnRight : ConveyorOnOff {
	protected override int MoveSpeed => PSwitch.Triggering ? 0 : OnOffSwitch.CurrentOn ? 22 : -22;
	protected override bool ReverseOnOff => false;
}


public class ConveyorOffLeft : ConveyorOnOff {
	protected override int MoveSpeed => PSwitch.Triggering ? 0 : OnOffSwitch.CurrentOn ? -22 : 22;
	protected override bool ReverseOnOff => true;
}


public class ConveyorOffRight : ConveyorOnOff {
	protected override int MoveSpeed => PSwitch.Triggering ? 0 : OnOffSwitch.CurrentOn ? 22 : -22;
	protected override bool ReverseOnOff => true;
}


[NoItemCombination]
public abstract class ConveyorOnOff : Conveyor {

	private static readonly SpriteCode CODE_ON_L = "ConveyorOn Left";
	private static readonly SpriteCode CODE_ON_M = "ConveyorOn Mid";
	private static readonly SpriteCode CODE_ON_R = "ConveyorOn Right";
	private static readonly SpriteCode CODE_ON_S = "ConveyorOn Single";
	private static readonly SpriteCode CODE_OFF_L = "ConveyorOff Left";
	private static readonly SpriteCode CODE_OFF_M = "ConveyorOff Mid";
	private static readonly SpriteCode CODE_OFF_R = "ConveyorOff Right";
	private static readonly SpriteCode CODE_OFF_S = "ConveyorOff Single";

	protected override int ArtCodeLeft => ReverseOnOff == OnOffSwitch.CurrentOn ? CODE_OFF_L : CODE_ON_L;
	protected override int ArtCodeMid => ReverseOnOff == OnOffSwitch.CurrentOn ? CODE_OFF_M : CODE_ON_M;
	protected override int ArtCodeRight => ReverseOnOff == OnOffSwitch.CurrentOn ? CODE_OFF_R : CODE_ON_R;
	protected override int ArtCodeSingle => ReverseOnOff == OnOffSwitch.CurrentOn ? CODE_OFF_S : CODE_ON_S;

	protected abstract bool ReverseOnOff { get; }

}