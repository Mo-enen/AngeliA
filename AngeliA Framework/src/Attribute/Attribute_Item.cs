using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


// Drop
/// <summary>
/// Register item drop for the entity, use ItemSystem.DropItemFor(Entity) to perform the item drop.
/// </summary>
/// <typeparam name="I">Which item does it drops</typeparam>
/// <param name="dropChance">Probability of dropping this item. 0 means 0%, 1000 means 100%</param>
/// <param name="dropCount">How many items does it drop at once</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemDropAttribute<I> (int dropChance, int dropCount = 1) : ItemDropAttribute(typeof(I).AngeHash(), dropChance, dropCount) { }


/// <inheritdoc cref="ItemDropAttribute{I}" />
public abstract class ItemDropAttribute (int typeID, int dropChance, int dropCount = 1) : Attribute {
	internal readonly int ItemTypeID = typeID;
	internal int DropCount = dropCount;
	internal int DropChance = dropChance;
}


// Class Combination
/// <inheritdoc cref="BasicItemCombinationAttribute" />
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute (
	int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraA = "", string extraB = "", string extraC = "", string extraD = ""
) : BasicItemCombinationAttribute(
	extraA, extraB, extraC, extraD, count, keepId0, keepId1, keepId2, keepId3
) { }



/// <inheritdoc cref="BasicItemCombinationAttribute" />
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0> (
	int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraB = "", string extraC = "", string extraD = ""
) : BasicItemCombinationAttribute(
	typeof(I0), extraB, extraC, extraD, count, keepId0, keepId1, keepId2, keepId3
) { }



/// <inheritdoc cref="BasicItemCombinationAttribute" />
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0, I1> (
	int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraC = "", string extraD = ""
) : BasicItemCombinationAttribute(
	typeof(I0), typeof(I1), extraC, extraD, count, keepId0, keepId1, keepId2, keepId3
) { }



/// <inheritdoc cref="BasicItemCombinationAttribute" />
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0, I1, I2> (
	int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraD = ""
) : BasicItemCombinationAttribute(
	typeof(I0), typeof(I1), typeof(I2), extraD, count, keepId0, keepId1, keepId2, keepId3
) { }



/// <inheritdoc cref="BasicItemCombinationAttribute" />
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute<I0, I1, I2, I3> (
	int count = 1, string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = ""
) : BasicItemCombinationAttribute(
	typeof(I0), typeof(I1), typeof(I2), typeof(I3), count, keepId0, keepId1, keepId2, keepId3
) { }



// Global Combination
/// <inheritdoc cref="BasicItemCombinationAttribute" />
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalItemCombinationAttribute (
	string result, int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraA = "", string extraB = "", string extraC = "", string extraD = ""
) : BasicGlobalItemCombinationAttribute(
	extraA, extraB, extraC, extraD, result,
	count, keepId0, keepId1, keepId2, keepId3
) { }


/// <inheritdoc cref="BasicItemCombinationAttribute" />
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalItemCombinationAttribute<I0> (
	string result, int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraB = "", string extraC = "", string extraD = ""
) : BasicGlobalItemCombinationAttribute(
	typeof(I0), extraB, extraC, extraD, result,
	count, keepId0, keepId1, keepId2, keepId3
) { }



/// <inheritdoc cref="BasicItemCombinationAttribute" />
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalItemCombinationAttribute<I0, I1> (
	string result, int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraC = "", string extraD = ""
) : BasicGlobalItemCombinationAttribute(
	typeof(I0), typeof(I1), extraC, extraD, result,
	count, keepId0, keepId1, keepId2, keepId3
) { }



/// <inheritdoc cref="BasicItemCombinationAttribute" />
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalItemCombinationAttribute<I0, I1, I2> (
	string result, int count = 1,
	string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = "",
	string extraD = ""
) : BasicGlobalItemCombinationAttribute(
	typeof(I0), typeof(I1), typeof(I2), extraD, result,
	count, keepId0, keepId1, keepId2, keepId3
) { }



/// <inheritdoc cref="BasicItemCombinationAttribute" />
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
	internal string Result = result;
}



// Basic Combination
#pragma warning disable CS1572
/// <summary>
/// Define an item combination to craft the item.
/// </summary>
/// <param name="count">How many item does it craft at once</param>
/// <param name="keepId0">Do not consume this item</param>
/// <param name="keepId1">Do not consume this item</param>
/// <param name="keepId2">Do not consume this item</param>
/// <param name="keepId3">Do not consume this item</param>
/// <param name="extraA">Use this if the item is not based on class</param>
/// <param name="extraB">Use this if the item is not based on class</param>
/// <param name="extraC">Use this if the item is not based on class</param>
/// <param name="extraD">Use this if the item is not based on class</param>
/// <param name="itemA">An item required to craft the result item</param>
/// <param name="itemB">An item required to craft the result item</param>
/// <param name="itemC">An item required to craft the result item</param>
/// <param name="itemD">An item required to craft the result item</param>
#pragma warning restore CS1572
public abstract class BasicItemCombinationAttribute (
	ItemCombinationParam itemA,
	ItemCombinationParam itemB,
	ItemCombinationParam itemC,
	ItemCombinationParam itemD,
	int count = 1, string keepId0 = "", string keepId1 = "", string keepId2 = "", string keepId3 = ""
) : Attribute {
	internal string ItemA = itemA;
	internal string ItemB = itemB;
	internal string ItemC = itemC;
	internal string ItemD = itemD;
	internal int Count = count;
	internal int KeepId0 = Util.GetAngeHashForClassName(keepId0);
	internal int KeepId1 = Util.GetAngeHashForClassName(keepId1);
	internal int KeepId2 = Util.GetAngeHashForClassName(keepId2);
	internal int KeepId3 = Util.GetAngeHashForClassName(keepId3);
}



public sealed class ItemCombinationParam {
	internal string Name = "";
	internal ItemCombinationParam (Type type) => Name = type.AngeName();
	internal ItemCombinationParam (string name) => Name = name;
#pragma warning disable CS1591
	public static implicit operator ItemCombinationParam (string name) => new(name);
	public static implicit operator ItemCombinationParam (Type type) => new(type);
	public static implicit operator string (ItemCombinationParam param) => param.Name;
	public static implicit operator int (ItemCombinationParam param) => param == null ? 0 : param.Name.AngeHash();
#pragma warning restore CS1591
}


/// <summary>
/// Indicates to the system that this item does not require crafting combination
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class NoItemCombinationAttribute : Attribute { }


