using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.Bounds(-Const.HALF, 0, Const.CEL * 2, Const.CEL * 2)]
	public class Cactus : Plant { }


	[EntityAttribute.Bounds(-Const.HALF, 0, Const.CEL * 2, Const.CEL * 2)]
	public class Coral : Plant { }


	[EntityAttribute.Bounds(-Const.HALF, 0, Const.CEL * 2, Const.CEL)]
	public class FiddleLeaf : Plant { }


	public class Flower : Plant { }


	public class MushroomPlant : Plant { }


	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
	public class Paspalum : Plant { }


	public class Shrub : Plant { }


}
