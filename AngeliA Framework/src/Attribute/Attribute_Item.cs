using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public sealed class ItemCombinationParam {
	public string Name;
	public ItemCombinationParam (Type type) => Name = type.AngeName();
	public ItemCombinationParam (string name) => Name = name;
	public static implicit operator ItemCombinationParam (string name) => new(name);
	public static implicit operator string (ItemCombinationParam param) => param.Name;
	public static implicit operator ItemCombinationParam (Type type) => new(type);
}




// Combination
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute (
	int count = 1,
	bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true,
	string extraA = "", string extraB = "", string extraC = "", string extraD = ""
) : BasicItemCombinationAttribute(
	extraA, extraB, extraC, extraD, count, consumeA, consumeB, consumeC, consumeD
) { }



[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0> (
	int count = 1,
	bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true,
	string extraB = "", string extraC = "", string extraD = ""
) : BasicItemCombinationAttribute(
	typeof(I0), extraB, extraC, extraD, count, consumeA, consumeB, consumeC, consumeD
) { }



[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0, I1> (
	int count = 1,
	bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true,
	string extraC = "", string extraD = ""
) : BasicItemCombinationAttribute(
	typeof(I0), typeof(I1), extraC, extraD, count, consumeA, consumeB, consumeC, consumeD
) { }



[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0, I1, I2> (
	int count = 1,
	bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true,
	string extraD = ""
) : BasicItemCombinationAttribute(
	typeof(I0), typeof(I1), typeof(I2), extraD, count, consumeA, consumeB, consumeC, consumeD
) { }



[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0, I1, I2, I3> (
	int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true
) : BasicItemCombinationAttribute(
	typeof(I0), typeof(I1), typeof(I2), typeof(I3), count, consumeA, consumeB, consumeC, consumeD
) { }



public abstract class BasicItemCombinationAttribute (
	ItemCombinationParam itemA,
	ItemCombinationParam itemB,
	ItemCombinationParam itemC,
	ItemCombinationParam itemD,
	int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true
) : Attribute {
	public string ItemA = itemA;
	public string ItemB = itemB;
	public string ItemC = itemC;
	public string ItemD = itemD;
	public int Count = count;
	public bool ConsumeA = consumeA;
	public bool ConsumeB = consumeB;
	public bool ConsumeC = consumeC;
	public bool ConsumeD = consumeD;
}



// Global Combination
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalItemCombinationAttribute (
	string result, int count = 1,
	bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true,
	string extraA = "", string extraB = "", string extraC = "", string extraD = ""
) : BasicGlobalItemCombinationAttribute(
	extraA, extraB, extraC, extraD, result,
	count, consumeA, consumeB, consumeC, consumeD
) { }


[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalItemCombinationAttribute<I0> (
	string result, int count = 1,
	bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true,
	string extraB = "", string extraC = "", string extraD = ""
) : BasicGlobalItemCombinationAttribute(
	typeof(I0), extraB, extraC, extraD, result,
	count, consumeA, consumeB, consumeC, consumeD
) { }



[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalItemCombinationAttribute<I0, I1> (
	string result, int count = 1,
	bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true,
	string extraC = "", string extraD = ""
) : BasicGlobalItemCombinationAttribute(
	typeof(I0), typeof(I1), extraC, extraD, result,
	count, consumeA, consumeB, consumeC, consumeD
) { }



[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalItemCombinationAttribute<I0, I1, I2> (
	string result, int count = 1,
	bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true,
	string extraD = ""
) : BasicGlobalItemCombinationAttribute(
	typeof(I0), typeof(I1), typeof(I2), extraD, result,
	count, consumeA, consumeB, consumeC, consumeD
) { }



[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalItemCombinationAttribute<I0, I1, I2, I3> (
	string result, int count = 1,
	bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true

) : BasicGlobalItemCombinationAttribute(
	typeof(I0), typeof(I1), typeof(I2), typeof(I3), result,
	count, consumeA, consumeB, consumeC, consumeD
) { }



public abstract class BasicGlobalItemCombinationAttribute (
	ItemCombinationParam itemA, ItemCombinationParam itemB, ItemCombinationParam itemC, ItemCombinationParam itemD,
	string result, int count = 1,
	bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true
) : BasicItemCombinationAttribute(itemA, itemB, itemC, itemD, count, consumeA, consumeB, consumeC, consumeD) {
	public string Result = result;
}



// Drop
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemDropAttribute (Type itemType, int dropCount = 1, int dropChance = 1000) : Attribute {
	public Type ItemType = itemType;
	public readonly int ItemTypeID = itemType.AngeHash();
	public int DropCount = dropCount;
	public int DropChance = dropChance;
}



