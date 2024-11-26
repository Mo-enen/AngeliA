using System;
using System.Collections;

namespace AngeliA;


// Init
public class OnGameInitializeAttribute (int order = 0) : OrderedAttribute(order) { }
public class OnGameInitializeLaterAttribute (int order = 0) : OrderedAttribute(order) { }


// Slot
public class BeforeSavingSlotChanged (int order = 0) : OrderedAttribute(order) { }
public class OnSavingSlotChanged (int order = 0) : OrderedAttribute(order) { }



// Game Update
public class OnGameUpdateAttribute (int order = 0) : EventAttribute(order) { }
public class OnGameUpdateLaterAttribute (int order = 0) : EventAttribute(order) { }
public class OnGameUpdatePauselessAttribute (int order = 0) : EventAttribute(order) { }


// Game Misc
public class OnGameRestartAttribute (int order = 0) : EventAttribute(order) { }
public class OnGameTryingToQuitAttribute (int order = 0) : EventAttribute(order) { }
public class OnGameQuittingAttribute (int order = 0) : EventAttribute(order) { }
public class OnGameFocusedAttribute : EventAttribute { }
public class OnGameLostFocusAttribute : EventAttribute { }


// System
public class OnWindowSizeChangedAttribute : EventAttribute { }
public class OnRemoteSettingChangedAttribute (int order = 0) : EventAttribute(order) { }
public class OnFileDroppedAttribute : EventAttribute { }


// Rendering
public class OnMainSheetReloadAttribute : EventAttribute { }


// Stage
public class OnViewZChangedAttribute : EventAttribute { }
public class BeforeLayerFrameUpdateAttribute : EventAttribute { }
public class AfterLayerFrameUpdateAttribute : EventAttribute { }
public class BeforeFirstUpdateAttribute : EventAttribute { }
public class BeforeBeforeUpdateAttribute : EventAttribute { }
public class BeforeUpdateUpdateAttribute : EventAttribute { }
public class BeforeLateUpdateAttribute : EventAttribute { }
public class AfterLateUpdateAttribute : EventAttribute { }
public class AfterEntityRepositionAttribute : EventAttribute { }


// World
public class BeforeLevelRenderedAttribute (int order = 0) : EventAttribute(order) { }
public class AfterLevelRenderedAttribute (int order = 0) : EventAttribute(order) { }
public class OnWorldCreatedBySquadAttribute (int order = 0) : EventAttribute(order) { }
public class OnWorldLoadedBySquadAttribute (int order = 0) : EventAttribute(order) { }


// Map Editor
public class OnWorldSavedByMapEditorAttribute : EventAttribute { }
public class OnMapEditorModeChange : EventAttribute {
	public enum Mode { EnterPlayMode, ExitPlayMode, EnterEditMode, ExitEditMode, }
}


// Language
public class OnLanguageChangedAttribute : EventAttribute { }


// Cheat
public class CheatCodeAttribute (string code) : EventAttribute { public string Code = code; }
public class OnCheatPerformedAttribute : EventAttribute { }


// Item
public class OnItemCollectedAttribute : EventAttribute { }
public class OnItemLostAttribute : EventAttribute { }
public class OnItemInsufficientAttribute : EventAttribute { }
public class OnItemDamageAttribute : EventAttribute { }
public class OnItemUnlockedAttribute : EventAttribute { }


// Character
public class OnCharacterSleeping : EventAttribute { }
public class OnCharacterJump : EventAttribute { }
public class OnCharacterPound : EventAttribute { }
public class OnCharacterFly : EventAttribute { }
public class OnCharacterSlideStepped : EventAttribute { }
public class OnCharacterPassOut : EventAttribute { }
public class OnCharacterTeleport : EventAttribute { }
public class OnCharacterCrash : EventAttribute { }


// Misc
public class OnFootStepped : EventAttribute { }
public class OnObjectBreakAttribute : EventAttribute { }
public class OnObjectFreeFallAttribute : EventAttribute { }
public class OnBlockPickedAttribute : EventAttribute { }
public class OnFallIntoWaterAttribute : EventAttribute { }
public class OnCameOutOfWaterAttribute : EventAttribute { }



