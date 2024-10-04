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
	public static void InvokeOnFallIntoWater (Rigidbody rig) => OnFallIntoWater?.Invoke(rig);
	public static void InvokeOnCameOutOfWater (Rigidbody rig) => OnCameOutOfWater?.Invoke(rig);

	// Fire
	public static event Action<IRect> RequirePutoutFire;
	public static void InvokeRequirePutoutFire (IRect rect) => RequirePutoutFire?.Invoke(rect);


}
