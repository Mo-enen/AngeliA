using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute : Attribute {
	public Type ItemA = null;
	public Type ItemB = null;
	public Type ItemC = null;
	public Type ItemD = null;
	public int Count = 1;
	public bool ConsumeA = true;
	public bool ConsumeB = true;
	public bool ConsumeC = true;
	public bool ConsumeD = true;
	public ItemCombinationAttribute (Type itemA, int count = 1, bool consumeA = true) : this(itemA, null, null, null, count, consumeA, true, true, true) { }
	public ItemCombinationAttribute (Type itemA, Type itemB, int count = 1, bool consumeA = true, bool consumeB = true) : this(itemA, itemB, null, null, count, consumeA, consumeB, true, true) { }
	public ItemCombinationAttribute (Type itemA, Type itemB, Type itemC, int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true) : this(itemA, itemB, itemC, null, count, consumeA, consumeB, consumeC, true) { }
	public ItemCombinationAttribute (Type itemA, Type itemB, Type itemC, Type itemD, int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true) {
		ItemA = itemA;
		ItemB = itemB;
		ItemC = itemC;
		ItemD = itemD;
		Count = count;
		ConsumeA = consumeA;
		ConsumeB = consumeB;
		ConsumeC = consumeC;
		ConsumeD = consumeD;
	}
}



[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemDropAttribute (Type itemType, int dropCount = 1, int dropChance = 1000) : Attribute {
	public Type ItemType = itemType;
	public readonly int ItemTypeID = itemType.AngeHash();
	public int DropCount = dropCount;
	public int DropChance = dropChance;
}





