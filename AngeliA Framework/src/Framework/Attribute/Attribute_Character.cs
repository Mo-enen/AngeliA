using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;



[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DefaultBodyGadgetAttribute : Attribute {
	public BodyGadgetType Type;
	public string TargetGadgetName;
	public DefaultBodyGadgetAttribute (BodyGadgetType type, string targetGadgetName) {
		Type = type;
		TargetGadgetName = targetGadgetName;
	}
}


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DefaultClothAttribute : Attribute {
	public ClothType Type;
	public string TargetClothName;
	public DefaultClothAttribute (ClothType type, string targetClothName) {
		Type = type;
		TargetClothName = targetClothName;
	}
}


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DefaultCharacterAnimationAttribute : Attribute {
	public int CharacterID;
	public CharacterAnimationType Type;
	public DefaultCharacterAnimationAttribute (Type character, CharacterAnimationType type) {
		CharacterID = character.AngeHash();
		Type = type;
	}
}


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DefaultCharacterHandheldAnimationAttribute : Attribute {
	public int CharacterID;
	public WeaponHandheld Held;
	public DefaultCharacterHandheldAnimationAttribute (Type character, WeaponHandheld held) {
		CharacterID = character.AngeHash();
		Held = held;
	}
}


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DefaultCharacterAttackAnimationAttribute : Attribute {
	public int CharacterID;
	public WeaponType Type;
	public DefaultCharacterAttackAnimationAttribute (Type character, WeaponType type) {
		CharacterID = character.AngeHash();
		Type = type;
	}
}