using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


// Game
[AttributeUsage(AttributeTargets.Method)] public class OnGameInitializeAttribute : Attribute { public int Order; public OnGameInitializeAttribute (int order = 0) => Order = order; }
[AttributeUsage(AttributeTargets.Method)] public class OnGameInitializeLaterAttribute : Attribute { public int Order; public OnGameInitializeLaterAttribute (int order = 0) => Order = order; }
[AttributeUsage(AttributeTargets.Method)] public class OnGameUpdateAttribute : OrderedAttribute { public OnGameUpdateAttribute (int order = 0) : base(order) { } }
[AttributeUsage(AttributeTargets.Method)] public class OnGameUpdateLaterAttribute : OrderedAttribute { public OnGameUpdateLaterAttribute (int order = 0) : base(order) { } }
[AttributeUsage(AttributeTargets.Method)] public class OnGameUpdatePauselessAttribute : OrderedAttribute { public OnGameUpdatePauselessAttribute (int order = 0) : base(order) { } }
[AttributeUsage(AttributeTargets.Method)] public class OnGameRestartAttribute : OrderedAttribute { public OnGameRestartAttribute (int order = 0) : base(order) { } }
[AttributeUsage(AttributeTargets.Method)] public class OnGameTryingToQuitAttribute : OrderedAttribute { public OnGameTryingToQuitAttribute (int order = 0) : base(order) { } }
[AttributeUsage(AttributeTargets.Method)] public class OnGameQuittingAttribute : OrderedAttribute { public OnGameQuittingAttribute (int order = 0) : base(order) { } }
[AttributeUsage(AttributeTargets.Method)] public class OnGameFocusedAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Method)] public class OnGameLostFocusAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Method)] public class OnFileDroppedAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Method)] public class OnSheetReloadAttribute : Attribute { }


// Project
[AttributeUsage(AttributeTargets.Assembly)]
public class ToolApplicationAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Assembly)]
public class DisablePauseAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Assembly)]
public class IgnoreArtworkPixelsAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Assembly)]
public class PlayerCanNotRestartGameAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class EntityLayerCapacityAttribute : Attribute {
	public int Layer;
	public int Capacity;
	public EntityLayerCapacityAttribute (int layer, int capacity) {
		Layer = layer;
		Capacity = capacity;
	}
}


[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class RenderLayerCapacityAttribute : Attribute {
	public int Layer;
	public int Capacity;
	public RenderLayerCapacityAttribute (int layer, int capacity) {
		Layer = layer;
		Capacity = capacity;
	}
}


// Character Default
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


// Stage
[AttributeUsage(AttributeTargets.Method)] public class OnViewZChangedAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Method)] public class BeforeLayerFrameUpdateAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Method)] public class AfterLayerFrameUpdateAttribute : Attribute { }


// Item
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute : Attribute {
	public Type ItemA = null;
	public Type ItemB = null;
	public Type ItemC = null;
	public Type ItemD = null;
	public int Count = 1;
	public bool ConsumeA = true;
	public bool ConsumeB = true;
	public bool ConsumeC = true;
	public bool ConsumeD = true;
	public ItemCombinationAttribute (Type itemA, int count = 1, bool consumeA = true) : this(itemA, null, null, null, count, consumeA, true, true, true) { }
	public ItemCombinationAttribute (Type itemA, Type itemB, int count = 1, bool consumeA = true, bool consumeB = true) : this(itemA, itemB, null, null, count, consumeA, consumeB, true, true) { }
	public ItemCombinationAttribute (Type itemA, Type itemB, Type itemC, int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true) : this(itemA, itemB, itemC, null, count, consumeA, consumeB, consumeC, true) { }
	public ItemCombinationAttribute (Type itemA, Type itemB, Type itemC, Type itemD, int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true) {
		ItemA = itemA;
		ItemB = itemB;
		ItemC = itemC;
		ItemD = itemD;
		Count = count;
		ConsumeA = consumeA;
		ConsumeB = consumeB;
		ConsumeC = consumeC;
		ConsumeD = consumeD;
	}
}


// Animation
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

