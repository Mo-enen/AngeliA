using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;



[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0> (
	int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true
) : ItemCombinationAttribute(
	typeof(I0), null, null, null, count, consumeA, consumeB, consumeC, consumeD
) { }


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0, I1> (
	int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true
) : ItemCombinationAttribute(
	typeof(I0), typeof(I1), null, null, count, consumeA, consumeB, consumeC, consumeD
) { }


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0, I1, I2> (
	int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true
) : ItemCombinationAttribute(
	typeof(I0), typeof(I1), typeof(I2), null, count, consumeA, consumeB, consumeC, consumeD
) { }


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0, I1, I2, I3> (
	int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true
) : ItemCombinationAttribute(
	typeof(I0), typeof(I1), typeof(I2), typeof(I3), count, consumeA, consumeB, consumeC, consumeD
) { }


public abstract class ItemCombinationAttribute (
	Type itemA, Type itemB, Type itemC, Type itemD, int count = 1,
	bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true
) : Attribute {
	public Type ItemA = itemA;
	public Type ItemB = itemB;
	public Type ItemC = itemC;
	public Type ItemD = itemD;
	public int Count = count;
	public bool ConsumeA = consumeA;
	public bool ConsumeB = consumeB;
	public bool ConsumeC = consumeC;
	public bool ConsumeD = consumeD;
}



[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemDropAttribute (Type itemType, int dropCount = 1, int dropChance = 1000) : Attribute {
	public Type ItemType = itemType;
	public readonly int ItemTypeID = itemType.AngeHash();
	public int DropCount = dropCount;
	public int DropChance = dropChance;
}





