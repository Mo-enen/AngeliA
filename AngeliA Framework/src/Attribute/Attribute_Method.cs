using System;
using System.Collections;

namespace AngeliA;


// Init
/// <summary>
/// The function will be called when game initialize.
/// </summary>
/// <param name="order">Function with smaller order will be called earlier</param>
/// <example><code>
/// [OnGameInitialize]
/// internal static void ExampleFunction () { }
/// </code></example>
public class OnGameInitializeAttribute (int order = 0) : OrderedAttribute(order) { }


/// <summary>
/// The function will be called when game initialize but after all [OnGameInitialize] functions already called
/// </summary>
/// <example><code>
/// [OnGameInitializeLater]
/// internal static void ExampleFunction () { }
/// </code></example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnGameInitializeLaterAttribute (int order = 0) : OrderedAttribute(order) { }


// Slot
/// <summary>
/// The function will be called before user change the saving slot. Universe.BuiltIn.CurrentSavingSlot is still the old value when this function is called.
/// </summary>
/// <example><code>
/// [BeforeSavingSlotChanged]
/// internal static void ExampleFunction () { }
/// </code></example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class BeforeSavingSlotChangedAttribute (int order = 0) : OrderedAttribute(order) { }

/// <summary>
/// The function will be called after user change the saving slot. Universe.BuiltIn.CurrentSavingSlot is the new value when this function is called.
/// </summary>
/// <example><code>
/// [OnSavingSlotChanged]
/// internal static void ExampleFunction () { }
/// </code></example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnSavingSlotChangedAttribute (int order = 0) : OrderedAttribute(order) { }


// Game Update
/// <summary>
/// The function will be called every time game update (60 times per second)
/// </summary>
/// <example><code>
/// [OnGameUpdate]
/// internal static void ExampleFunction () { }
/// </code></example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnGameUpdateAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called every time game update (60 times per second), but after all [OnGameUpdate] functions already called.
/// </summary>
/// <example><code>
/// [OnGameUpdateLater]
/// internal static void ExampleFunction () { }
/// </code></example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnGameUpdateLaterAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called every time game update (60 times per second), even when the game is paused.
/// </summary>
/// <example><code>
/// [OnGameUpdatePauseless]
/// internal static void ExampleFunction () { }
/// </code></example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnGameUpdatePauselessAttribute (int order = 0) : EventAttribute(order) { }


// Game Misc
/// <summary>
/// The function will be called when game restart.
/// </summary>
/// <example><code>
/// [OnGameRestart]
/// internal static void ExampleFunction () { }
/// </code></example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnGameRestartAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called when player try to quit the game. Return false will stop the application from quiting.
/// </summary>
/// <example><code>
/// [OnGameTryingToQuit]
/// internal static bool ExampleFunction () { return true; }
/// </code></example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnGameTryingToQuitAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called before the application actually quit.
/// </summary>
/// <example><code>
/// [OnGameQuitting]
/// internal static void ExampleFunction () { }
/// </code></example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnGameQuittingAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called when the application window regain focus.
/// </summary>
/// <example><code>
/// [OnGameFocused]
/// internal static void ExampleFunction () { }
/// </code></example>
public class OnGameFocusedAttribute : EventAttribute { }


/// <summary>
/// The function will be called when the application window lost focus.
/// </summary>
/// <example><code>
/// [OnGameLostFocus]
/// internal static void ExampleFunction () { }
/// </code></example>
public class OnGameLostFocusAttribute : EventAttribute { }


// System
/// <summary>
/// The function will be called when user change the size of the application window
/// </summary>
/// <example><code>
/// [OnWindowSizeChanged]
/// internal static void ExampleFunction () { }
/// </code></example>
public class OnWindowSizeChangedAttribute : EventAttribute { }


/// <summary>
/// The function will be called when engine send remote message to rigged game
/// </summary>
/// <example><code>
/// [OnRemoteSettingChanged_IntID_IntData]
/// internal static void ExampleFunction (int id, int data) { }
/// </code></example>
public class OnRemoteSettingChanged_IntID_IntDataAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called when user drag and drop a file into the application window
/// </summary>
/// <example><code>
/// [OnFileDropped_StringPath]
/// internal static void ExampleFunction (string filePath) { }
/// </code></example>
public class OnFileDropped_StringPathAttribute : EventAttribute { }


// Rendering
/// <summary>
/// The function will be called when artwork sheet for rendering loaded from file
/// </summary>
/// <example><code>
/// [OnMainSheetReload]
/// internal static void ExampleFunction () { }
/// </code></example>
public class OnMainSheetReloadAttribute : EventAttribute { }


