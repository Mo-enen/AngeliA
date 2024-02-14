using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {

	public class StoveA : Furniture {
		public override void FillPhysics () { }
	}
	public class StoveB : Furniture {
		public override void FillPhysics () { }
	}
	public class StoveC : Furniture {
		public override void FillPhysics () { }
	}
	public class StoveD : Furniture {
		public override void FillPhysics () { }
	}

	public class StoveCabinetA : StoveCabinet { }
	public class StoveCabinetB : StoveCabinet { }
	public class StoveCabinetC : StoveCabinet { }
	public class StoveCabinetD : StoveCabinet { }


	public abstract class StoveCabinet : OpenableFurniture {



	}
}
