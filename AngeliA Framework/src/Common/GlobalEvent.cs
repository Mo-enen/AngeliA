using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static class GlobalEvent {


	// Obj Break
	public static event Action<int, IRect, bool> OnObjectBreak;
	public static void InvokeObjectBreak (int spriteID, IRect rect, bool lightWeight = false) => OnObjectBreak?.Invoke(spriteID, rect, lightWeight);

	// Free Fall
	public static event Action<int, Int4, Int4> OnObjectFreeFall;
	public static void InvokeObjectFreeFall (int spriteID, int x, int y, int speedX = 0, int speedY = 0, int rotation = int.MinValue, int rotationSpeed = 0, int gravity = 5, bool flipX = false) => OnObjectFreeFall?.Invoke(spriteID, new(x, y, rotation, flipX ? 1 : 0), new(speedX, speedY, rotationSpeed, gravity));

	// Dust
	public static event Action<int, IRect> OnBlockPicked;
	public static void InvokeBlockPicked (int spriteID, IRect rect) => OnBlockPicked?.Invoke(spriteID, rect);

	// Water
	public static event Action<Rigidbody> OnFallIntoWater;
	public static event Action<Rigidbody> OnCameOutOfWater;
	public static void InvokeFallIntoWater (Rigidbody rig) => OnFallIntoWater?.Invoke(rig);
	public static void InvokeCameOutOfWater (Rigidbody rig) => OnCameOutOfWater?.Invoke(rig);

	// Item
	public static event Action<Entity, int, int> OnItemCollected;
	public static void InvokeItemCollected (Entity collector, int id, int count) => OnItemCollected?.Invoke(collector, id, count);

	public static event Action<Character, int> OnItemLost;
	public static void InvokeItemLost (Character holder, int id) => OnItemLost?.Invoke(holder, id);

	public static event Action<Character, int> OnItemInsufficient;
	public static void InvokeItemInsufficient (Character holder, int id) => OnItemInsufficient?.Invoke(holder, id);

	public static event Action<Character, int, int> OnItemDamage;
	public static void InvokeItemDamage (Character holder, int fromID, int toID) => OnItemDamage?.Invoke(holder, fromID, toID);



}
