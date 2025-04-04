using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Furniture that functions as a vertical expanding wardrobe
/// </summary>
public abstract class Wardrobe : InventoryFurniture<InventoryPartnerUI> {
	protected sealed override Direction3 ModuleType => Direction3.Vertical;
	protected override int InventoryColumn => 5;
	protected override int InventoryRow => 10;
}
