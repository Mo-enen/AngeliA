using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Furniture that functions as a horizontal expanding table
/// </summary>
public abstract class Table : Furniture {
	protected sealed override Direction3 ModuleType => Direction3.Horizontal;
}
