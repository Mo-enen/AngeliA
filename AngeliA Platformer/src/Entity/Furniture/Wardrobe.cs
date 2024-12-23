using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;

public abstract class Wardrobe : InventoryFurniture<InventoryPartnerUI> {
	protected override int InventoryColumn => 10;
	protected override int InventoryRow => 8;
}
