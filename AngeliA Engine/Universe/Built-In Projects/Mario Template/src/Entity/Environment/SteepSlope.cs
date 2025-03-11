using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[NoItemCombination]
public class SteepSlopeA : Slope {
	public override Direction2 DirectionVertical => Direction2.Up;
	public override Direction2 DirectionHorizontal => Direction2.Right;
}

[NoItemCombination]
public class SteepSlopeB : Slope {
	public override Direction2 DirectionVertical => Direction2.Up;
	public override Direction2 DirectionHorizontal => Direction2.Left;
}

[NoItemCombination]
public class SteepSlopeC : Slope {
	public override Direction2 DirectionVertical => Direction2.Down;
	public override Direction2 DirectionHorizontal => Direction2.Right;
}

[NoItemCombination]
public class SteepSlopeD : Slope {
	public override Direction2 DirectionVertical => Direction2.Down;
	public override Direction2 DirectionHorizontal => Direction2.Left;
}

