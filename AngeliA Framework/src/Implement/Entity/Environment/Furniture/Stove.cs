using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 

public class StoveA : Furniture {
	public override void FirstUpdate () { }
}
public class StoveB : Furniture {
	public override void FirstUpdate () { }
}
public class StoveC : Furniture {
	public override void FirstUpdate () { }
}
public class StoveD : Furniture {
	public override void FirstUpdate () { }
}

public class StoveCabinetA : StoveCabinet { }
public class StoveCabinetB : StoveCabinet { }
public class StoveCabinetC : StoveCabinet { }
public class StoveCabinetD : StoveCabinet { }


public abstract class StoveCabinet : OpenableFurniture {



}
