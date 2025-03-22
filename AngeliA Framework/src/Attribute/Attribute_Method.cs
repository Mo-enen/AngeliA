using System;
using System.Collections;

namespace AngeliA;


// Init
/// <summary>
/// The function will be called when game initialize.
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnGameInitializeAttribute (int order = 0) : OrderedAttribute(order) { }


/// <summary>
/// The function will be called when game initialize but after all [OnGameInitialize] functions already called
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnGameInitializeLaterAttribute (int order = 0) : OrderedAttribute(order) { }


// Slot
/// <summary>
/// The function will be called before user change the saving slot. Universe.BuiltIn.CurrentSavingSlot is still the old value when this function is called.
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class BeforeSavingSlotChanged (int order = 0) : OrderedAttribute(order) { }

/// <summary>
/// The function will be called after user change the saving slot. Universe.BuiltIn.CurrentSavingSlot is the new value when this function is called.
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnSavingSlotChanged (int order = 0) : OrderedAttribute(order) { }


// Game Update
/// <summary>
/// The function will be called every time game update (60 times per second)
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnGameUpdateAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called every time game update (60 times per second), but after all [OnGameUpdate] functions already called.
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnGameUpdateLaterAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called every time game update (60 times per second), even when the game is paused.
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnGameUpdatePauselessAttribute (int order = 0) : EventAttribute(order) { }


// Game Misc
/// <summary>
/// The function will be called when game restart.
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnGameRestartAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called when player try to quit the game. Return false will stop the application from quiting.
/// </summary>
/// <example>internal static bool ExampleFunction () => true; </example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnGameTryingToQuitAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called before the application actually quit.
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
/// <param name="order">Function with smaller order will be called earlier</param>
public class OnGameQuittingAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called when the application window regain focus.
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class OnGameFocusedAttribute : EventAttribute { }


/// <summary>
/// The function will be called when the application window lost focus.
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class OnGameLostFocusAttribute : EventAttribute { }


// System
/// <summary>
/// The function will be called when user change the size of the application window
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class OnWindowSizeChangedAttribute : EventAttribute { }


/// <summary>
/// The function will be called when engine send remote message to rigged game
/// </summary>
/// <example>internal static void ExampleFunction (int id, int data) { }</example>
public class OnRemoteSettingChanged_IntID_IntDataAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called when user drag and drop a file into the application window
/// </summary>
/// <example>internal static void ExampleFunction (string filePath) { }</example>
public class OnFileDropped_StringPathAttribute : EventAttribute { }


// Rendering
/// <summary>
/// The function will be called when artwork sheet for rendering loaded from file
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class OnMainSheetReloadAttribute : EventAttribute { }


// Stage
/// <summary>
/// The function will be called when player change the map layer
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class OnViewZChangedAttribute : EventAttribute { }


/// <summary>
/// The function will be called before rendering layer update
/// </summary>
/// <example>internal static void ExampleFunction (int layerIndex) { }</example>
public class BeforeLayerFrameUpdate_IntLayerAttribute : EventAttribute { }


/// <summary>
/// The function will be called after rendering layer update 
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class AfterLayerFrameUpdate_IntLayerAttribute : EventAttribute { }


/// <summary>
/// The function will be called before any entity.FirstUpdate is called
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class BeforeFirstUpdateAttribute : EventAttribute { }


/// <summary>
/// The function will be called before any entity.BeforeUpdate is called 
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class BeforeBeforeUpdateAttribute : EventAttribute { }


/// <summary>
/// The function will be called before any entity.Update is called 
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class BeforeUpdateUpdateAttribute : EventAttribute { }


/// <summary>
/// The function will be called before any entity.LateUpdate is called 
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class BeforeLateUpdateAttribute : EventAttribute { }


/// <summary>
/// The function will be called after all entity.LateUpdate is called 
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class AfterLateUpdateAttribute : EventAttribute { }


