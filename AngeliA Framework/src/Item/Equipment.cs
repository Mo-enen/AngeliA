using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


/// <summary>
/// Represent the type of the equipment
/// </summary>
public enum EquipmentType {
	/// <summary>
	/// Equipment that equip into hand slot and can be use as a tool
	/// </summary>
	HandTool,
	/// <summary>
	/// Equipment that equip into body slot
	/// </summary>
	BodyArmor,
	/// <summary>
	/// Equipment that equip into head slot 
	/// </summary>
	Helmet,
	/// <summary>
	/// Equipment that equip into foot slot
	/// </summary>
	Shoes,
	/// <summary>
	/// Equipment that equip into hand slot
	/// </summary>
	Gloves,
	/// <summary>
	/// Equipment that equip into jewelry slot. Do not render onto the character by default.
	/// </summary>
	Jewelry,
}


/// <summary>
/// Represent an item that can be equip into a slot for a character
/// </summary>
[EntityAttribute.MapEditorGroup("ItemEquipment")]
public abstract class Equipment : Item {

	/// <summary>
	/// Which type is this quipment
	/// </summary>
	public abstract EquipmentType EquipmentType { get; }
	public override int MaxStackCount => 1;

}