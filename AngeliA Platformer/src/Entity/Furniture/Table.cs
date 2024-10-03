using System.Collections;
using System.Collections.Generic;


using AngeliA;namespace AngeliA.Platformer; 


public abstract class Table : Furniture {
	protected override Direction3 ModuleType => Direction3.Horizontal;
}