// World
/// <summary>
/// The function will be called before world squad rendering any level blocks for the current frame
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class BeforeLevelRenderedAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called after world squad render all level blocks for the current frame 
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class AfterLevelRenderedAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called when a world instance is created by world squad
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class OnWorldCreatedBySquad_WorldAttribute (int order = 0) : EventAttribute(order) { }


/// <summary>
/// The function will be called when a world instance is loaded by world squad 
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class OnWorldLoadedBySquad_WorldAttribute (int order = 0) : EventAttribute(order) { }


// Map Editor
/// <summary>
/// The function will be called when a world instance is saved to file by the world squad
/// </summary>
/// <example>internal static void ExampleFunction () { }</example>
public class OnWorldSavedByMapEditor_WorldAttribute : EventAttribute { }


/// <summary>
/// The function will be called when user change map editor editing mode
/// </summary>
/// <example>internal static void ExampleFunction (OnMapEditorModeChange_ModeAttribute.Mode mode) { }</example>
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
/// <example>internal static void ExampleFunction () { }</example>
public class OnLanguageChangedAttribute : EventAttribute { }


// Cheat
/// <summary>
/// The function will be called when user perform the given cheat code. Cheat code can be perform like those in NES games.
/// </summary>
/// <example>internal static void ExampleFunction (string code) { }</example>
public class CheatCodeAttribute (string code) : EventAttribute {
	internal string Code = code;
}


/// <summary>
/// The function will be called when user performed any cheat code. Cheat code can be perform like those in NES games.
/// </summary>
/// <example>internal static void ExampleFunction (string code) { }</example>
public class OnCheatPerformed_StringCodeAttribute : EventAttribute { }


// Item 
/// <summary>
/// The function will be called when a character collect an item
/// </summary>
/// <example>internal static void ExampleFunction (Entity holder, Int2 pos, int itemID, int itemCount) { }</example>
public class OnItemCollected_Entity_Int2Pos_IntItemID_IntItemCountAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a character lost an item
/// </summary>
/// <example>internal static void ExampleFunction (Character holder, int itemID) { }</example>
public class OnItemLost_Character_IntItemIDAttribute : EventAttribute { }


/// <summary>
/// The function will be called when something wrong about an item (like when guns out of ammo)
/// </summary>
/// <example>internal static void ExampleFunction (Entity holder, Int2 pos, int iconID) { }</example>
public class OnItemError_Entity_Int2Pos_IntIconID : EventAttribute { }


/// <summary>
/// The function will be called when an item is damaged into another item
/// </summary>
/// <example>internal static void ExampleFunction (Character holder, int itemIdBefore, int itemIdAfter) { }</example>
public class OnItemDamage_Character_IntItemBefore_IntItemAfterAttribute : EventAttribute { }


/// <summary>
/// The function will be called when an item is unlocked by player
/// </summary>
/// <example>internal static void ExampleFunction (int itemID) { }</example>
public class OnItemUnlocked_IntItemIDAttribute : EventAttribute { }


// Character
/// <summary>
/// The function will be called when a character makes a foot step on running
/// </summary>
/// <example>internal static void ExampleFunction (Entity character) { }</example>
public class OnCharacterFootStepped_Entity : EventAttribute { }


/// <summary>
/// The function will be called when a character is sleeping
/// </summary>
/// <example>internal static void ExampleFunction (Entity character) { }</example>
public class OnCharacterSleeping_Entity : EventAttribute { }


/// <summary>
/// The function will be called when a character jumps
/// </summary>
/// <example>internal static void ExampleFunction (Entity character) { }</example>
public class OnCharacterJump_Entity : EventAttribute { }


/// <summary>
/// The function will be called when a character ground pound
/// </summary>
/// <example>internal static void ExampleFunction (Entity character) { }</example>
public class OnCharacterPound_Entity : EventAttribute { }


/// <summary>
/// The function will be called when a character fly
/// </summary>
/// <example>internal static void ExampleFunction (Entity character) { }</example>
public class OnCharacterFly_Entity : EventAttribute { }