// Stage
/// <summary>
/// The function will be called when player change the map layer
/// </summary>
/// <example><code>
/// [OnViewZChanged]
/// internal static void ExampleFunction () { }
/// </code></example>
public class OnViewZChangedAttribute : EventAttribute { }


/// <summary>
/// The function will be called before rendering layer update
/// </summary>
/// <example><code>
/// [BeforeLayerFrameUpdate_IntLayer]
/// internal static void ExampleFunction (int layer) { }
/// </code></example>
public class BeforeLayerFrameUpdate_IntLayerAttribute : EventAttribute { }


/// <summary>
/// The function will be called after rendering layer update 
/// </summary>
/// <example><code>
/// [AfterLayerFrameUpdate_IntLayer]
/// internal static void ExampleFunction () { }
/// </code></example>
public class AfterLayerFrameUpdate_IntLayerAttribute : EventAttribute { }


/// <summary>
/// The function will be called before any entity.FirstUpdate is called
/// </summary>
/// <example><code>
/// [BeforeFirstUpdate]
/// internal static void ExampleFunction () { }
/// </code></example>
public class BeforeFirstUpdateAttribute : EventAttribute { }


/// <summary>
/// The function will be called before any entity.BeforeUpdate is called 
/// </summary>
/// <example><code>
/// [BeforeBeforeUpdate]
/// internal static void ExampleFunction () { }
/// </code></example>
public class BeforeBeforeUpdateAttribute : EventAttribute { }


/// <summary>
/// The function will be called before any entity.Update is called 
/// </summary>
/// <example><code>
/// [BeforeUpdateUpdate]
/// internal static void ExampleFunction () { }
/// </code></example>
public class BeforeUpdateUpdateAttribute : EventAttribute { }


/// <summary>
/// The function will be called before any entity.LateUpdate is called 
/// </summary>
/// <example><code>
/// [BeforeLateUpdate]
/// internal static void ExampleFunction () { }
/// </code></example>
public class BeforeLateUpdateAttribute : EventAttribute { }


/// <summary>
/// The function will be called after all entity.LateUpdate is called 
/// </summary>
/// <example><code>
/// [AfterLateUpdate]
/// internal static void ExampleFunction () { }
/// </code></example>
public class AfterLateUpdateAttribute : EventAttribute { }


// World
/// <summary>
/// The function will be called before world squad rendering any level blocks for the current frame
/// </summary>
/// <example><code>
/// [BeforeLevelRendered]
/// internal static void ExampleFunction () { }
/// </code></example>
public class BeforeLevelRenderedAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called after world squad render all level blocks for the current frame 
/// </summary>
/// <example><code>
/// [AfterLevelRendered]
/// internal static void ExampleFunction () { }
/// </code></example>
public class AfterLevelRenderedAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called when a world instance is created by world squad
/// </summary>
/// <example><code>
/// [OnWorldCreatedBySquad_World]
/// internal static void ExampleFunction () { }
/// </code></example>
public class OnWorldCreatedBySquad_WorldAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called when a world instance is loaded by world squad 
/// </summary>
/// <example><code>
/// [OnWorldLoadedBySquad_World]
/// internal static void ExampleFunction () { }
/// </code></example>
public class OnWorldLoadedBySquad_WorldAttribute (int order = 0) : EventAttribute(order) { }


// Map Editor
/// <summary>
/// The function will be called when a world instance is saved to file by the world squad
/// </summary>
/// <example><code>
/// [OnWorldSavedByMapEditor_World]
/// internal static void ExampleFunction () { }
/// </code></example>
public class OnWorldSavedByMapEditor_WorldAttribute : EventAttribute { }


/// <summary>
/// The function will be called when user change map editor editing mode
/// </summary>
/// <example><code>
/// [OnMapEditorModeChange_Mode]
/// internal static void ExampleFunction (OnMapEditorModeChange_ModeAttribute.Mode mode) { }
/// </code></example>
public class OnMapEditorModeChange_ModeAttribute : EventAttribute {

	/// <summary></summary>
	public enum Mode {
		/// <summary>
		/// Edit mode to play mode. After map editor internal logic is done
		/// </summary>
		EnterPlayMode,

		/// <summary>
		/// Play mode to edit mode. Before map editor internal logic is done
		/// </summary>
		ExitPlayMode,

		/// <summary>
		/// Play mode to edit mode. After map editor internal logic is done
		/// </summary>
		EnterEditMode,

