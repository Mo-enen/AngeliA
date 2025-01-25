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
public class OnRemoteSettingChanged_IntID_IntDataAttribute (int order = 0) : EventAttribute(order) { }
public class OnFileDropped_StringPathAttribute : EventAttribute { }


// Rendering
public class OnMainSheetReloadAttribute : EventAttribute { }


// Stage
public class OnViewZChangedAttribute : EventAttribute { }
public class BeforeLayerFrameUpdate_IntLayerAttribute : EventAttribute { }
public class AfterLayerFrameUpdate_IntLayerAttribute : EventAttribute { }
public class BeforeFirstUpdateAttribute : EventAttribute { }
public class BeforeBeforeUpdateAttribute : EventAttribute { }
public class BeforeUpdateUpdateAttribute : EventAttribute { }
public class BeforeLateUpdateAttribute : EventAttribute { }
public class AfterLateUpdateAttribute : EventAttribute { }
public class AfterEntityReposition_Entity_Int3From_Int3ToAttribute : EventAttribute { }


// World
public class BeforeLevelRenderedAttribute (int order = 0) : EventAttribute(order) { }
public class AfterLevelRenderedAttribute (int order = 0) : EventAttribute(order) { }
public class OnWorldCreatedBySquad_WorldAttribute (int order = 0) : EventAttribute(order) { }
public class OnWorldLoadedBySquad_WorldAttribute (int order = 0) : EventAttribute(order) { }


// Map Editor
public class OnWorldSavedByMapEditor_WorldAttribute : EventAttribute { }
public class OnMapEditorModeChange_ModeAttribute : EventAttribute {
	public enum Mode { EnterPlayMode, ExitPlayMode, EnterEditMode, ExitEditMode, }
}


// Language
public class OnLanguageChangedAttribute : EventAttribute { }


// Cheat
public class CheatCodeAttribute (string code) : EventAttribute { public string Code = code; }
public class OnCheatPerformed_StringCodeAttribute : EventAttribute { }


// Item 
public class OnItemCollected_Entity_Int2Pos_IntItemID_IntItemCountAttribute : EventAttribute { }
public class OnItemLost_Character_IntItemIDAttribute : EventAttribute { }
public class OnItemError_Entity_Int2Pos_IntItemIDAttribute : EventAttribute { }
public class OnItemDamage_Character_IntItemBefore_IntItemAfterAttribute : EventAttribute { }
public class OnItemUnlocked_IntItemIDAttribute : EventAttribute { }


// Character
public class OnCharacterFootStepped_Entity : EventAttribute { }
public class OnCharacterSleeping_Entity : EventAttribute { }
public class OnCharacterJump_Entity : EventAttribute { }
public class OnCharacterPound_Entity : EventAttribute { }
public class OnCharacterFly_Entity : EventAttribute { }
public class OnCharacterSlideStepped_Entity : EventAttribute { }
public class OnCharacterPassOut_Entity : EventAttribute { }
public class OnCharacterTeleport_Entity : EventAttribute { }
public class OnCharacterCrash_Entity : EventAttribute { }


// Misc
public class OnObjectBreak_IntSpriteID_IRectAttribute : EventAttribute { }
public class OnObjectFreeFall_IntSpriteID_Int2Pos_IntRot_BoolFlip_Int2Velocity_IntRotSpeed_IntGravityAttribute : EventAttribute { }
public class OnBlockPicked_IntSpriteID_IrectAttribute : EventAttribute { }
public class OnFallIntoWater_RigidbodyAttribute : EventAttribute { }
public class OnCameOutOfWater_RigidbodyAttribute : EventAttribute { }

public class OnTransferArrivedAttribute_IntEntityID_Int3UnitPos_ObjectData : EventAttribute { }
public class OnTransferPassAttribute_Int3UnitPos_ObjectData : EventAttribute { }

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CircuitOperator_Int3UnitPos_IntStamp_Direction5From : Attribute { }
public class OnCircuitWireActived_Int3UnitPosAttribute : EventAttribute { }
public class OnCircuitOperatorTriggered_Int3UnitPosAttribute : EventAttribute { }




