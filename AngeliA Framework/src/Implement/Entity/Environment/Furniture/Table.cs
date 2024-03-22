using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 

public class DiningTableA : Table, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}

public class DiningTableB : Table, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}

public class DiningTableC : Table, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}

public class DiningTableD : Table, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}


public abstract class Table : Furniture {
	protected override Direction3 ModuleType => Direction3.Horizontal;
}