		/// <summary>
		/// Edit mode to play mode. Before map editor internal logic is done
		/// </summary>
		ExitEditMode,
	}
}


// Language
/// <summary>
/// The function will be called when user change game display language. Language.CurrentLanguage is already set to new value when this function is called.
/// </summary>
/// <example><code>
/// [OnLanguageChanged]
/// internal static void ExampleFunction () { }
/// </code></example>
public class OnLanguageChangedAttribute : EventAttribute { }


// Cheat
/// <summary>
/// The function will be called when user perform the given cheat code. Cheat code can be perform like those in NES games.
/// </summary>
/// <example><code>
/// [CheatCode]
/// internal static void ExampleFunction (string code) { }
/// </code></example>
public class CheatCodeAttribute (string code) : EventAttribute {
	internal string Code = code;
}


/// <summary>
/// The function will be called when user performed any cheat code. Cheat code can be perform like those in NES games.
/// </summary>
/// <example><code>
/// [OnCheatPerformed_StringCode]
/// internal static void ExampleFunction (string code) { }
/// </code></example>
public class OnCheatPerformed_StringCodeAttribute : EventAttribute { }


// Item 
/// <summary>
/// The function will be called when a character collect an item
/// </summary>
/// <example><code>
/// [OnItemCollected_Entity_Int2Pos_IntItemID_IntItemCount]
/// internal static void ExampleFunction (Entity holder, Int2 pos, int itemID, int itemCount) { }
/// </code></example>
public class OnItemCollected_Entity_Int2Pos_IntItemID_IntItemCountAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a character lost an item
/// </summary>
/// <example><code>
/// [OnItemLost_Character_IntItemID]
/// internal static void ExampleFunction (Character holder, int itemID) { }
/// </code></example>
public class OnItemLost_Character_IntItemIDAttribute : EventAttribute { }


/// <summary>
/// The function will be called when something wrong about an item (like when guns out of ammo)
/// </summary>
/// <example><code>
/// [OnItemError_Entity_Int2Pos_IntIconID]
/// internal static void ExampleFunction (Entity holder, Int2 pos, int iconID) { }
/// </code></example>
public class OnItemError_Entity_Int2Pos_IntIconID : EventAttribute { }


/// <summary>
/// The function will be called when an item is damaged into another item
/// </summary>
/// <example><code>
/// [OnItemDamage_Character_IntItemBefore_IntItemAfter]
/// internal static void ExampleFunction (Character holder, int itemIdBefore, int itemIdAfter) { }
/// </code></example>
public class OnItemDamage_Character_IntItemBefore_IntItemAfterAttribute : EventAttribute { }


/// <summary>
/// The function will be called when an item is unlocked by player
/// </summary>
/// <example><code>
/// [OnItemUnlocked_IntItemID]
/// internal static void ExampleFunction (int itemID) { }
/// </code></example>
public class OnItemUnlocked_IntItemIDAttribute : EventAttribute { }


// Character
/// <summary>
/// The function will be called when a character makes a foot step on running
/// </summary>
/// <example><code>
/// [OnCharacterFootStepped_Entity]
/// internal static void ExampleFunction (Entity character) { }
/// </code></example>
public class OnCharacterFootStepped_EntityAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a character is sleeping
/// </summary>
/// <example><code>
/// [OnCharacterSleeping_Entity]
/// internal static void ExampleFunction (Entity character) { }
/// </code></example>
public class OnCharacterSleeping_EntityAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a character jumps
/// </summary>
/// <example><code>
/// [OnCharacterJump_Entity]
/// internal static void ExampleFunction (Entity character) { }
/// </code></example>
public class OnCharacterJump_EntityAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a character ground pound
/// </summary>
/// <example><code>
/// [OnCharacterPound_Entity]
/// internal static void ExampleFunction (Entity character) { }
/// </code></example>
public class OnCharacterPound_EntityAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a character fly
/// </summary>
/// <example><code>
/// [OnCharacterFly_Entity]
/// internal static void ExampleFunction (Entity character) { }
/// </code></example>
public class OnCharacterFly_EntityAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a character makes a step when sliding
/// </summary>
/// <example><code>
/// [OnCharacterSlideStepped_Entity]
/// internal static void ExampleFunction (Entity character) { }
/// </code></example>
public class OnCharacterSlideStepped_EntityAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a character pass out
/// </summary>
/// <example><code>
/// [OnCharacterPassOut_Entity]
/// internal static void ExampleFunction (Entity character) { }
/// </code></example>
public class OnCharacterPassOut_EntityAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a character teleport
/// </summary>
/// <example><code>
/// [OnCharacterTeleport_Entity]
/// internal static void ExampleFunction (Entity character) { }
/// </code></example>
public class OnCharacterTeleport_EntityAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a character crash
/// </summary>
/// <example><code>
/// [OnCharacterCrash_Entity]
/// internal static void ExampleFunction (Entity character) { }
/// </code></example>
public class OnCharacterCrash_EntityAttribute : EventAttribute { }


