using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[AttributeUsage(AttributeTargets.Method)]
public class OnGameInitializeAttribute : OrderedAttribute {
	public OnGameInitializeAttribute (int order = 0) : base(order) { }
}


[AttributeUsage(AttributeTargets.Method)]
public class OnGameInitializeLaterAttribute : OrderedAttribute {
	public OnGameInitializeLaterAttribute (int order = 0) : base(order) { }
}


[AttributeUsage(AttributeTargets.Method)]
public class OnGameUpdateAttribute : OrderedAttribute {
	public OnGameUpdateAttribute (int order = 0) : base(order) { }
}


[AttributeUsage(AttributeTargets.Method)]
public class OnGameUpdateLaterAttribute : OrderedAttribute {
	public OnGameUpdateLaterAttribute (int order = 0) : base(order) { }
}


[AttributeUsage(AttributeTargets.Method)]
public class OnGameUpdatePauselessAttribute : OrderedAttribute {
	public OnGameUpdatePauselessAttribute (int order = 0) : base(order) { }
}


[AttributeUsage(AttributeTargets.Method)]
public class OnGameRestartAttribute : OrderedAttribute {
	public OnGameRestartAttribute (int order = 0) : base(order) { }
}


[AttributeUsage(AttributeTargets.Method)]
public class OnGameTryingToQuitAttribute : OrderedAttribute {
	public OnGameTryingToQuitAttribute (int order = 0) : base(order) { }
}


[AttributeUsage(AttributeTargets.Method)]
public class OnGameQuittingAttribute : OrderedAttribute {
	public OnGameQuittingAttribute (int order = 0) : base(order) { }
}


[AttributeUsage(AttributeTargets.Method)]
public class OnGameFocusedAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class OnGameLostFocusAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class OnFileDroppedAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class OnMainSheetReload : Attribute { }


// Slot
[AttributeUsage(AttributeTargets.Method)]
public class BeforeSavingSlotChanged : OrderedAttribute {
	public BeforeSavingSlotChanged (int order = 0) : base(order) { }
}


[AttributeUsage(AttributeTargets.Method)]
public class OnSavingSlotChanged : OrderedAttribute {
	public OnSavingSlotChanged (int order = 0) : base(order) { }
}


// Stage
[AttributeUsage(AttributeTargets.Method)]
public class OnViewZChangedAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class BeforeLayerFrameUpdateAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class AfterLayerFrameUpdateAttribute : Attribute { }


// World
[AttributeUsage(AttributeTargets.Method)]
public class BeforeLevelRenderedAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class AfterLevelRenderedAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class OnMapGeneratorInitializedAttribute : Attribute { }


// Language
[AttributeUsage(AttributeTargets.Method)]
public class OnLanguageChangedAttribute : Attribute { }


// Cheat
[AttributeUsage(AttributeTargets.Method)]
public class OnCheatPerformAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CheatCodeAttribute : Attribute {
	public string Code = "";
	public CheatCodeAttribute (string code) {
		Code = code;
	}
}

