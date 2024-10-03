using System.Collections;
using System.Collections.Generic;

namespace AngeliA; 


public enum EquipmentType { Weapon, BodyArmor, Helmet, Shoes, Gloves, Jewelry, }


[EntityAttribute.MapEditorGroup("ItemEquipment")]
public abstract class Equipment : Item {

	public abstract EquipmentType EquipmentType { get; }
	public override int MaxStackCount => 1;

}