// Misc
/// <summary>
/// The function will be called when an object break
/// </summary>
/// <example><code>
/// [OnObjectBreak_IntSpriteID_IRect]
/// internal static void ExampleFunction (int spriteID, IRect rectPosition) { }
/// </code></example>
public class OnObjectBreak_IntSpriteID_IRectAttribute : EventAttribute { }


/// <summary>
/// The function will be called when an object start to free fall
/// </summary>
/// <example><code>
/// [OnObjectFreeFall_IntSpriteID_Int2Pos_IntRot_BoolFlip_Int2Velocity_IntRotSpeed_IntGravity]
/// internal static void ExampleFunction (int spriteID, Int2 pos, int startRotation, bool flip, Int2 velocity, int rotationSpeed, int gravity) { }
/// </code></example>
public class OnObjectFreeFall_IntSpriteID_Int2Pos_IntRot_BoolFlip_Int2Velocity_IntRotSpeed_IntGravityAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a map block has been picked
/// </summary>
/// <example><code>
/// [OnBlockPicked_IntSpriteID_IRect]
/// internal static void ExampleFunction (int spriteID, IRect rectPosition) { }
/// </code></example>
public class OnBlockPicked_IntSpriteID_IRectAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a rigidbody fall into water
/// </summary>
/// <example><code>
/// [OnFallIntoWater_Rigidbody_Entity]
/// internal static void ExampleFunction (Rigidbody rig, Entity water) { }
/// </code></example>
public class OnFallIntoWater_Rigidbody_EntityAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a rigidbody came out of water
/// </summary>
/// <example><code>
/// [OnCameOutOfWater_Rigidbody_Entity]
/// internal static void ExampleFunction (Rigidbody rig, Entity water) { }
/// </code></example>
public class OnCameOutOfWater_Rigidbody_EntityAttribute : EventAttribute { }


/// <summary>
/// The function will be called when something deal damage to a damage-receiver
/// </summary>
/// <example><code>
/// [OnDealDamage_Damage_IDamageReceiver]
/// internal static void ExampleFunction (Damage damage, IDamageReceiver receiver) { }
/// </code></example>
public class OnDealDamage_Damage_IDamageReceiverAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a bullet hit environment (something not an IDamageReceiver)
/// </summary>
/// <example><code>
/// [OnBulletHitEnvironment_Bullet]
/// internal static void ExampleFunction (Bullet bullet) { }
/// </code></example>
public class OnBulletHitEnvironment_BulletAttribute : EventAttribute { }


/// <summary>
/// The function will be called when message from TransferSystem.StartTransfer() arrived
/// </summary>
/// <example><code>
/// [OnTransferArrivedAttribute_IntEntityID_Int3UnitPos_ObjectData]
/// internal static void ExampleFunction (int receiverID, Int3 unitPosition, object userData) { }
/// </code></example>
public class OnTransferArrivedAttribute_IntEntityID_Int3UnitPos_ObjectDataAttribute : EventAttribute { }


/// <summary>
/// The function will be called when message from TransferSystem.StartTransfer() pass through
/// </summary>
/// <example><code>
/// [OnTransferPassAttribute_Int3UnitPos_ObjectData]
/// internal static void ExampleFunction (Int3 unitPos, object userData) { }
/// </code></example>
public class OnTransferPassAttribute_Int3UnitPos_ObjectDataAttribute : EventAttribute { }


/// <summary>
/// The function will be called when CircuitSystem triggers the entity that holds this function
/// </summary>
/// <example><code>
/// [CircuitOperate_Int3UnitPos_IntStamp_Direction5From]
/// internal static void ExampleFunction (Int3 unitPos, int stamp, Direction5 from) { }
/// </code></example>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CircuitOperate_Int3UnitPos_IntStamp_Direction5FromAttribute : Attribute { }


/// <summary>
/// The function will be called when CircuitSystem's electric current pass through
/// </summary>
/// <example><code>
/// [OnCircuitWireActived_Int3UnitPos]
/// internal static void ExampleFunction (Int3 unitPos) { }
/// </code></example>
public class OnCircuitWireActived_Int3UnitPosAttribute : EventAttribute { }




