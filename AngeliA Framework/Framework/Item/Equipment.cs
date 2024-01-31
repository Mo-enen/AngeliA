using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	public enum EquipmentType { Weapon, BodyArmor, Helmet, Shoes, Gloves, Jewelry, }


	[EntityAttribute.MapEditorGroup("ItemEquipment")]
	public abstract class Equipment : Item {

		public abstract EquipmentType EquipmentType { get; }
		public sealed override int MaxStackCount => 1;

		public virtual int GetOverrideMovementAnimationID (CharacterAnimationType type, Character character) => 0;

	}
}