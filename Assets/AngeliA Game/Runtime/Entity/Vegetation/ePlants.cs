using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaGame {


	[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL * 2)]
	public class eCactus : Plant {
		protected override bool Breath => false;
	}


	[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL * 2)]
	public class eCoral : Plant { }


	[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL)]
	public class eFiddleLeaf : Plant { }


	public class eFlower : Plant { }


	public class eMushroomPlant : Plant { }


	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
	public class ePaspalum : Plant { }


	public class eShrub : Plant {
		protected override int MinSize => 12;
		protected override int MaxSize => 24;
	}


}
