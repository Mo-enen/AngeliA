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

	// Fire Putout
	public static event Action<int, IRect> OnFirePutout;
	public static void InvokeFirePutout (int fireID, IRect fireRect) => OnFirePutout?.Invoke(fireID, fireRect);


}
