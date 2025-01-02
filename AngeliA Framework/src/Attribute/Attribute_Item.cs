using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;



// Drop
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemDropAttribute (Type itemType, int dropCount = 1, int dropChance = 1000) : Attribute {
	public Type ItemType = itemType;
	public readonly int ItemTypeID = itemType.AngeHash();
	public int DropCount = dropCount;
	public int DropChance = dropChance;
}


// Class Combination
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute (
	int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraA = "", string extraB = "", string extraC = "", string extraD = ""
) : BasicItemCombinationAttribute(
	extraA, extraB, extraC, extraD, count, keepId0, keepId1, keepId2, keepId3
) { }



[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0> (
	int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraB = "", string extraC = "", string extraD = ""
) : BasicItemCombinationAttribute(
	typeof(I0), extraB, extraC, extraD, count, keepId0, keepId1, keepId2, keepId3
) { }



[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0, I1> (
	int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraC = "", string extraD = ""
) : BasicItemCombinationAttribute(
	typeof(I0), typeof(I1), extraC, extraD, count, keepId0, keepId1, keepId2, keepId3
) { }



[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0, I1, I2> (
	int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraD = ""
) : BasicItemCombinationAttribute(
	typeof(I0), typeof(I1), typeof(I2), extraD, count, keepId0, keepId1, keepId2, keepId3
) { }



[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0, I1, I2, I3> (
	int count = 1, string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = ""
) : BasicItemCombinationAttribute(
	typeof(I0), typeof(I1), typeof(I2), typeof(I3), count, keepId0, keepId1, keepId2, keepId3
) { }



// Global Combination
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalItemCombinationAttribute (
	string result, int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraA = "", string extraB = "", string extraC = "", string extraD = ""
) : BasicGlobalItemCombinationAttribute(
	extraA, extraB, extraC, extraD, result,
	count, keepId0, keepId1, keepId2, keepId3
) { }


[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalItemCombinationAttribute<I0> (
	string result, int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraB = "", string extraC = "", string extraD = ""
) : BasicGlobalItemCombinationAttribute(
	typeof(I0), extraB, extraC, extraD, result,
	count, keepId0, keepId1, keepId2, keepId3
) { }



[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalItemCombinationAttribute<I0, I1> (
	string result, int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraC = "", string extraD = ""
) : BasicGlobalItemCombinationAttribute(
	typeof(I0), typeof(I1), extraC, extraD, result,
	count, keepId0, keepId1, keepId2, keepId3
) { }



[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalItemCombinationAttribute<I0, I1, I2> (
	string result, int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraD = ""
) : BasicGlobalItemCombinationAttribute(
	typeof(I0), typeof(I1), typeof(I2), extraD, result,
	count, keepId0, keepId1, keepId2, keepId3
) { }



[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalItemCombinationAttribute<I0, I1, I2, I3> (
	string result, int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = ""
) : BasicGlobalItemCombinationAttribute(
	typeof(I0), typeof(I1), typeof(I2), typeof(I3), result,
	count, keepId0, keepId1, keepId2, keepId3
) { }



public abstract class BasicGlobalItemCombinationAttribute (
	ItemCombinationParam itemA, ItemCombinationParam itemB, ItemCombinationParam itemC, ItemCombinationParam itemD,
	string result, int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = ""
) : BasicItemCombinationAttribute(itemA, itemB, itemC, itemD, count, keepId0, keepId1, keepId2, keepId3) {
	public string Result = result;
}



// Basic Combination
public abstract class BasicItemCombinationAttribute (
	ItemCombinationParam itemA,
	ItemCombinationParam itemB,
	ItemCombinationParam itemC,
	ItemCombinationParam itemD,
	int count = 1, string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = ""
) : Attribute {
	public string ItemA = itemA;
	public string ItemB = itemB;
	public string ItemC = itemC;
	public string ItemD = itemD;
	public int Count = count;
	public int KeepId0 = Util.GetAngeHashForClassName(keepId0);
	public int KeepId1 = Util.GetAngeHashForClassName(keepId1);
	public int KeepId2 = Util.GetAngeHashForClassName(keepId2);
	public int KeepId3 = Util.GetAngeHashForClassName(keepId3);
}


public sealed class ItemCombinationParam {
	public string Name = "";
	public ItemCombinationParam (Type type) => Name = type.AngeName();
	public ItemCombinationParam (string name) => Name = name;
	public static implicit operator ItemCombinationParam (string name) => new(name);
	public static implicit operator ItemCombinationParam (Type type) => new(type);
	public static implicit operator string (ItemCombinationParam param) => param.Name;
	public static implicit operator int (ItemCombinationParam param) => param == null ? 0 : param.Name.AngeHash();
}



[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class NoItemCombinationAttribute : Attribute { }