/// <summary>
/// The function will be called when a character makes a step when sliding
/// </summary>
/// <example>internal static void ExampleFunction (Entity character) { }</example>
public class OnCharacterSlideStepped_Entity : EventAttribute { }


/// <summary>
/// The function will be called when a character pass out
/// </summary>
/// <example>internal static void ExampleFunction (Entity character) { }</example>
public class OnCharacterPassOut_Entity : EventAttribute { }


/// <summary>
/// The function will be called when a character teleport
/// </summary>
/// <example>internal static void ExampleFunction (Entity character) { }</example>
public class OnCharacterTeleport_Entity : EventAttribute { }


/// <summary>
/// The function will be called when a character crash
/// </summary>
/// <example>internal static void ExampleFunction (Entity character) { }</example>
public class OnCharacterCrash_Entity : EventAttribute { }


// Misc
/// <summary>
/// The function will be called when an object break
/// </summary>
/// <example>internal static void ExampleFunction (int spriteID, IRect rectPosition) { }</example>
public class OnObjectBreak_IntSpriteID_IRectAttribute : EventAttribute { }


/// <summary>
/// The function will be called when an object start to free fall
/// </summary>
/// <example>internal static void ExampleFunction (int spriteID, Int2 pos, int startRotation, bool flip, Int2 velocity, int rotationSpeed, int gravity) { }</example>
public class OnObjectFreeFall_IntSpriteID_Int2Pos_IntRot_BoolFlip_Int2Velocity_IntRotSpeed_IntGravityAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a map block has been picked
/// </summary>
/// <example>internal static void ExampleFunction (int spriteID, IRect rectPosition) { }</example>
public class OnBlockPicked_IntSpriteID_IRectAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a rigidbody fall into water
/// </summary>
/// <example>internal static void ExampleFunction (Rigidbody rig, Entity water) { }</example>
public class OnFallIntoWater_Rigidbody_EntityAttribute : EventAttribute { }


/// <summary>
/// The function will be called when a rigidbody came out of water
/// </summary>
/// <example>internal static void ExampleFunction (Rigidbody rig, Entity water) { }</example>
public class OnCameOutOfWater_Rigidbody_EntityAttribute : EventAttribute { }


/// <summary>
/// The function will be called when something deal damage to a damage-receiver
/// </summary>
/// <example>internal static void ExampleFunction (Damage damage, IDamageReceiver receiver) { }</example>
public class OnDealDamage_Damage_IDamageReceiver : EventAttribute { }


/// <summary>
/// The function will be called when a bullet hit environment (something not an IDamageReceiver)
/// </summary>
/// <example>internal static void ExampleFunction (Bullet bullet) { }</example>
public class OnBulletHitEnvironment_Bullet : EventAttribute { }


/// <summary>
/// The function will be called when message from TransferSystem.StartTransfer() arrived
/// </summary>
/// <example>internal static void ExampleFunction (int receiverID, Int3 unitPosition, object userData) { }</example>
public class OnTransferArrivedAttribute_IntEntityID_Int3UnitPos_ObjectData : EventAttribute { }


/// <summary>
/// The function will be called when message from TransferSystem.StartTransfer() pass through
/// </summary>
/// <example>internal static void ExampleFunction (Int3 unitPos, object userData) { }</example>
public class OnTransferPassAttribute_Int3UnitPos_ObjectData : EventAttribute { }


/// <summary>
/// The function will be called when CircuitSystem triggers the entity that holds this function
/// </summary>
/// <example>internal static void ExampleFunction (Int3 unitPos, int stamp, Direction5 from) { }</example>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CircuitOperate_Int3UnitPos_IntStamp_Direction5From : Attribute { }


/// <summary>
/// The function will be called when CircuitSystem's electric current pass through
/// </summary>
/// <example>internal static void ExampleFunction (Int3 unitPos) { }</example>
public class OnCircuitWireActived_Int3UnitPosAttribute : EventAttribute { }




