using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static partial class FrameworkUtil {

	[OnObjectBreak_IntSpriteID_IRect] internal static Action<int, IRect> OnObjectBreak;
	[OnObjectFreeFall_IntSpriteID_Int2Pos_IntRot_BoolFlip_Int2Velocity_IntRotSpeed_IntGravity] internal static Action<int, Int2, int, bool, Int2, int, int> OnObjectFreeFall;
	[OnBlockPicked_IntSpriteID_Irect] internal static Action<int, IRect> OnBlockPicked;
	[OnFallIntoWater_Rigidbody] internal static Action<Rigidbody> OnFallIntoWater;
	[OnCameOutOfWater_Rigidbody] internal static Action<Rigidbody> OnCameOutOfWater;
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
	public static void InvokeObjectBreak (int spriteID, IRect rect) => OnObjectBreak?.Invoke(spriteID, rect);
	public static void InvokeObjectFreeFall (int spriteID, int x, int y, int speedX = 0, int speedY = 0, int rotation = int.MinValue, int rotationSpeed = 0, int gravity = 5, bool flipX = false) => OnObjectFreeFall?.Invoke(spriteID, new(x, y), rotation, flipX, new(speedX, speedY), rotationSpeed, gravity);
	public static void InvokeBlockPicked (int spriteID, IRect rect) => OnBlockPicked?.Invoke(spriteID, rect);
	public static void InvokeFallIntoWater (Rigidbody rig) => OnFallIntoWater?.Invoke(rig);
	public static void InvokeCameOutOfWater (Rigidbody rig) => OnCameOutOfWater?.Invoke(rig);
	public static void InvokeErrorHint (int x, int y, int id) => OnErrorHint?.Invoke(null, new Int2(x, y), id);
	public static void InvokeErrorHint (Entity holder, int id) {
		if (holder == null) return;
		OnErrorHint?.Invoke(holder, new(holder.X, holder.Y), id);
	}
	public static void InvokeItemCollected (int id, int x, int y, int count) => OnItemCollected?.Invoke(null, new Int2(x, y), id, count);
	public static void InvokeItemCollected (Entity collector, int id, int count) => OnItemCollected?.Invoke(collector, new Int2(collector.X, collector.Y + Const.CEL * 2), id, count);
	public static void InvokeItemLost (Character holder, int id) => OnItemLost?.Invoke(holder, id);
	public static void InvokeItemDamage (Character holder, int fromID, int toID) => OnItemDamage?.Invoke(holder, fromID, toID);
	public static void InvokeItemUnlocked (int itemID) => OnItemUnlocked?.Invoke(itemID);
	public static void InvokeCheatPerformed (string cheatCode) => OnCheatPerformed?.Invoke(cheatCode);
	public static void InvokeOnFootStepped (Entity target) => OnFootStepped?.Invoke(target);
	public static void InvokeOnCharacterSleeping (Entity target) => OnCharacterSleeping?.Invoke(target);
	public static void InvokeOnCharacterJump (Entity target) => OnCharacterJump?.Invoke(target);
	public static void InvokeOnCharacterPound (Entity target) => OnCharacterPound?.Invoke(target);
	public static void InvokeOnCharacterFly (Entity target) => OnCharacterFly?.Invoke(target);
	public static void InvokeOnCharacterSlideStepped (Entity target) => OnCharacterSlideStepped?.Invoke(target);
	public static void InvokeOnCharacterPassOut (Entity target) => OnCharacterPassOut?.Invoke(target);
	public static void InvokeOnCharacterTeleport (Entity target) => OnCharacterTeleport?.Invoke(target);
	public static void InvokeOnCharacterCrash (Entity target) => OnCharacterCrash?.Invoke(target);

}
