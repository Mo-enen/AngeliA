using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// A type of equipment that EquipmentType always set to Jewelry
/// </summary>
public abstract class Jewelry : Equipment {
	public sealed override EquipmentType EquipmentType => EquipmentType.Jewelry;
}
