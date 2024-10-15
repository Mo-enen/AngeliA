using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[AttributeUsage(AttributeTargets.Method)]
public class OnGameInitializeAttribute (int order = 0) : OrderedAttribute(order) { }


[AttributeUsage(AttributeTargets.Method)]
public class OnGameInitializeLaterAttribute (int order = 0) : OrderedAttribute(order) { }


[AttributeUsage(AttributeTargets.Method)]
public class OnGameUpdateAttribute (int order = 0) : OrderedAttribute(order) { }


[AttributeUsage(AttributeTargets.Method)]
public class OnGameUpdateLaterAttribute (int order = 0) : OrderedAttribute(order) { }


[AttributeUsage(AttributeTargets.Method)]
public class OnGameUpdatePauselessAttribute (int order = 0) : OrderedAttribute(order) { }


[AttributeUsage(AttributeTargets.Method)]
public class OnGameRestartAttribute (int order = 0) : OrderedAttribute(order) { }


[AttributeUsage(AttributeTargets.Method)]
public class OnGameTryingToQuitAttribute (int order = 0) : OrderedAttribute(order) { }


[AttributeUsage(AttributeTargets.Method)]
public class OnGameQuittingAttribute (int order = 0) : OrderedAttribute(order) { }


[AttributeUsage(AttributeTargets.Method)]
public class OnGameFocusedAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class OnGameLostFocusAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class OnWindowSizeChangedAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class OnFileDroppedAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class OnMainSheetReloadAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class OnRemoteSettingChangedAttribute : Attribute { }


// Slot
[AttributeUsage(AttributeTargets.Method)]
public class BeforeSavingSlotChanged (int order = 0) : OrderedAttribute(order) { }


[AttributeUsage(AttributeTargets.Method)]
public class OnSavingSlotChanged (int order = 0) : OrderedAttribute(order) { }


// Stage
[AttributeUsage(AttributeTargets.Method)]
public class OnViewZChangedAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class BeforeLayerFrameUpdateAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class AfterLayerFrameUpdateAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)] public class BeforeFirstUpdateAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Method)] public class BeforeBeforeUpdateAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Method)] public class BeforeUpdateUpdateAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Method)] public class BeforeLateUpdateAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Method)] public class AfterLateUpdateAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Method)] public class AfterEntityRepositionAttribute : Attribute { }


// World
[AttributeUsage(AttributeTargets.Method)]
public class BeforeLevelRenderedAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class AfterLevelRenderedAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class OnWorldCreatedAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public class OnWorldLoadedAttribute : Attribute { }


// Language
[AttributeUsage(AttributeTargets.Method)]
public class OnLanguageChangedAttribute : Attribute { }


// Cheat
[AttributeUsage(AttributeTargets.Method)]
public class OnCheatPerformAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CheatCodeAttribute (string code, object param = null) : Attribute {
	public string Code = code;
	public object Param = param;
}


// Map Editor
[AttributeUsage(AttributeTargets.Method)]
public class OnMapEditorEditModeChangedAttribute : Attribute { }
