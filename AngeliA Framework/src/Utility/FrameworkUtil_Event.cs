using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static partial class FrameworkUtil {

	[OnObjectBreak_IntSpriteID_IRect] internal static Action<int, IRect> OnObjectBreak;
	[OnObjectFreeFall_IntSpriteID_Int2Pos_IntRot_BoolFlip_Int2Velocity_IntRotSpeed_IntGravity] internal static Action<int, Int2, int, bool, Int2, int, int> OnObjectFreeFall;
	[OnBlockPicked_IntSpriteID_IRect] internal static Action<int, IRect> OnBlockPicked;
	[OnFallIntoWater_Rigidbody_Entity] internal static Action<Rigidbody, Entity> OnFallIntoWater;
	[OnCameOutOfWater_Rigidbody_Entity] internal static Action<Rigidbody, Entity> OnCameOutOfWater;
	[OnItemCollected_Entity_Int2Pos_IntItemID_IntItemCount] internal static Action<Entity, Int2, int, int> OnItemCollected;
	[OnItemLost_Character_IntItemID] internal static Action<Character, int> OnItemLost;
	[OnItemError_Entity_Int2Pos_IntIconID] internal static Action<Entity, Int2, int> OnErrorHint;
	[OnItemDamage_Character_IntItemBefore_IntItemAfter] internal static Action<Character, int, int> OnItemDamage;
	[OnItemUnlocked_IntItemID] internal static Action<int> OnItemUnlocked;
	[OnCheatPerformed_StringCode] internal static Action<string> OnCheatPerformed;
	[OnCharacterFootStepped_Entity] internal static Action<Entity> OnFootStepped;
	[OnCharacterSleeping_Entity] internal static Action<Entity> OnCharacterSleeping;
	[OnCharacterJump_Entity] internal static Action<Entity> OnCharacterJump;
	[OnCharacterPound_Entity] internal static Action<Entity> OnCharacterPound;
	[OnCharacterFly_Entity] internal static Action<Entity> OnCharacterFly;
	[OnCharacterSlideStepped_Entity] internal static Action<Entity> OnCharacterSlideStepped;
	[OnCharacterPassOut_Entity] internal static Action<Entity> OnCharacterPassOut;
	[OnCharacterTeleport_Entity] internal static Action<Entity> OnCharacterTeleport;
	[OnCharacterCrash_Entity] internal static Action<Entity> OnCharacterCrash;

	// API
	/// <summary>
	/// Invoke function for object broke animation
	/// </summary>
	/// <param name="spriteID">Artwork sprite ID</param>
	/// <param name="rect">Starting rect position in global space</param>
	public static void InvokeObjectBreak (int spriteID, IRect rect) => OnObjectBreak?.Invoke(spriteID, rect);

	/// <summary>
	/// Invoke function for free fall animation
	/// </summary>
	/// <param name="spriteID">Artwork sprite ID</param>
	/// <param name="x">Start position X in global space</param>
	/// <param name="y">Start position Y in global space</param>
	/// <param name="speedX">Initial speed X</param>
	/// <param name="speedY">Initial speed Y</param>
	/// <param name="rotation">Initial rotation</param>
	/// <param name="rotationSpeed"></param>
	/// <param name="gravity"></param>
	/// <param name="flipX">True if artwork sprite flip horizontaly</param>
	public static void InvokeObjectFreeFall (int spriteID, int x, int y, int speedX = 0, int speedY = 0, int rotation = int.MinValue, int rotationSpeed = 0, int gravity = 5, bool flipX = false) => OnObjectFreeFall?.Invoke(spriteID, new(x, y), rotation, flipX, new(speedX, speedY), rotationSpeed, gravity);

	/// <summary>
	/// Invoke function for map block get picked
	/// </summary>
	/// <param name="spriteID">Artwork sprite ID</param>
	/// <param name="rect">Starting rect position in global space</param>
	public static void InvokeBlockPicked (int spriteID, IRect rect) => OnBlockPicked?.Invoke(spriteID, rect);
	
	/// <summary>
	/// Invoke function for object get into water
	/// </summary>
	/// <param name="rig">Target object</param>
	/// <param name="water">Water entity instance. Null if water is from map block</param>
	public static void InvokeFallIntoWater (Rigidbody rig, Entity water) => OnFallIntoWater?.Invoke(rig, water);

	/// <summary>
	/// Invoke function for object get out of water
	/// </summary>
	/// <param name="rig">Target object</param>
	/// <param name="water">Water entity instance. Null if water is from map block</param>
	public static void InvokeCameOutOfWater (Rigidbody rig, Entity water) => OnCameOutOfWater?.Invoke(rig, water);

	/// <summary>
	/// Invoke animation hint for error
	/// </summary>
	/// <param name="x">Position in global space</param>
	/// <param name="y">Position in global space</param>
	/// <param name="id">Artwork sprite ID for the icon inside</param>
	public static void InvokeErrorHint (int x, int y, int id) => OnErrorHint?.Invoke(null, new Int2(x, y), id);

	/// <summary>
	/// Invoke animation hint for error
	/// </summary>
	/// <param name="holder">Entity that get the error</param>
	/// <param name="id">Artwork sprite ID for the icon inside</param>
	public static void InvokeErrorHint (Entity holder, int id) {
		if (holder == null) return;
		OnErrorHint?.Invoke(holder, new(holder.X, holder.Y), id);
	}

	/// <summary>
	/// Invoke function for item get collected by a character
	/// </summary>
	/// <param name="id">ID of the item</param>
	/// <param name="x">Position in global space</param>
	/// <param name="y">Position in global space</param>
	/// <param name="count">How many items get collected</param>
	public static void InvokeItemCollected (int id, int x, int y, int count) => OnItemCollected?.Invoke(null, new Int2(x, y), id, count);

	/// <summary>
	/// Invoke function for item get collected by a character
	/// </summary>
	/// <param name="collector">Character that collect the item</param>
	/// <param name="id">ID of the item</param>
	/// <param name="count">How many items get collected</param>
	public static void InvokeItemCollected (Entity collector, int id, int count) => OnItemCollected?.Invoke(collector, new Int2(collector.X, collector.Y + Const.CEL * 2), id, count);
	
	/// <summary>
	/// Invoke function for character lost an item 
	/// </summary>
	/// <param name="holder">Character that lost the item</param>
	/// <param name="id">ID of the item</param>
	public static void InvokeItemLost (Character holder, int id) => OnItemLost?.Invoke(holder, id);
	
	/// <summary>
	/// Invoke function for item being damaged/broken
	/// </summary>
	/// <param name="holder">Character that own the item</param>
	/// <param name="fromID">ID of the item before it broke</param>
	/// <param name="toID">ID of the item after it broke</param>
	public static void InvokeItemDamage (Character holder, int fromID, int toID) => OnItemDamage?.Invoke(holder, fromID, toID);
	
	/// <summary>
	/// Invoke function for item being unlocked
	/// </summary>
	/// <param name="itemID">ID of the unlocked item</param>
	public static void InvokeItemUnlocked (int itemID) => OnItemUnlocked?.Invoke(itemID);
	
	/// <summary>
	/// Invoke function for a cheat code get performed
	/// </summary>
	/// <param name="cheatCode">The performed cheat code</param>
	public static void InvokeCheatPerformed (string cheatCode) => OnCheatPerformed?.Invoke(cheatCode);
	
	/// <summary>
	/// Invoke function for a character walks with a foot step
	/// </summary>
	public static void InvokeOnFootStepped (Entity target) => OnFootStepped?.Invoke(target);
	
	/// <summary>
	/// Invoke function for a character is sleeping
	/// </summary>
	public static void InvokeOnCharacterSleeping (Entity target) => OnCharacterSleeping?.Invoke(target);
	
	/// <summary>
	/// Invoke function for character jumps
	/// </summary>
	public static void InvokeOnCharacterJump (Entity target) => OnCharacterJump?.Invoke(target);

	/// <summary>
	/// Invoke function for character ground pound
	/// </summary>
	public static void InvokeOnCharacterPound (Entity target) => OnCharacterPound?.Invoke(target);

	/// <summary>
	/// Invoke function for character fly
	/// </summary>
	public static void InvokeOnCharacterFly (Entity target) => OnCharacterFly?.Invoke(target);

	/// <summary>
	/// Invoke function for character slide with a step
	/// </summary>
	public static void InvokeOnCharacterSlideStepped (Entity target) => OnCharacterSlideStepped?.Invoke(target);

	/// <summary>
	/// Invoke function for character get pass out
	/// </summary>
	public static void InvokeOnCharacterPassOut (Entity target) => OnCharacterPassOut?.Invoke(target);

	/// <summary>
	/// Invoke function for character perform a teleport
	/// </summary>
	public static void InvokeOnCharacterTeleport (Entity target) => OnCharacterTeleport?.Invoke(target);

	/// <summary>
	/// Invoke function for character crash
	/// </summary>
	public static void InvokeOnCharacterCrash (Entity target) => OnCharacterCrash?.Invoke(target);

}
